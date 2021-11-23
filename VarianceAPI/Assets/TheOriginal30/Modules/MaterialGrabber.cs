using RoR2;
using System.Collections.Generic;
using UnityEngine;
using VarianceAPI;

namespace TheOriginal30
{
    public static class MaterialGrabber
    {
        public static ItemDisplayRuleSet IDRS;

        public static List<(string, Material)> VanillaMaterials = new List<(string, Material)>();

        public static void CreateCorrectMaterials()
        {
            IDRS = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet;

            //Perforator mat
            VanillaMaterials.Add(("TO30_PerforatorMaterial", UnityEngine.Object.Instantiate(IDRS.FindDisplayRuleGroup(RoR2Content.Items.FireballsOnHit).rules[0].followerPrefab.GetComponentInChildren<MeshRenderer>().material)));

            //Beetle Gland Material
            VanillaMaterials.Add(("TO30_GlandMaterial", UnityEngine.Object.Instantiate(IDRS.FindDisplayRuleGroup(RoR2Content.Items.BeetleGland).rules[0].followerPrefab.GetComponentInChildren<Renderer>().material)));

            //Ghost Effect Material
            VanillaMaterials.Add(("TO30_GhostMaterial", Resources.Load<Material>("Materials/matGhostEffect")));

            //fireTrail Material
            VanillaMaterials.Add(("TO30_FireTrailMaterial", UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/ProjectileGhosts/FireMeatBallGhost").GetComponentInChildren<TrailRenderer>().material)));

            //solus Material
            VanillaMaterials.Add(("TO30_SolusMaterial", UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/RoboBallBossBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial)));

            //Visions Material
            VanillaMaterials.Add(("TO30_VisionsMaterial", UnityEngine.Object.Instantiate(IDRS.FindDisplayRuleGroup(RoR2Content.Items.LunarPrimaryReplacement).rules[0].followerPrefab.GetComponentInChildren<MeshRenderer>().material)));

            //Dunestrider Material
            VanillaMaterials.Add(("TO30_DunestriderMaterial", UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/ClayBossBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[2].defaultMaterial)));

            //Lunar Flame Material
            VanillaMaterials.Add(("TO30_LunarFlameMaterial", UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/Effects/MuzzleFlashes/MuzzleflashLunarGolemTwinShot").transform.Find("FlameCloud_Ps").GetComponent<ParticleSystemRenderer>().material)));

            //Greater Wisp Body Material
            VanillaMaterials.Add(("TO30_GreaterWispMaterial", UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/GreaterWispBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial)));

            //Greater Wisp Flame Material
            VanillaMaterials.Add(("TO30_GreaterWispFlameMaterial", UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/GreaterWispBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[1].defaultMaterial)));

            //Wisp Flame Material
            VanillaMaterials.Add(("TO30_LesserWispFlameMaterial", UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/WispBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[1].defaultMaterial)));

            //Lunar Golem Chimera Material
            VanillaMaterials.Add(("TO30_LunarGolemMaterial", UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/LunarGolemBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial)));

            //Aurelionite Material
            VanillaMaterials.Add(("TO30_AurelioniteMaterial", UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/TitanGoldBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[19].defaultMaterial)));

            VanillaMaterials.Add(("TO30_SkeletalMaterial", UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/Effects/BrotherDashEffect").transform.Find("Donut").GetComponent<ParticleSystemRenderer>().material)));

            VanillaMaterials.Add(("TO30_ShatterspleenMaterial", UnityEngine.Object.Instantiate(IDRS.FindDisplayRuleGroup(RoR2Content.Items.BleedOnHitAndExplode).rules[0].followerPrefab.GetComponentInChildren<MeshRenderer>().material)));

            VariantMaterialGrabber.StoreMaterials(VanillaMaterials);
        }
    }
}