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

    public enum ComponentAttachmentType
    {
        Body,
        Master,
        Model
    }
}
