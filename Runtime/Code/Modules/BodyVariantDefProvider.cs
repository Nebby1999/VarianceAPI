using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// Represents a collection of VariantDefs for a specific body.
    /// </summary>
    public class BodyVariantDefProvider
    {
        internal static List<BodyVariantDefProvider> instances = new List<BodyVariantDefProvider>();
        [SystemInitializer]
        private static void SystemInitializer()
        {
            Stage.onStageStartGlobal += (_) =>
            {
                OnStageStartGlobal(SceneInfo.instance, Run.instance);
            };
        }

        /// <summary>
        /// Forces all the BodyVariantProviders to filter their current variants depending on the sceneInfo and Run provided.
        /// This shouldnt be called unless you know what youre doing
        /// </summary>
        public static void FilterVariants(SceneInfo sceneInfo, Run run)
        {
            OnStageStartGlobal(sceneInfo, run);
        }

        private static void OnStageStartGlobal(SceneInfo info, Run run)
        {
            if (!info || !run)
            {
                VAPILog.Error($"A Stage ({Stage.instance.sceneDef.baseSceneNameOverride}) has started, but there is no Run And/Or SceneInfo instances! Variants will not be filtered." +
                    $"\n(Run: {run}, SceneInfo: {info}");
                return;
            }

            DirectorAPI.StageInfo stageInfo = DirectorAPI.StageInfo.ParseInternalStageName(info.sceneDef.baseSceneName);
            ExpansionDef[] enabledExpansions = ExpansionCatalog._expansionDefs.Where(ed => run.IsExpansionEnabled(ed)).ToArray();

            foreach (BodyVariantDefProvider provider in instances)
                provider.FilterVariants(stageInfo, enabledExpansions, run.ruleBook);
        }
        /// <summary>
        /// Finds a BodyVariantDefProvider using a string
        /// </summary>
        /// <param name="masterOrCharacterName">The CharacterMaster or CharacterBody name to use for the search</param>
        /// <returns>The BodyVariantDefProvider, null if it doesnt exist</returns>
        public static BodyVariantDefProvider FindProvider(string masterOrCharacterName)
        {
            var index = MasterCatalog.FindMasterIndex(masterOrCharacterName);
            if (index == MasterCatalog.MasterIndex.none)
            {
                BodyIndex bodyIndex = BodyCatalog.FindBodyIndex(masterOrCharacterName);
                if (bodyIndex == BodyIndex.None)
                {
                    return null;
                }
                return FindProvider(bodyIndex);
            }

            return FindProvider(index);
        }
        /// <summary>
        /// Finds the specified prefab's BodyVariantDefProvider, if it exists
        /// </summary>
        /// <param name="prefab">The prefab to use for the search</param>
        /// <returns>The prefab's BodyVariantDefProvider, null if it doesnt exist</returns>
        public static BodyVariantDefProvider FindProvider(GameObject prefab)
        {
            CharacterMaster master = prefab.GetComponent<CharacterMaster>();
            CharacterBody body = prefab.GetComponent<CharacterBody>();
            if (master)
                return FindProvider(master);

            return body ? FindProvider(body) : null;
        }
        /// <summary>
        /// Finds the specified MasterIndex's BodyVariantDefProvider, if it exists
        /// </summary>
        /// <param name="masterIndex">The master index to use for the search</param>
        /// <returns>The masterIndex's BodyVariantDefProvider, null if it doesnt exist</returns>
        public static BodyVariantDefProvider FindProvider(MasterCatalog.MasterIndex masterIndex)
        {
            var characterMaster = MasterCatalog.GetMasterPrefab(masterIndex).GetComponent<CharacterMaster>();
            return FindProvider(characterMaster);
        }
        /// <summary>
        /// Finds the specified CharacterMaster's BodyVariantDefProvider, if it exists
        /// </summary>
        /// <param name="master">The master to use for the search</param>
        /// <returns>The bodyInstance or the master's body prefab's BodyVariantDefProvider, null if it doesnt exist</returns>
        public static BodyVariantDefProvider FindProvider(CharacterMaster master)
        {
            if (master.hasBody)
            {
                return FindProvider(master.bodyInstanceObject.GetComponent<CharacterBody>());
            }
            return master.bodyPrefab ? FindProvider(master.bodyPrefab.GetComponent<CharacterBody>()) : null;
        }
        /// <summary>
        /// Finds the specified CharacterBody's BodyVariantDefProvider, if it exists
        /// </summary>
        /// <param name="body">The CharacterBody to use for the search</param>
        /// <returns>The CharacterBody's BodyVariantDefProvider, null if it doesnt exist</returns>
        public static BodyVariantDefProvider FindProvider(CharacterBody body)
        {
            VariantCatalog.ThrowIfNotInitialized();
            return VariantCatalog.GetBodyVariantDefProvider(body.bodyIndex);
        }
        /// <summary>
        /// Finds a BodyVariantDefProvider tied to the specified BodyIndex
        /// </summary>
        /// <param name="index">The BodyIndex to use for the search</param>
        /// <returns>The BodyIndex's BodyVariantDefProvider, null if it doesnt exist</returns>
        public static BodyVariantDefProvider FindProvider(BodyIndex index)
        {
            VariantCatalog.ThrowIfNotInitialized();
            return VariantCatalog.GetBodyVariantDefProvider(index);
        }

        private VariantDef[] variantsForBody = Array.Empty<VariantDef>();
        private VariantDef[] filteredUniques = Array.Empty<VariantDef>();
        private VariantDef[] filteredNonUniques = Array.Empty<VariantDef>();

        /// <summary>
        /// The BodyIndex that's tied to this BodyVariantDefProvider
        /// </summary>
        public BodyIndex TiedIndex { get; internal set; }
        /// <summary>
        /// The total amount of variants that this body has
        /// </summary>
        public int TotalVariantCount { get => variantsForBody.Length; }

        private void FilterVariants(DirectorAPI.StageInfo stageInfo, ExpansionDef[] runExpansions, RuleBook runRulebook)
        {
            try
            {
                filteredUniques = variantsForBody.Where(vd => vd.isUnique)
                    .Where(vd => vd.IsAvailable(stageInfo, runExpansions, runRulebook))
                    .ToArray();

                filteredNonUniques = variantsForBody.Where(vd => !vd.isUnique)
                    .Where(vd => vd.IsAvailable(stageInfo, runExpansions, runRulebook))
                    .ToArray();
            }
            catch (Exception e)
            {
                VAPILog.Error($"Could not filter variants for body {BodyCatalog.GetBodyName(TiedIndex)}: {e}");
            }
        }
        /// <summary>
        /// Returns all the unique variants for this body
        /// </summary>
        /// <param name="filtered">Wether to return the per stage filtered collection</param>
        /// <returns>An array of the Body's unique variants</returns>
        public VariantDef[] GetUniqueVariants(bool filtered)
        {
            return filtered ? filteredUniques : variantsForBody.Where(vd => vd.isUnique).ToArray();
        }

        /// <summary>
        /// Returns all the non unique variants for this body
        /// </summary>
        /// <param name="filtered">Wether to return the per stage filtered collection</param>
        /// <returns>An array of the Body's non unique variants</returns>
        public VariantDef[] GetVariants(bool filtered)
        {
            return filtered ? filteredNonUniques : variantsForBody.Where(vd => !vd.isUnique).ToArray();
        }

        /// <summary>
        /// Returns all the variants for this body
        /// </summary>
        /// <param name="filtered">Wether to return the per stage filtered collection of non unique and unique variants</param>
        /// <returns>An array of the Body's non unique variants</returns>
        public VariantDef[] GetAllVariants(bool filtered)
        {
            return filtered ? filteredUniques.Concat(filteredNonUniques).ToArray() : variantsForBody;
        }

        /// <summary>
        /// Get the tied body prefab's name
        /// </summary>
        /// <returns>the tied body prefab's name</returns>
        public string GetBodyName()
        {
            GameObject prefab = BodyCatalog.GetBodyPrefab(TiedIndex);
            return prefab ? prefab.name : null;
        }

        /// <summary>
        /// Gets the variantDef at index <paramref name="index"/>
        /// </summary>
        /// <param name="index">The index to retireve</param>
        /// <returns>The VariantDef, null if its out of range</returns>
        public VariantDef GetVariantDef(int index) => HG.ArrayUtils.GetSafe(variantsForBody, index);
        /// <summary>
        /// BodyVariantDefProvider constructor
        /// </summary>
        public BodyVariantDefProvider(VariantDef[] variantsForBody, BodyIndex tiedIndex)
        {
            this.variantsForBody = variantsForBody;
            TiedIndex = tiedIndex;
            instances.Add(this);
        }

    }
}