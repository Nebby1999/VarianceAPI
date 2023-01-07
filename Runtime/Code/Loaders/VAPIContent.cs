using Moonstorm.Loaders;
using R2API.ScriptableObjects;
using RoR2;
using System;
using VAPI.Modules;
namespace VAPI
{
    /// <summary>
    /// VAPI's ContentLoader
    /// </summary>
    public class VAPIContent : ContentLoader<VAPIContent>
    {
        /// <summary>
        /// A static list of all the default VariantTierDefs
        /// </summary>
        public static class VariantTiers
        {
            /// <summary>
            /// Loads the Common VariantTierDef
            /// </summary>
            public static VariantTierDef Common => Load(nameof(Common));
            /// <summary>
            /// Loads the Uncommon VariantTierDef
            /// </summary>
            public static VariantTierDef Uncommon => Load(nameof(Uncommon));
            /// <summary>
            /// Loads the Rare VariantTierDef
            /// </summary>
            public static VariantTierDef Rare => Load(nameof(Rare));
            /// <summary>
            /// Loads the Legendary VariantTierDef
            /// </summary>
            public static VariantTierDef Legendary => Load(nameof(Legendary));

            private static VariantTierDef Load(string name)
            {
                if (VAPIAssets.Instance == null)
                    throw new InvalidOperationException($"Cannot load tier {name} without VAPIAssets initialized.");

                return VAPIAssets.LoadAsset<VariantTierDef>(name);
            }
        }
        /// <summary>
        /// A static class with the ArtifactOfVariance's ArtifactDef
        /// </summary>
        public static class Artifacts
        {
            public static ArtifactDef Variance;
        }
        /// <summary>
        /// A static class with the VarianceAPI's variant buffDef
        /// </summary>
        public static class Buffs
        {
            public static BuffDef bdVariant;
        }
        /// <summary>
        /// A static class with VarianceAPI's intrinsic variant items
        /// </summary>
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
                    VAPILog.Info($"Adding base VariantPack");
                    VariantPackCatalog.AddVariantPack(VAPIAssets.LoadAsset<VariantPackDef>("BaseVariantPack"), VAPIConfig.rewardsConfig);
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
