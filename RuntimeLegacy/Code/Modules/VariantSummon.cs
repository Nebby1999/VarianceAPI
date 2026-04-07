using RoR2;
using System;
using VAPI.Legacy.Components;

namespace VAPI.Legacy
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
        public VariantDef[] variantDefs = Array.Empty<VariantDef>();
        /// <summary>
        /// Wether or not to apply the variants to the Body on Start
        /// </summary>
        public bool applyOnStart = true;
        /// <summary>
        /// If supplied, the spawned variant will drop XP and gold, just like a regular, director spawned enemy
        /// <para>The base values of the gold and xp are determined by the supplied DeathRewards, multiplied by the <see cref="deathRewardsCoefficient"/></para>
        /// <para>Ifnored if <see cref="supressRewards"/> is false</para>
        /// </summary>
        public DeathRewards summonerDeathRewards;
        /// <summary>
        /// If <see cref="summonerDeathRewards"/> is supplied, the spawned variant will drop XP and gold, said amounts of gold and XP are multiplied by this value.
        /// <para>Ifnored if <see cref="supressRewards"/> is false</para>
        /// </summary>
        public float deathRewardsCoefficient;
        /// <summary>
        /// Wether or not to supress the reward for the summoned Body
        /// </summary>
        public bool supressRewards = false;

        public event Action<CharacterMaster> onSummonCompleted;
        /// <summary>
        /// an event that gets triggered when a VariantSummon is performed
        /// </summary>
        public static event Action<VariantSummonReport> OnServerVariantSummonGlobal;

        [Obsolete("Call \"PerformSummon\" instead.", true)]
        public new CharacterMaster Perform()
        {
            var master = base.Perform();
            ModifySpawnedInstance(master);
            return master;
        }

        public CharacterMaster PerformSummon() => base.Perform();

        private void ModifySpawnedInstance(CharacterMaster spawnedMaster)
        {
            var body = spawnedMaster.GetBodyObject();
            if (body)
            {
                var summonedBodyDeathRewards = body.GetComponent<DeathRewards>();
                if (summonerDeathRewards && summonedBodyDeathRewards && !supressRewards)
                {
                    summonedBodyDeathRewards.expReward = (uint)(summonerDeathRewards.expReward * deathRewardsCoefficient);
                    summonedBodyDeathRewards.goldReward = (uint)(summonerDeathRewards.goldReward * deathRewardsCoefficient);
                }

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
                    if (!supressRewards)
                    {
                        bodyVariantReward.AddVariants(variantDefs);
                    }
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

            onSummonCompleted?.Invoke(spawnedMaster);

            VariantSummonReport report = new VariantSummonReport
            {
                leaderMasterInstance = leader,
                summonInstanceVariants = variantDefs,
                summonMasterInstance = spawnedMaster,
            };
            OnServerVariantSummonGlobal?.Invoke(report);
        }

        //Nasty as fuck
        [SystemInitializer]
        private static void SystemInit()
        {
            On.RoR2.MasterSummon.Perform += (orig, self) =>
            {
                var instance = orig(self);
                if(self is VariantSummon summon)
                {
                    summon.ModifySpawnedInstance(instance);
                }
                return instance;
            };
        }
    }
}