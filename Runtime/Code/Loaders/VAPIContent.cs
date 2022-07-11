using Moonstorm.Loaders;
using R2API.ScriptableObjects;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAPI.Modules;
namespace VAPI
{
    public class VAPIContent : ContentLoader<VAPIContent>
    {
        public static class VariantTiers
        {
            public static VariantTierDef Common => Load(nameof(Common));
            public static VariantTierDef Uncommon => Load(nameof(Uncommon));
            public static VariantTierDef Rare => Load(nameof(Rare));
            public static VariantTierDef Legendary => Load(nameof(Legendary));

            private static VariantTierDef Load(string name)
            {
                if (VAPIAssets.Instance == null)
                    throw new InvalidOperationException($"Cannot load tier {name} without VAPIAssets initialized.");

                return VAPIAssets.LoadAsset<VariantTierDef>(name);
            }
        }
        public static class Artifacts
        {
            public static ArtifactDef Variance;
        }
        public static class Buffs
        {
            public static BuffDef bdVariant;
        }
        public static class Items
        {
            public static ItemDef ExtraPrimary;
            public static ItemDef ExtraSecondary;
            public static ItemDef ExtraSpecial;
            public static ItemDef ExtraUtility;
            public static ItemDef GlobalCDR;
            public static ItemDef GreenHealthbar;
            public static ItemDef Plus1Crit;
            public static ItemDef PrimaryCDR;
            public static ItemDef SecondaryCDR;
            public static ItemDef SpecialCDR;
            public static ItemDef UtilityCDR;
        }
        public override string identifier => VAPIMain.GUID;
        public override R2APISerializableContentPack SerializableContentPack { get; protected set; } = VAPIAssets.LoadAsset<R2APISerializableContentPack>("VAPIContent");
        public override Action[] LoadDispatchers { get; protected set; }
        public override Action[] PopulateFieldsDispatchers { get; protected set; }

        public override void Init()
        {
            base.Init();
            LoadDispatchers = new Action[]
            {
                () =>
                {
                    new ItemModule().Initialize();
                },
                () =>
                {
                    VAPILog.Info($"Adding default Variant Tiers");
                    VariantTierCatalog.AddTiers(VAPIAssets.LoadAllAssetsOfType<VariantTierDef>(), VAPIConfig.rewardsConfig);
                },
            };
            PopulateFieldsDispatchers = new Action[]
            {
                () => PopulateTypeFields(typeof(Artifacts), ContentPack.artifactDefs),
                () => PopulateTypeFields(typeof(Buffs), ContentPack.buffDefs),
                () => PopulateTypeFields(typeof(Items), ContentPack.itemDefs)
            };
        }
    }
}
