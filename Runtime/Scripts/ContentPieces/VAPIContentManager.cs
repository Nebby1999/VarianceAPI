using MSU;
using RoR2;

namespace VAPI
{
    internal static class VAPIContentManager
    {
        [SystemInitializer]
        private static void Initializer()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += ApplyVAPIItemEffects;
            On.RoR2.UI.HealthBar.UpdateBarInfos += MakeHealthBarGreen;
        }

        private static void MakeHealthBarGreen(On.RoR2.UI.HealthBar.orig_UpdateBarInfos orig, RoR2.UI.HealthBar self)
        {
            orig(self);
            if(self.source && self.source.body && self.source.body.HasItem(VAPIContent.Items.GreenHealthbar))
            {
                self.barInfoCollection.trailingOverHealthbarInfo.color = VAPIConfig._variantHealthBarColor;
            }
        }

        private static void ApplyVAPIItemEffects(RoR2.CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            int linearArmorBonusCount = sender.GetBuffCount(VAPIContent.Buffs.LinearArmorBonus);

            args.armorAdd += linearArmorBonusCount;

            int greenHealthBarCount = sender.GetItemCount(VAPIContent.Items.GreenHealthbar);

            int globalCDRCount = sender.GetItemCount(VAPIContent.Items.GlobalCDR);
            args.allSkills.cooldownMultAdd -= globalCDRCount * 0.01f;

            int extraPrimaryCount = sender.GetItemCount(VAPIContent.Items.ExtraPrimary);
            int primaryCDRCount = sender.GetItemCount(VAPIContent.Items.PrimaryCDR);
            args.primarySkill.bonusStockAdd += extraPrimaryCount;
            args.primarySkill.cooldownMultAdd -= primaryCDRCount * 0.01f;

            int extraSecondaryCount = sender.GetItemCount(VAPIContent.Items.ExtraSecondary);
            int secondaryCRDCount = sender.GetItemCount(VAPIContent.Items.SecondaryCDR);
            args.secondarySkill.bonusStockAdd += extraSecondaryCount;
            args.secondarySkill.cooldownMultAdd -= secondaryCRDCount * 0.01f;

            int extraUtilityCount = sender.GetItemCount(VAPIContent.Items.ExtraUtility);
            int utilityCDRCount = sender.GetItemCount(VAPIContent.Items.UtilityCDR);
            args.utilitySkill.bonusStockAdd += extraUtilityCount;
            args.utilitySkill.cooldownMultAdd -= utilityCDRCount * 0.01f;

            int extraSpecialCount = sender.GetItemCount(VAPIContent.Items.ExtraSpecial);
            int specialCDRCount = sender.GetItemCount(VAPIContent.Items.SpecialCDR);
            args.specialSkill.bonusStockAdd += extraSpecialCount;
            args.specialSkill.cooldownMultAdd -= specialCDRCount * 0.01f;

            int plus1CritCount = sender.GetItemCount(VAPIContent.Items.Plus1Crit);
            args.critAdd += plus1CritCount;
        }
    }
}