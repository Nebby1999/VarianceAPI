/*using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VarianceAPI.Modules;
using VarianceAPI.Scriptables;

namespace TheOriginal30
{
    public class MaterialGrabber : VariantMaterialGrabber
    {
        public ItemDisplayRuleSet IDRS;
        public Material perforatorMat;
        public Material glandMaterial;
        public Material ghostMaterial;
        public Material fireTrailMaterial;
        public Material solusMaterial;
        public Material visionsMaterial;
        public Material dunestriderMat;
        public Material lunarFlameMat;
        public Material greaterWispMat;
        public Material greaterWispFlameMat;
        public Material wispFlameMat;
        public Material lunarGolemMat;
        public Material titanGoldMat;
        public Material skeletalMat;
        public Material shatterspleenMat;

        public void StartGrabber(AssetBundle assets)
        {
            assetBundle = assets;
            CreateCorrectMaterials();
            Init();
        }

        public void CreateCorrectMaterials()
        {
            IDRS = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet;
            //Perforator mat
            perforatorMat = UnityEngine.Object.Instantiate(IDRS.FindDisplayRuleGroup(RoR2Content.Items.FireballsOnHit).rules[0].followerPrefab.GetComponentInChildren<MeshRenderer>().material);
            CreateVMR(perforatorMat, "PerforatorMaterial");

            //Beetle Gland Material
            glandMaterial = UnityEngine.Object.Instantiate(IDRS.FindDisplayRuleGroup(RoR2Content.Items.BeetleGland).rules[0].followerPrefab.GetComponentInChildren<Renderer>().material);
            CreateVMR(glandMaterial, "GlandMaterial");

            //Ghost Effect Material
            ghostMaterial = Resources.Load<Material>("Materials/matGhostEffect");
            CreateVMR(ghostMaterial, "GhostMaterial");

            //fireTrail Material
            fireTrailMaterial = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/ProjectileGhosts/FireMeatBallGhost").GetComponentInChildren<TrailRenderer>().material);
            CreateVMR(fireTrailMaterial, "FireTrailMaterial");

            //solus Material
            solusMaterial = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/RoboBallBossBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial);
            CreateVMR(solusMaterial, "SolusMaterial");

            //Visions Material
            visionsMaterial = UnityEngine.Object.Instantiate(IDRS.FindDisplayRuleGroup(RoR2Content.Items.LunarPrimaryReplacement).rules[0].followerPrefab.GetComponentInChildren<MeshRenderer>().material);
            CreateVMR(visionsMaterial, "VisionsMaterial");

            //Dunestrider Material
            dunestriderMat = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/ClayBossBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[2].defaultMaterial);
            CreateVMR(dunestriderMat, "DunestriderMaterial");

            //Lunar Flame Material
            lunarFlameMat = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/Effects/MuzzleFlashes/MuzzleflashLunarGolemTwinShot").transform.Find("FlameCloud_Ps").GetComponent<ParticleSystemRenderer>().material);
            CreateVMR(lunarFlameMat, "LunarFlameMaterial");

            //Greater Wisp Body Material
            greaterWispMat = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/GreaterWispBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial);
            CreateVMR(greaterWispMat, "GreaterWispMaterial");

            //Greater Wisp Flame Material
            greaterWispFlameMat = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/GreaterWispBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[1].defaultMaterial);
            CreateVMR(greaterWispFlameMat, "GreaterWispFlameMaterial");

            //Wisp Flame Material
            wispFlameMat = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/WispBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[1].defaultMaterial);
            CreateVMR(wispFlameMat, "LesserWispFlameMaterial");

            //Lunar Golem Chimera Material
            lunarGolemMat = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/LunarGolemBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial);
            CreateVMR(lunarGolemMat, "LunarGolemMaterial");

            //Aurelionite Material
            titanGoldMat = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/TitanGoldBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[19].defaultMaterial);
            CreateVMR(titanGoldMat, "AurelioniteMaterial");

            skeletalMat = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/Effects/BrotherDashEffect").transform.Find("Donut").GetComponent<ParticleSystemRenderer>().material);
            CreateVMR(skeletalMat, "SkeletalMaterial");

            shatterspleenMat = UnityEngine.Object.Instantiate(IDRS.FindDisplayRuleGroup(RoR2Content.Items.BleedOnHitAndExplode).rules[0].followerPrefab.GetComponentInChildren<MeshRenderer>().material);
            CreateVMR(shatterspleenMat, "ShatterspleenMaterial");
        }
        public void CreateVMR(Material material, string identifier)
        {
            VariantMaterialReplacement variantMaterialReplacement = ScriptableObject.CreateInstance<VariantMaterialReplacement>();
            variantMaterialReplacement.identifier = identifier;
            variantMaterialReplacement.material = material;

            completeVariantsMaterials.Add(variantMaterialReplacement);
        }
    }
}*/