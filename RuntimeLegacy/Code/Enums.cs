using System;

namespace VAPI
{
    /// <summary>
    /// An index that represents a VariantDef, do not set this value yourself.
    /// </summary>
    public enum VariantIndex
    {
        /// <summary>
        /// Represents an invalid or uninitialized VariantDef
        /// </summary>
        None = -1
    }

    /// <summary>
    /// An index that represents a VariantTier, do not set this value yourself
    /// </summary>
    public enum VariantTierIndex
    {
        /// <summary>
        /// Represents an invalid or uninitialized VariantTierDef
        /// </summary>
        None = -1,
        /// <summary>
        /// Represents a Common variant, common variants have a unique buff to indicatte its a Variant.
        /// </summary>
        Common = 0,
        /// <summary>
        /// Represents an Uncommon variant, uncommon variants have all the Common properties, alongside their health bars being green.
        /// </summary>
        Uncommon = 1,
        /// <summary>
        /// Represents a Rare variant, Rare variants have all the Uncommon properties, alongside announcing ther arrival thru a chat message
        /// </summary>
        Rare = 2,
        /// <summary>
        /// Represents a Legendary variant, Legendary variants have all the Rare properties, alongside playing a unique spawn sound effect.
        /// </summary>
        Legendary = 3,
        /// <summary>
        /// Represents a tier that's assigned at runtime, you only want to set this for custom VariantTierDefs that will be initialized by the <see cref="VariantTierCatalog"/>
        /// </summary>
        AssignedAtRuntime = 4,
    }

    /// <summary>
    /// An index that represents a VariantPack, do not set this value yourself
    /// </summary>
    public enum VariantPackIndex
    {
        /// <summary>
        /// Represents an invalid or uninitalized VariantPack
        /// </summary>
        None = -1,
    }

    /// <summary>
    /// An index to specify how a VariantComponentt should be added.
    /// </summary>
    public enum ComponentAttachmentType
    {
        /// <summary>
        /// Adds the specified VariantComponent to the variant's Body object
        /// </summary>
        Body,
        /// <summary>
        /// Adds the specified VariantComponent to the variant's Master object
        /// </summary>
        Master,
        /// <summary>
        /// Adds the specified VariantComponent to the variant's Model object
        /// </summary>
        Model
    }

    /// <summary>
    /// An index to specify how the NameOverride system should use the custom variant name
    /// </summary>
    public enum OverrideNameType
    {
        /// <summary>
        /// The Override Name will be treated as a prefix, EJ: Lemurian -> Awesome Lemurian
        /// </summary>
        Prefix,
        /// <summary>
        /// The Override Name will be treated as a sufix, EJ: Lemurian => Lemurian God
        /// </summary>
        Suffix,
        /// <summary>
        /// The override Name will completely override the variant's name, EJ: Elder Lemurian => Thanatos
        /// </summary>
        Complete
    }

    /// <summary>
    /// Represents a flag that modifies AI for a variant
    /// </summary>
    [Flags]
    public enum BasicAIModifier
    {
        /// <summary>
        /// No changes are made
        /// </summary>
        Default,
        /// <summary>
        /// The variant can use it's "Desperation Attacks" always, EJ: Wandering Vagrant Nova
        /// </summary>
        Unstable,
        /// <summary>
        /// The variant always sprints.
        /// </summary>
        ForceSprint
    }

    /// <summary>
    /// Represents a MeshType for use with the mesh override system
    /// </summary>
    public enum MeshType
    {
        /// <summary>
        /// The mesh is not a special mesh
        /// </summary>
        Default,
        /// <summary>
        /// The mesh is used for a Beetle variant
        /// </summary>
        Beetle, //https://github.com/ToastedOven/WetDanger/blob/457c254cca45f961c20665a085e096da23edac0a/MoistureUpset/MoistureUpset/EnemyReplacements.cs#L1318
        /// <summary>
        /// The mesh is used for a Beetle Guard variant
        /// </summary>
        BeetleGuard, //https://github.com/ToastedOven/WetDanger/blob/457c254cca45f961c20665a085e096da23edac0a/MoistureUpset/MoistureUpset/EnemyReplacements.cs#L1111
        /// <summary>
        /// The mesh is used for a Mini Mushrum variant
        /// </summary>
        MiniMushrum, //https://github.com/ToastedOven/WetDanger/blob/457c254cca45f961c20665a085e096da23edac0a/MoistureUpset/MoistureUpset/EnemyReplacements.cs#L1209
        /// <summary>
        /// The mesh is used for a Magma Worm variant
        /// </summary>
        MagmaWorm, //https://github.com/ToastedOven/WetDanger/blob/457c254cca45f961c20665a085e096da23edac0a/MoistureUpset/MoistureUpset/EnemyReplacements.cs#L2051
        /// <summary>
        /// The mesh is used for an Overloading Worm variant
        /// </summary>
        OverloadingWorm //https://github.com/ToastedOven/WetDanger/blob/457c254cca45f961c20665a085e096da23edac0a/MoistureUpset/MoistureUpset/EnemyReplacements.cs#L2232
    }
}
