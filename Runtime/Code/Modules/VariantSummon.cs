using RoR2;
using System;
using VAPI.Components;

namespace VAPI
{
    /// <summary>
    /// An extended version of the game's <see cref="MasterSummon"/>, which can allow you to summon or spawn a specific variant
    /// </summary>
    public class VariantSummon : MasterSummon
    {
        /// <summary>
        /// A Variant that has been summoned
        /// </summary>
        public struct VariantSummonReport
        {
            /// <summary>
            /// The summoned Variant's master
            /// </summary>
            public CharacterMaster summonMasterInstance;
            /// <summary>
            /// The summoned variant's master
            /// </summary>
            public VariantDef[] summonInstanceVariants;
            /// <summary>
            /// The master that summoned the variants, can be null
            /// </summary>
            public CharacterMaster leaderMasterInstance;
        }
        /// <summary>
        /// The VariantDefs that'll be given to the variant when spawned
        /// </summary>
        public VariantDef[] variantDefs;
        /// <summary>
        /// Wether or not to apply the variants to the Body on Start
        /// </summary>
        public bool applyOnStart = true;
        /// <summary>
        /// an event that gets triggered when a VariantSummon is performed
        /// </summary>
        public static event Action<VariantSummonReport> OnServerVariantSummonGlobal;
        /// <summary>
        /// Performs the VariantSummon
        /// </summary>
        /// <returns>The Summoned variant's CharacterMaster</returns>
        public CharacterMaster Perform()
        {
            var master = base.Perform();
            var body = master.GetBodyObject();
            if (body)
            {
                body.AddComponent<DoNotTurnIntoVariant>();
                var bodyVariantManager = body.GetComponent<BodyVariantManager>();
                var bodyVariantReward = body.GetComponentInParent<BodyVariantReward>();

                if (bodyVariantManager)
                {
                    bodyVariantManager.AddVariants(variantDefs);
                    bodyVariantManager.applyOnStart = applyOnStart;
                }
                if (bodyVariantReward)
                {
                    bodyVariantReward.AddVariants(variantDefs);
                    bodyVariantReward.applyOnStart = applyOnStart;
                }
            }

            CharacterMaster leader = null;
            if (summonerBodyObject)
            {
                CharacterBody summonerBody = summonerBodyObject.GetComponent<CharacterBody>();
                if (summonerBody)
                {
                    leader = summonerBody.master;
                }
            }
            VariantSummonReport report = new VariantSummonReport
            {
                leaderMasterInstance = leader,
                summonInstanceVariants = variantDefs,
                summonMasterInstance = master,
            };
            OnServerVariantSummonGlobal?.Invoke(report);

            return master;
        }
    }
}