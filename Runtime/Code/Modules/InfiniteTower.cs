using RoR2;
using RoR2.UI;
using System;
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

        internal static void Init()
        {
            _init = true;

            _commonWaveCategory = Addressables.LoadAssetAsync<InfiniteTowerWaveCategory>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerWaveCategories/CommonWaveCategory.asset").WaitForCompletion();
            _wavePrefab = VAPIAssets.LoadAsset<GameObject>("InfiniteTowerWaveArtifactVariance");
            _wavePrerequisite = VAPIAssets.LoadAsset<InfiniteTowerWaveArtifactPrerequisites>("ArtifactVarianceDisabledPrerequisite");
            CloneOverlayEntry(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerCurrentArtifactWispOnDeathUI.prefab").WaitForCompletion());
            FinishPrefab(_wavePrefab);

            _weightedWave = new InfiniteTowerWaveCategory.WeightedWave
            {
                prerequisites = _wavePrerequisite,
                wavePrefab = _wavePrefab,
#if DEBUG
                weight = 100f
#else
                weight = 1f
#endif
            };
        }

        private static void FinishPrefab(GameObject prefab)
        {
            var wispWave = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerWaveArtifactWispOnDeath.prefab").WaitForCompletion();

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
            Debug.Log(_clonedShitIHateCloning);
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
