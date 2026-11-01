#nullable enable
using MonoMod.Utils;
using RoR2;
using RoR2.Items;
using RoR2BepInExPack.Utilities;
using System;

namespace VAPI
{
    //This class is very neat, the constructor will register to MasterSummon's event, and the deconstructor will unregister.
    //As soon as a master summon happens, and the master summon that happened is the one stored in this instance, it'll turn it into a variant.
    //Beats inheriting from MasterSummon any day.

    /// <summary>
    /// A class that encapsulates a <see cref="MasterSummon"/>.
    /// <para></para>
    /// Used to turn the master summoned into a variant.
    /// </summary>
    public sealed class VariantMasterSummon
    {
        private static FixedConditionalWeakTable<MasterSummon, VariantMasterSummon> _masterSummonToVariantMasterSummon = new FixedConditionalWeakTable<MasterSummon, VariantMasterSummon>();

        [SystemInitializer]
        private static void Init()
        {
            MasterSummon.onServerMasterSummonGlobal += CallVariantMasterSummonMethodIfApplicable;
        }

        private static void CallVariantMasterSummonMethodIfApplicable(MasterSummon.MasterSummonReport obj)
        {
            var masterSummon = obj.masterSummon;
            if(_masterSummonToVariantMasterSummon.TryGetValue(masterSummon, out var variantMasterSummon))
            {
                variantMasterSummon.OnMasterSummonReportGenerated(obj);
            }
        }

        /// <summary>
        /// The report of the VariantSummon
        /// </summary>
        public readonly struct VariantMasterSummonReport
        {
            /// <summary>
            /// The base masterSummonReport from the MasterSummon
            /// </summary>
            public readonly MasterSummon.MasterSummonReport masterSummonReport;

            /// <summary>
            /// The <see cref="CharacterVariantDef"/>s that the summon has
            /// </summary>
            public readonly CharacterVariantDef[]? summonInstanceVariants;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="masterSummonReport">The base master summon report</param>
            /// <param name="summonInstanceVariants">The summon's character variant defs</param>
            public VariantMasterSummonReport(MasterSummon.MasterSummonReport masterSummonReport, CharacterVariantDef[] summonInstanceVariants)
            {
                this.masterSummonReport = masterSummonReport;
                this.summonInstanceVariants = summonInstanceVariants;
            }
        }

        /// <summary>
        /// The VariantDefs for the master
        /// </summary>
        public CharacterVariantDef[]? variantDefs;

        /// <summary>
        /// The death rewards from the summoner, if supplied the summon will have the same gold and experience reward
        /// </summary>
        public DeathRewards? summonerDeathRewards;

        /// <summary>
        /// A reward coefficient of <see cref="summonerDeathRewards"/>'s gold and experience amounts
        /// </summary>
        public float deathRewardsCoefficient = 1;

        /// <summary>
        /// If true, the announcement system will not announce the variant's arrival.
        /// </summary>
        public bool supressArrivalMessage;

        /// <summary>
        /// If true, when the summoned variant dies it is guaranteed to never drop items
        /// </summary>
        public bool noItemReward;

        /// <summary>
        /// If true, when the summoned variant is spawned it is guaranteed to not recieve any reward modifications from the applied variants.
        /// </summary>
        public bool noGoldExpModifications;

        /// <summary>
        /// Event called when the summon is performed
        /// </summary>
        public event Action<VariantMasterSummonReport>? onSummonPerformed;

        /// <summary>
        /// Event called when any <see cref="VariantMasterSummon"/> is performed
        /// </summary>
        public static event Action<VariantMasterSummonReport>? onServerVariantSummonGlobal;

        private void OnMasterSummonReportGenerated(MasterSummon.MasterSummonReport masterSummonReport)
        {
            //MasterSummon.onServerMasterSummonGlobal -= OnMasterSummonReportGenerated;

            VAPIUtils.TransformSpawnedInstanceIntoVariant(new VAPIUtils.TransformSpawnedInstanceIntoVariantArgs
            {
                spawnedInstance = masterSummonReport.summonMasterInstance.gameObject,
                variantDefs = variantDefs,
                baseDeathRewards = summonerDeathRewards,
                deathRewardsCoefficient = deathRewardsCoefficient,
                supressArrivalMessage = supressArrivalMessage,
                noGoldExpModifications = noGoldExpModifications,
                noItemRewards = noItemReward,
            });

            var report = new VariantMasterSummonReport(masterSummonReport, variantDefs);
            onSummonPerformed?.Invoke(report);
            onServerVariantSummonGlobal?.Invoke(report);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="masterSummon">The master summon, from which once performed, will be turned into a variant</param>
        public VariantMasterSummon(MasterSummon masterSummon)
        {
            _masterSummonToVariantMasterSummon.Add(masterSummon, this);
        }
    }
}