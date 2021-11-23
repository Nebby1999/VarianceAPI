

using EntityStates;
using EntityStates.BeetleQueenMonster;
using RoR2;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace NebbysWrath.VariantEntityStates.BeetleQueen
{
    public class BeetleSwarm : BaseState
    {
        public static float baseDuration = 9f;

        public static string attackSoundString = "Play_beetle_queen_attack1";

        public static float randomRadius = 10f;

        public static GameObject spitPrefab = SummonEggs.spitPrefab;

        public static int maxBeetleCount = 10;

        public static float summonBeetleInterval = 0.30f;


        public static SpawnCard beetleSpawnCard = Resources.Load<SpawnCard>("SpawnCards/CharacterSpawnCards/cscbeetle");

        private Animator animator;

        private Transform modelTransform;

        private ChildLocator childLocator;

        private float duration;

        private float summonBeetleTimer;

        private int beetleSummonCount;

        private bool isSummoning;

        private BullseyeSearch enemySearch;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            modelTransform = GetModelTransform();
            childLocator = modelTransform.GetComponent<ChildLocator>();
            duration = baseDuration;
            PlayCrossfade("Gesture", "SummonEggs", 0.5f);
            Util.PlaySound(attackSoundString, base.gameObject);
            if (NetworkServer.active)
            {
                enemySearch = new BullseyeSearch();
                enemySearch.filterByDistinctEntity = false;
                enemySearch.filterByLoS = false;
                enemySearch.maxDistanceFilter = float.PositiveInfinity;
                enemySearch.minDistanceFilter = 0f;
                enemySearch.minAngleFilter = 0f;
                enemySearch.maxAngleFilter = 180f;
                enemySearch.teamMaskFilter = TeamMask.GetEnemyTeams(GetTeam());
                enemySearch.sortMode = BullseyeSearch.SortMode.Distance;
                enemySearch.viewer = base.characterBody;
            }
        }

        private void SummonBeetleEgg()
        {
            Vector3 searchOrigin = GetAimRay().origin;
            if ((bool)base.inputBank && base.inputBank.GetAimRaycast(float.PositiveInfinity, out var hitInfo))
            {
                searchOrigin = hitInfo.point;
            }
            if (enemySearch == null)
            {
                return;
            }
            enemySearch.searchOrigin = searchOrigin;
            enemySearch.RefreshCandidates();
            HurtBox hurtBox = enemySearch.GetResults().FirstOrDefault();
            Transform transform = (((bool)hurtBox && (bool)hurtBox.healthComponent) ? hurtBox.healthComponent.body.coreTransform : base.characterBody.coreTransform);
            if ((bool)transform)
            {
                DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(beetleSpawnCard, new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    minDistance = 3f,
                    maxDistance = 20f,
                    spawnOnTarget = transform,
                }, RoR2Application.rng);
                directorSpawnRequest.summonerBodyObject = base.gameObject;
                directorSpawnRequest.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(directorSpawnRequest.onSpawnedServer, (Action<SpawnCard.SpawnResult>)delegate (SpawnCard.SpawnResult spawnResult)
                {
                    spawnResult.spawnedInstance.GetComponent<Inventory>().CopyEquipmentFrom(base.characterBody.inventory);
                });
                DirectorCore.instance?.TrySpawnObject(directorSpawnRequest);
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            bool flag = animator.GetFloat("SummonEggs.active") > 0.9f;
            if (flag && !isSummoning)
            {
                string muzzleName = "Mouth";
                EffectManager.SimpleMuzzleFlash(spitPrefab, base.gameObject, muzzleName, transmit: false);
            }
            if (isSummoning)
            {
                summonBeetleTimer += Time.fixedDeltaTime;
                if (NetworkServer.active && summonBeetleTimer > 0f && beetleSummonCount < maxBeetleCount)
                {
                    beetleSummonCount++;
                    summonBeetleTimer -= summonBeetleInterval;
                    SummonBeetleEgg();
                }
            }
            isSummoning = flag;
            if (base.fixedAge >= duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
