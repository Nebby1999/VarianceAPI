#nullable enable
using UnityEngine.Networking;

namespace VAPI
{
    /// <summary>
    /// Inherits from <see cref="SyncListStruct{T}"/>, used to sync CharacterVariantIndices
    /// </summary>
    public sealed class SyncListCharacterVariantIndex : SyncListStruct<NetworkCharacterVariantIndex>
    {

    }
}