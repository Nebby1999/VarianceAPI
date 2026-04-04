#nullable enable
using RoR2;
using System;

namespace VAPI
{
    public sealed class VariantMasterSummon
    {
        public readonly struct VariantMasterSummonReport
        {
            public readonly MasterSummon.MasterSummonReport masterSummonReport;
            public readonly CharacterVariantDef[] summonInstanceVariants;

            public VariantMasterSummonReport(MasterSummon.MasterSummonReport masterSummonReport, CharacterVariantDef[] summonInstanceVariants)
            {
                this.masterSummonReport = masterSummonReport;
                this.summonInstanceVariants = summonInstanceVariants;
            }
        }

        public readonly MasterSummon masterSummon;

        public CharacterVariantDef[] variantDefs = Array.Empty<CharacterVariantDef>();
        public DeathRewards? summonerDeathRewards;
        public float deathRewardsCoefficient;

        public event Action<VariantMasterSummonReport>? onSummonPerformed;
        public static event Action<VariantMasterSummonReport>? onServerVariantSummonGlobal;

        public void Perform() => masterSummon.Perform();

        private void OnMasterSummonReportGenerated(MasterSummon.MasterSummonReport masterSummonReport)
        {
            if(masterSummonReport.masterSummon == masterSummon)
            {
                MasterSummon.onServerMasterSummonGlobal -= OnMasterSummonReportGenerated;

                VAPIUtils.ModifyMasterToBecomeVariant(masterSummonReport.summonMasterInstance.gameObject, variantDefs, summonerDeathRewards, deathRewardsCoefficient);
                var report = new VariantMasterSummonReport(masterSummonReport, variantDefs);
                onSummonPerformed?.Invoke(report);
                onServerVariantSummonGlobal?.Invoke(report);
            }
        }

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