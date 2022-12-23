using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAPI
{
    public enum VariantIndex
    {
        None = -1
    }

    public enum VariantTierIndex
    {
        None = -1,
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Legendary = 3,
        AssignedAtRuntime = 4,
    }

    public enum VariantPackIndex
    {
        None = -1,
    }

    public enum ComponentAttachmentType
    {
        Body,
        Master,
        Model
    }

    public enum OverrideNameType
    {
        Prefix,
        Suffix,
        Complete
    }

    [Flags]
    public enum BasicAIModifier
    {
        Default,
        Unstable,
        ForceSprint
    };

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
