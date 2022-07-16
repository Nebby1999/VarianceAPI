using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using R2API;
using UnityEngine;

namespace VAPI
{
    public class BodyVariantDefProvider
    {
        private static List<BodyVariantDefProvider> instances = new List<BodyVariantDefProvider>();
        [SystemInitializer]
        private static void SystemInitializer()
        {
            Stage.onStageStartGlobal += OnStageStartGlobal;
        }

        private static void OnStageStartGlobal(Stage obj)
        {
            SceneInfo info = SceneInfo.instance;
            Run run = Run.instance;

            if(!info || !run)
            {
                VAPILog.Error($"A Stage ({obj}) has started, but there is no Run And/Or SceneInfo instances! Variants will not be filtered." +
                    $"\n(Run: {run}, SceneInfo: {info}");
                return;
            }

            DirectorAPI.StageInfo stageInfo = DirectorAPI.StageInfo.ParseInternalStageName(info.sceneDef.baseSceneName);
            ExpansionDef[] enabledExpansions = ExpansionCatalog._expansionDefs.Where(ed => run.IsExpansionEnabled(ed)).ToArray();
            
            foreach (BodyVariantDefProvider provider in instances)
                provider.FilterVariants(stageInfo, enabledExpansions);
        }

        public static BodyVariantDefProvider FindProvider(BodyIndex index)
        {
            VariantCatalog.ThrowIfNotInitialized();
            return VariantCatalog.GetBodyVariantDefProvider(index);
        }

        public static BodyVariantDefProvider FindProvider(string bodyName)
        {
            VariantCatalog.ThrowIfNotInitialized();
            return VariantCatalog.GetBodyVariantDefProvider(BodyCatalog.FindBodyIndexCaseInsensitive(bodyName));
        }

        private VariantDef[] variantsForBody = Array.Empty<VariantDef>();
        private VariantDef[] filteredUniques = Array.Empty<VariantDef>();
        private VariantDef[] filteredNonUniques = Array.Empty<VariantDef>();
        public BodyIndex TiedIndex { get; internal set; }

        private void FilterVariants(DirectorAPI.StageInfo stageInfo, ExpansionDef[] runExpansions)
        {
            try
            {
                filteredUniques = variantsForBody.Where(vd => vd.isUnique)
                    .Where(vd => vd.IsAvailable(stageInfo, runExpansions))
                    .ToArray();

                filteredNonUniques = variantsForBody.Where(vd => !vd.isUnique)
                    .Where(vd => vd.IsAvailable(stageInfo, runExpansions))
                    .ToArray();
            }
            catch(Exception e)
            {
                VAPILog.Error($"Could not filter variants for body {BodyCatalog.GetBodyName(TiedIndex)}: {e}");
            }
        }
        public VariantDef[] GetUniqueVariants(bool filtered)
        {
            return filtered ? filteredUniques : variantsForBody.Where(vd => vd.isUnique).ToArray();
        }

        public VariantDef[] GetVariants(bool filtered)
        {
            return filtered ? filteredNonUniques : variantsForBody.Where(vd => !vd.isUnique).ToArray();
        }

        public string GetBodyName()
        {
            GameObject prefab = BodyCatalog.GetBodyPrefab(TiedIndex);
            return prefab ? prefab.name : null;
        }
        public BodyVariantDefProvider(VariantDef[] variantsForBody, BodyIndex tiedIndex)
        {
            this.variantsForBody = variantsForBody;
            TiedIndex = tiedIndex;
            instances.Add(this);
        }

    }
}