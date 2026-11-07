#nullable enable
using HG;
using System;
using System.Collections;
using System.Collections.Generic;
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