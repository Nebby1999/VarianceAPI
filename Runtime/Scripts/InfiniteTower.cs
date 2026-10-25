using MSU;
using RoR2;
using RoR2.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace VAPI
{
    internal static class InfiniteTower
    {
        private static GameObject _wavePrefab;
        private static GameObject _augmentDisplayPrefab;
        private static InfiniteTowerWaveArtifactPrerequisites _wavePrerequisite;
        private static InfiniteTowerWaveCategory _commonWaveCategory;
        private static InfiniteTowerWaveCategory.WeightedWave _weightedWave;
        private static int _waveIndex = -1;
        private static bool _init = false;

        public static void AddOrRemoveWave(bool add)
        {
            if (add && _waveIndex == -1)
            {
                Add();
            }
            else if (!add && _waveIndex >= 0)
            {
                Remove();
            }
        }

        private static void Add()
        {
            HG.ArrayUtils.ArrayAppend(ref _commonWaveCategory.wavePrefabs, in _weightedWave);
            _waveIndex = _commonWaveCategory.wavePrefabs.Length - 1;
        }

        private static void Remove()
        {
            HG.ArrayUtils.ArrayRemoveAtAndResize(ref _commonWaveCategory.wavePrefabs, _waveIndex);
            _waveIndex = -1;
        }

        internal static IEnumerator Init()
        {
            VAPILog.Info($"Initializing Infinite Tower Support.");

            //"RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/ITWaveCategories/CommonWaveCategory.asset"
            var commonWaveCategoryRequest = Addressables.LoadAssetAsync<InfiniteTowerWaveCategory>("4e63333e89a09f64680d3475ba1b5903");
            //"RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/InfiniteTowerCurrentArtifactWispOnDeathUI.prefab"
            var overlayEntryRequest = Addressables.LoadAssetAsync<GameObject>("ba7ed5ac69f57ea4999d0850ef1978f4");
            //"RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/InfiniteTowerWaveArtifactWispOnDeath.prefab"
            var wispWaveRequest = Addressables.LoadAssetAsync<GameObject>("d4cdad7b9d38d63488989d9f09a92999");
            using var assetCollectionRequest = VAPIAssets.LoadAssetAsync<AssetCollection>("acInfiniteTower");

            var coroutine = ParallelCoroutine.RequestFromPool();
            coroutine.Add(commonWaveCategoryRequest);
            coroutine.Add(overlayEntryRequest);
            coroutine.Add(wispWaveRequest);
            coroutine.Add(assetCollectionRequest);

            while(!coroutine.isDone)
            {
                yield return null;
            }

            _commonWaveCategory = commonWaveCategoryRequest.Result;

            var assetCollection = assetCollectionRequest.asset;
            _wavePrefab = assetCollection.FindAsset<GameObject>("InfiniteTowerWaveArtifactVariance");
            _wavePrerequisite = assetCollection.FindAsset<InfiniteTowerWaveArtifactPrerequisites>("ArtifactVarianceDisabledPrerequisite");

            CloneOverlayEntry(overlayEntryRequest.Result);
            FinishPrefab(_wavePrefab, wispWaveRequest.Result);

            _weightedWave = new InfiniteTowerWaveCategory.WeightedWave
            {
                prerequisites = _wavePrerequisite,
                wavePrefab = _wavePrefab,
                weight = 1f
            };
        }

        private static void FinishPrefab(GameObject prefab, GameObject wispWave)
        {

            var wispWaveController = wispWave.GetComponent<InfiniteTowerWaveController>();
            var prefabWaveController = prefab.GetComponent<InfiniteTowerWaveController>();

            prefabWaveController.uiPrefab = wispWaveController.uiPrefab;
            prefabWaveController.overlayEntries = HG.ArrayUtils.Clone(wispWaveController.overlayEntries);
            prefabWaveController.overlayEntries[1].prefab = _augmentDisplayPrefab;
            prefabWaveController.rewardDropTable = wispWaveController.rewardDropTable;
            prefabWaveController.rewardPickupPrefab = wispWaveController.rewardPickupPrefab;
        }

        private static GameObject CloneOverlayEntry(GameObject original)
        {
            _augmentDisplayPrefab = R2API.PrefabAPI.InstantiateClone(original, "VarianceAugmentDisplay", false);
            var offset = _augmentDisplayPrefab.transform.GetChild(0);
            var waveIcon = offset.GetChild(0);
            var iconGameObject = waveIcon.GetChild(0);
            var icon = iconGameObject.GetComponent<Image>();
            icon.sprite = _wavePrerequisite.bannedArtifact.smallIconSelectedSprite;

            _augmentDisplayPrefab.GetComponentInChildren<LanguageTextMeshController>()._token = _wavePrerequisite.bannedArtifact.descriptionToken;
            _augmentDisplayPrefab.GetComponentInChildren<InfiniteTowerWaveCounter>().token = "VAPI_INFINITETOWER_WAVE_COUNTER_VARIANCE";
            return _augmentDisplayPrefab;
        }
    }
}