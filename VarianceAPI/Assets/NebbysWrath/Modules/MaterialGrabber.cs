/*using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VarianceAPI.Modules;
using VarianceAPI.Scriptables;
using Object = UnityEngine.Object;

namespace NebbysWrath
{
    public class MaterialGrabber : VariantMaterialGrabber
    {
        public ItemDisplayRuleSet IDRS;
        public Material archWispMat;
        public Material archWispFireMat;
        public Material stoneGolemMat;
        public Material stoneGolemClap;
        public Material lesserWispMat;
        public Material lesserWispFireMat;
        public Material perforatorMat;
        public Material spectralMat;
        public Material solusProbeMat;

        public void StartGrabber(AssetBundle assets)
        {
            assetBundle = assets;
            CreateCorrectMaterials();
            Init();
        }

        public void CreateCorrectMaterials()
        {
            IDRS = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet;
            //Archaic Wisp Material
            archWispMat = Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/ArchWispBody").GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().baseRendererInfos[0].defaultMaterial);
            CreateVMR(archWispMat, "ArchaicWispMaterial");

            //Archaic Wisp Fire
            archWispFireMat = Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/ArchWispBody").GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().baseRendererInfos[1].defaultMaterial);
            CreateVMR(archWispFireMat, "ArchaicWispFireMaterial");

            //Stone Golem Material
            stoneGolemMat = Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/GolemBody").GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().baseRendererInfos[0].defaultMaterial);
            CreateVMR(stoneGolemMat, "StoneGolemMaterial");

            //Lesser Wisp Material
            lesserWispMat = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/WispBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial);
            CreateVMR(lesserWispMat, "LesserWispMaterial");

            //Lesser Wisp Fire Material
            lesserWispFireMat = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/WispBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[1].defaultMaterial);
            CreateVMR(lesserWispFireMat, "LesserWispFireMaterial");

            //Perforator Material
            perforatorMat = UnityEngine.Object.Instantiate(IDRS.FindDisplayRuleGroup(RoR2Content.Items.FireballsOnHit).rules[0].followerPrefab.GetComponentInChildren<MeshRenderer>().material);
            CreateVMR(perforatorMat, "PerforatorMaterial");

            //Spectral Material
            spectralMat = Resources.Load<Material>("Materials/matGhostEffect");
            CreateVMR(spectralMat, "SpectralMaterial");

            //Solus Probe Mat
            solusProbeMat = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/RoboBallBossBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial);
            CreateVMR(solusProbeMat, "SolusProbeMaterial");

        }
        public void CreateVMR(Material material, string identifier)
        {
            VariantMaterialReplacement variantMaterialReplacement = ScriptableObject.CreateInstance<VariantMaterialReplacement>();
            variantMaterialReplacement.identifier = identifier;
            variantMaterialReplacement.material = material;

            completeVariantsMaterials.Add(variantMaterialReplacement);
        }
    }
}
*/