using EntityStates;
using EntityStates.Bell.BellWeapon;
using R2API;
using RoR2;
using RoR2.Projectile;
using System.Globalization;
using UnityEngine;

namespace NebbysWrath.VariantEntityStates.BrassContraption
{
    public class MegaBall : BaseState
    {
        public static float basePrepDuration;

        public static float baseTimeBetweenPreps;

        public static GameObject preppedBombPrefab;

        public static float baseBarrageDuration;

        public static float baseTimeBetweenBarrages;

        public static GameObject bombProjectilePrefab;

        public static GameObject muzzleflashPrefab;

        public static float damageCoefficient;

        public static float force;

        public static float selfForce;

        private static bool madeClone = false;

        private float prepDuration;

        private float timeBetweenPreps;

        private float barrageDuration;

        private float timeBetweenBarrages;

        private ChildLocator childLocator;

        private GameObject preppedBombPrefabInstance;

        private int currentBombIndex;

        private float perProjectileStopwatch;

        public override void OnEnter()
        {
            if (!madeClone)
            {
                madeClone = true;
                preppedBombPrefab = PrefabAPI.InstantiateClone(ChargeTrioBomb.preppedBombPrefab, "preppedBomb");
                preppedBombPrefab.GetComponentInChildren<MeshRenderer>().material = MainClass.nebbysWrathAssets.LoadAsset<Material>("matSteelContraption");
            }
            basePrepDuration = ChargeTrioBomb.basePrepDuration;
            baseTimeBetweenPreps = ChargeTrioBomb.baseTimeBetweenPreps;
            baseBarrageDuration = ChargeTrioBomb.baseBarrageDuration;
            baseTimeBetweenBarrages = ChargeTrioBomb.baseTimeBetweenBarrages;
            muzzleflashPrefab = ChargeTrioBomb.muzzleflashPrefab;
            damageCoefficient = ChargeTrioBomb.damageCoefficient;
            force = ChargeTrioBomb.force * 2;
            selfForce = ChargeTrioBomb.selfForce * 2;
            currentBombIndex = 1;
            bombProjectilePrefab = Projectiles.SteelBall.projectile;
            base.OnEnter();
            prepDuration = basePrepDuration / attackSpeedStat;
            timeBetweenPreps = baseTimeBetweenPreps / attackSpeedStat;
            barrageDuration = baseBarrageDuration / attackSpeedStat;
            timeBetweenBarrages = baseTimeBetweenBarrages / attackSpeedStat;
            Transform modelTransform = GetModelTransform();
            if ((bool)modelTransform)
            {
                childLocator = modelTransform.GetComponent<ChildLocator>();
            }
        }

        private string FindTargetChildStringFromBombIndex()
        {
            return string.Format(CultureInfo.InvariantCulture, "ProjectilePosition{0}", 2);
        }

        private Transform FindTargetChildTransformFromBombIndex()
        {
            string childName = FindTargetChildStringFromBombIndex();
            return childLocator.FindChild(childName);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            perProjectileStopwatch += Time.fixedDeltaTime;
            if (base.fixedAge < prepDuration)
            {
                if (perProjectileStopwatch > timeBetweenPreps && currentBombIndex < 2)
                {
                    currentBombIndex++;
                    perProjectileStopwatch = 0f;
                    Transform transform = FindTargetChildTransformFromBombIndex();
                    if ((bool)transform)
                    {
                        preppedBombPrefabInstance = Object.Instantiate(preppedBombPrefab, transform);
                        preppedBombPrefabInstance.transform.localScale *= 4;
                    }
                }
            }
            else if (base.fixedAge < prepDuration + barrageDuration)
            {
                if (!(perProjectileStopwatch > timeBetweenBarrages) || currentBombIndex <= 1)
                {
                    return;
                }
                perProjectileStopwatch = 0f;
                Ray aimRay = GetAimRay();
                Transform transform2 = FindTargetChildTransformFromBombIndex();
                if ((bool)transform2)
                {
                    if (base.isAuthority)
                    {
                        ProjectileManager.instance.FireProjectile(bombProjectilePrefab, transform2.position, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master));
                        Rigidbody component = GetComponent<Rigidbody>();
                        if ((bool)component)
                        {
                            component.AddForceAtPosition((0f - selfForce) * transform2.forward, transform2.position);
                        }
                    }
                    EffectManager.SimpleMuzzleFlash(muzzleflashPrefab, base.gameObject, FindTargetChildStringFromBombIndex(), transmit: false);
                }
                currentBombIndex--;
                EntityState.Destroy(preppedBombPrefabInstance);
            }
            else if (base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            EntityState.Destroy(preppedBombPrefabInstance);
        }
    }
}