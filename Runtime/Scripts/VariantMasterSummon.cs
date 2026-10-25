#nullable enable
using RoR2;
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
            public readonly CharacterVariantDef[] summonInstanceVariants;

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
        /// The MasterSummon that'll be turned into a variant
        /// </summary>
        public readonly MasterSummon masterSummon;

        /// <summary>
        /// The VariantDefs for the master
        /// </summary>
        public CharacterVariantDef[] variantDefs = Array.Empty<CharacterVariantDef>();

        /// <summary>
        /// The death rewards from the summoner, if supplied the summon will have the same gold and experience reward
        /// </summary>
        public DeathRewards? summonerDeathRewards;

        /// <summary>
        /// A reward coefficient of <see cref="summonerDeathRewards"/>'s gold and experience amounts
        /// </summary>
        public float deathRewardsCoefficient = 1;

        /// <summary>
        /// Event called when the summon is performed
        /// </summary>
        public event Action<VariantMasterSummonReport>? onSummonPerformed;

        /// <summary>
        /// Event called when any <see cref="VariantMasterSummon"/> is performed
        /// </summary>
        public static event Action<VariantMasterSummonReport>? onServerVariantSummonGlobal;

        /// <summary>
        /// Performs the summon
        /// </summary>
        public void Perform() => masterSummon.Perform();

        private void OnMasterSummonReportGenerated(MasterSummon.MasterSummonReport masterSummonReport)
        {
            //If the master summon that just happened is the one stored in this, unhook from the event and turn it into a variant.
            if(masterSummonReport.masterSummon == masterSummon)
            {
                MasterSummon.onServerMasterSummonGlobal -= OnMasterSummonReportGenerated;

                VAPIUtils.TransformSpawnedInstanceIntoVariant(masterSummonReport.summonMasterInstance.gameObject, variantDefs, summonerDeathRewards, deathRewardsCoefficient);

                var report = new VariantMasterSummonReport(masterSummonReport, variantDefs);
                onSummonPerformed?.Invoke(report);
                onServerVariantSummonGlobal?.Invoke(report);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="masterSummon">The master summon, from which once performed, will be turned into a variant</param>
        public VariantMasterSummon(MasterSummon masterSummon)
        {
            this.masterSummon = masterSummon;
            MasterSummon.onServerMasterSummonGlobal += OnMasterSummonReportGenerated;
        }

        ~VariantMasterSummon()
        {
            MasterSummon.onServerMasterSummonGlobal -= OnMasterSummonReportGenerated;
        }
    }
}