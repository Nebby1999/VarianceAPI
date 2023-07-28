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
        private static GameObject wavePrefab;
        private static GameObject clonedShitIHateCloning;
        private static InfiniteTowerWaveArtifactPrerequisites wavePrerequisite;
        private static InfiniteTowerWaveCategory commonWaveCategory;
        private static InfiniteTowerWaveCategory.WeightedWave weightedWave;
        private static int waveIndex = -1;
        private static bool init = false;
        public static void AddOrRemoveWave(bool add)
        {
            if(Run.instance && Run.instance is InfiniteTowerRun)
            {
                VAPILog.Warning("Trying to remove Variance artifact wave while an infinite tower run is active! this may cause instability and issues, here be dragons.");
            }

            if(add && waveIndex == -1)
            {
                Add();
            }
            else if(!add && waveIndex >= 0)
            {
                Remove();
            }
        }

        private static void Add()
        {
            HG.ArrayUtils.ArrayAppend(ref commonWaveCategory.wavePrefabs, in weightedWave);
            waveIndex = commonWaveCategory.wavePrefabs.Length - 1;
        }

        private static void Remove()
        {
            HG.ArrayUtils.ArrayRemoveAtAndResize(ref commonWaveCategory.wavePrefabs, waveIndex);
            waveIndex = -1;
        }

        internal static void Init()
        {
            init = true;

            commonWaveCategory = Addressables.LoadAssetAsync<InfiniteTowerWaveCategory>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerWaveCategories/CommonWaveCategory.asset").WaitForCompletion();
            wavePrefab = VAPIAssets.LoadAsset<GameObject>("InfiniteTowerWaveArtifactVariance");
            wavePrerequisite = VAPIAssets.LoadAsset<InfiniteTowerWaveArtifactPrerequisites>("ArtifactVarianceDisabledPrerequisite");
            CloneOverlayEntry(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerCurrentArtifactWispOnDeathUI.prefab").WaitForCompletion());
            FinishPrefab(wavePrefab);

            weightedWave = new InfiniteTowerWaveCategory.WeightedWave
            {
                prerequisites = wavePrerequisite,
                wavePrefab = wavePrefab,
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
            prefabWaveController.overlayEntries[1].prefab = clonedShitIHateCloning;
            prefabWaveController.rewardDropTable = wispWaveController.rewardDropTable;
            prefabWaveController.rewardPickupPrefab = wispWaveController.rewardPickupPrefab;
        }

        private static GameObject CloneOverlayEntry(GameObject original)
        {
            clonedShitIHateCloning = R2API.PrefabAPI.InstantiateClone(original, "VarianceAugmentDisplay", false);
            Debug.Log(clonedShitIHateCloning);
            var offset = clonedShitIHateCloning.transform.GetChild(0);
            var waveIcon = offset.GetChild(0);
            var iconGameObject = waveIcon.GetChild(0);
            var icon = iconGameObject.GetComponent<Image>();
            icon.sprite = wavePrerequisite.bannedArtifact.smallIconSelectedSprite;

            clonedShitIHateCloning.GetComponentInChildren<LanguageTextMeshController>()._token = wavePrerequisite.bannedArtifact.descriptionToken;
            clonedShitIHateCloning.GetComponentInChildren<InfiniteTowerWaveCounter>().token = "VAPI_INFINITETOWER_WAVE_COUNTER_VARIANCE";
            return clonedShitIHateCloning;
        }
    }
}
