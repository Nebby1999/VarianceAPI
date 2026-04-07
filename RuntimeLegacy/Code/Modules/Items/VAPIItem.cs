using MSU;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI.Legacy.Items
{
    public abstract class VAPIItem : IItemContentPiece
    {
        public NullableRef<List<GameObject>> itemDisplayPrefabs { get; protected set; }
        NullableRef<List<GameObject>> IItemContentPiece.itemDisplayPrefabs => itemDisplayPrefabs;

        public ItemDef itemDef { get; private set; }
        ItemDef IContentPiece<ItemDef>.asset => itemDef;

        public abstract VAPIAssetRequest<ItemDef> GetAssetRequest();
        public virtual void Initialize() { }
        public abstract bool IsAvailable(ContentPack contentPack);
        public IEnumerator LoadContentAsync()
        {
            var assetRequest = GetAssetRequest();
            while (!assetRequest.isDone)
                yield return null;

            itemDef = assetRequest.asset;
        }
    }
}