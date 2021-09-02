using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VarianceAPI.Modules;
using VarianceAPI;
using VarianceAPI.Scriptables;
using Object = UnityEngine.Object;

namespace NebbysWrath
{
    public static class MaterialGrabber
    {
        public static ItemDisplayRuleSet IDRS;

        public static List<(string, Material)> VanillaMaterials = new List<(string, Material)>();

        public static void CreateCorrectMaterials()
        {
            IDRS = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet;

            //Archaic Wisp Material
            VanillaMaterials.Add(("NW_ArchaicWispMaterial", Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/ArchWispBody").GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().baseRendererInfos[0].defaultMaterial)));

            //Archaic Wisp Fire
            VanillaMaterials.Add(("NW_ArchaicWispFireMaterial", Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/ArchWispBody").GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().baseRendererInfos[1].defaultMaterial)));

            //Stone Golem Material
            VanillaMaterials.Add(("NW_StoneGolemMaterial", Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/GolemBody").GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().baseRendererInfos[0].defaultMaterial)));

            //Lesser Wisp Material
            VanillaMaterials.Add(("NW_LesserWispMaterial", UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/WispBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial)));

            //Lesser Wisp Fire Material
            VanillaMaterials.Add(("NW_LesserWispFireMaterial", UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/WispBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[1].defaultMaterial)));

            //Perforator Material
            VanillaMaterials.Add(("NW_PerforatorMaterial", UnityEngine.Object.Instantiate(IDRS.FindDisplayRuleGroup(RoR2Content.Items.FireballsOnHit).rules[0].followerPrefab.GetComponentInChildren<MeshRenderer>().material)));

            //Spectral Material
            VanillaMaterials.Add(("NW_SpectralMaterial", Resources.Load<Material>("Materials/matGhostEffect")));

            //Solus Probe Mat
            VanillaMaterials.Add(("NW_SolusProbeMaterial", UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/RoboBallBossBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial)));

            //OnFire Mat
            VanillaMaterials.Add(("NW_OnFireMaterial", Resources.Load<Material>("Materials/matOnFire")));

            //GolemElectric
            VanillaMaterials.Add(("NW_GolemElectric", Resources.Load<GameObject>("Prefabs/GolemClapCharge").GetComponentInChildren<ParticleSystemRenderer>().material));

            VariantMaterialGrabber.StoreMaterials(VanillaMaterials);
        }
    }
}
