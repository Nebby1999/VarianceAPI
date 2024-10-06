using MSU;
using RoR2;
using RoR2.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace VAPI.Modules
{
    internal static class InfiniteTower
    {
        private static GameObject _wavePrefab;
        private static GameObject _clonedShitIHateCloning;
        private static InfiniteTowerWaveArtifactPrerequisites _wavePrerequisite;
        private static InfiniteTowerWaveCategory _commonWaveCategory;
        private static InfiniteTowerWaveCategory.WeightedWave _weightedWave;
        private static int _waveIndex = -1;
        private static bool _init = false;
        public static void AddOrRemoveWave(bool add)
        {
            if(Run.instance && Run.instance is InfiniteTowerRun)
            {
                VAPILog.Warning("Trying to remove Variance artifact wave while an infinite tower run is active! this may cause instability and issues, here be dragons.");
            }

            if(add && _waveIndex == -1)
            {
                Add();
            }
            else if(!add && _waveIndex >= 0)
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
            VAPILog.Info($"Initializing infinite tower support");
            _init = true;

            var commonWaveCategoryRequest = Addressables.LoadAssetAsync<InfiniteTowerWaveCategory>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerWaveCategories/CommonWaveCategory.asset");
            var overlayEntryRequest = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerCurrentArtifactWispOnDeathUI.prefab");
            var assetCollectionRequest = VAPIAssets.LoadAssetAsync<AssetCollection>("acInfiniteTower");
            var wispWave = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerWaveArtifactWispOnDeath.prefab");

            ParallelCoroutine coroutine = new ParallelCoroutine();
            coroutine.Add(commonWaveCategoryRequest);
            coroutine.Add(overlayEntryRequest);
            coroutine.Add(assetCollectionRequest);
            coroutine.Add(wispWave);

            while (!coroutine.IsDone())
                yield return null;

            _commonWaveCategory = commonWaveCategoryRequest.Result;

            var assetCollection = assetCollectionRequest.asset;
            _wavePrefab = assetCollection.FindAsset<GameObject>("InfiniteTowerWaveArtifactVariance");
            _wavePrerequisite = assetCollection.FindAsset<InfiniteTowerWaveArtifactPrerequisites>("ArtifactVarianceDisabledPrerequisite");

            CloneOverlayEntry(overlayEntryRequest.Result);
            FinishPrefab(_wavePrefab, wispWave.Result);

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
            prefabWaveController.overlayEntries[1].prefab = _clonedShitIHateCloning;
            prefabWaveController.rewardDropTable = wispWaveController.rewardDropTable;
            prefabWaveController.rewardPickupPrefab = wispWaveController.rewardPickupPrefab;
        }

        private static GameObject CloneOverlayEntry(GameObject original)
        {
            _clonedShitIHateCloning = R2API.PrefabAPI.InstantiateClone(original, "VarianceAugmentDisplay", false);
            var offset = _clonedShitIHateCloning.transform.GetChild(0);
            var waveIcon = offset.GetChild(0);
            var iconGameObject = waveIcon.GetChild(0);
            var icon = iconGameObject.GetComponent<Image>();
            icon.sprite = _wavePrerequisite.bannedArtifact.smallIconSelectedSprite;

            _clonedShitIHateCloning.GetComponentInChildren<LanguageTextMeshController>()._token = _wavePrerequisite.bannedArtifact.descriptionToken;
            _clonedShitIHateCloning.GetComponentInChildren<InfiniteTowerWaveCounter>().token = "VAPI_INFINITETOWER_WAVE_COUNTER_VARIANCE";
            return _clonedShitIHateCloning;
        }
    }
}
