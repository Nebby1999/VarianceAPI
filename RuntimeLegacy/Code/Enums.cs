using System;

namespace VAPI.Legacy
{
    [Obsolete("VarianceAPI's 2.0 systems are obsolete, VariantTiers are now a \"None\" tier and get assigned at runtime")]
    public enum VariantTierIndex
    {
        None = -1,
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Legendary = 3,
        AssignedAtRuntime = 4,
    }

    [Obsolete("VarianceAPI's 2.0 systems are obsolete, you can specify where a variant component goes by decorating your component class with the TargetComponentObjectAttribute")]
    public enum ComponentAttachmentType
    {
        Body,
        Master,
        Model
    }

    [Obsolete("VarianceAPI's 2.0 systems are obsolete, defining name overrides is done via the IVariantNameProvider interface, a reimplementation of Prefix, Suffix and Complete can be found in VariantNamePrefix, VariantNameSuffix and VariantNameOverride respectively")]
    public enum OverrideNameType
    {
        Prefix,
        Suffix,
        Complete
    }

    [Obsolete("VarianceAPI's 2.0 systems are obsolete, defining AI modifiers is done via the IVariantMasterModifier interface, a reimplementation of Unstable and ForceSprint can be found in UnstableAIModifier and AlwaysSprintAIModifier respectively")]
    [Flags]
    public enum BasicAIModifier
    {
        Default,
        Unstable,
        ForceSprint
    }


    [Obsolete("VarianceAPI's 2.0 systems are obsolete")]
    public enum MeshType
    {
        Default,
        Beetle, //https://github.com/ToastedOven/WetDanger/blob/457c254cca45f961c20665a085e096da23edac0a/MoistureUpset/MoistureUpset/EnemyReplacements.cs#L1318
        BeetleGuard, //https://github.com/ToastedOven/WetDanger/blob/457c254cca45f961c20665a085e096da23edac0a/MoistureUpset/MoistureUpset/EnemyReplacements.cs#L1111
        MiniMushrum, //https://github.com/ToastedOven/WetDanger/blob/457c254cca45f961c20665a085e096da23edac0a/MoistureUpset/MoistureUpset/EnemyReplacements.cs#L1209
        MagmaWorm, //https://github.com/ToastedOven/WetDanger/blob/457c254cca45f961c20665a085e096da23edac0a/MoistureUpset/MoistureUpset/EnemyReplacements.cs#L2051
        OverloadingWorm //https://github.com/ToastedOven/WetDanger/blob/457c254cca45f961c20665a085e096da23edac0a/MoistureUpset/MoistureUpset/EnemyReplacements.cs#L2232
    }
}
