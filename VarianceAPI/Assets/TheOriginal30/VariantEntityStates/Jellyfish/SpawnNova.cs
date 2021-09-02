using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TheOriginal30.VariantEntityStates.Jellyfish
{
    public class SpawnNova : BaseState
    {
        public static float baseDuration = 0.5f;
        public static int jellyCount = 5;
        public static float jellyDropRadius = 5;

        private bool hasExploded;
        private float duration;
        private float stopwatch;

        private GameObject chargeEffect;
        private PrintController printController;
        private uint soundID;

        public override void OnEnter()
        {
            base.OnEnter();
            this.stopwatch = 0f;
            this.duration = SpawnNova.baseDuration / this.attackSpeedStat;
            Transform modelTransform = base.GetModelTransform();

            base.PlayCrossfade("Body", "Nova", "Nova.playbackRate", this.duration, 0.1f);
            this.soundID = Util.PlaySound(EntityStates.JellyfishMonster.JellyNova.chargingSoundString, base.gameObject);

            if (EntityStates.JellyfishMonster.JellyNova.chargingEffectPrefab)
            {
                this.chargeEffect = UnityEngine.Object.Instantiate<GameObject>(EntityStates.JellyfishMonster.JellyNova.chargingEffectPrefab, base.transform.position, base.transform.rotation);
                this.chargeEffect.transform.parent = base.transform;
                this.chargeEffect.transform.localScale = Vector3.one * NuclearNova.novaRadius;
                this.chargeEffect.GetComponent<ScaleParticleSystemDuration>().newDuration = this.duration;
            }

            if (modelTransform)
            {
                this.printController = modelTransform.GetComponent<PrintController>();
                if (this.printController)
                {
                    this.printController.enabled = true;
                    this.printController.printTime = this.duration;
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (this.chargeEffect) EntityState.Destroy(this.chargeEffect);
            if (this.printController) this.printController.enabled = false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.fixedDeltaTime;

            if (this.stopwatch >= this.duration && base.isAuthority && !this.hasExploded)
            {
                this.Detonate();
                return;
            }
        }

        private void Detonate()
        {
            this.hasExploded = true;
            Util.PlaySound(EntityStates.JellyfishMonster.JellyNova.novaSoundString, base.gameObject);

            if (base.modelLocator)
            {
                if (base.modelLocator.modelBaseTransform)
                {
                    EntityState.Destroy(base.modelLocator.modelBaseTransform.gameObject);
                }
                if (base.modelLocator.modelTransform)
                {
                    EntityState.Destroy(base.modelLocator.modelTransform.gameObject);
                }
            }

            if (this.chargeEffect)
            {
                EntityState.Destroy(this.chargeEffect);
            }

            if (EntityStates.JellyfishMonster.JellyNova.novaEffectPrefab)
            {
                EffectManager.SpawnEffect(EntityStates.JellyfishMonster.JellyNova.novaEffectPrefab, new EffectData
                {
                    origin = base.transform.position,
                    scale = NuclearNova.novaRadius
                }, true);
            }

            if (NetworkServer.active)
            {
                for (int i = 0; i < SpawnNova.jellyCount; i++)
                {
                    Vector3 position = base.characterBody.corePosition + (SpawnNova.jellyDropRadius * UnityEngine.Random.insideUnitSphere);

                    DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest((SpawnCard)Resources.Load(string.Format("SpawnCards/CharacterSpawnCards/cscJellyfish")), new DirectorPlacementRule
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.Direct,
                        minDistance = 0f,
                        maxDistance = 0f,
                        position = position
                    }, RoR2Application.rng);

                    directorSpawnRequest.summonerBodyObject = base.gameObject;

                    GameObject jelly = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
                    if (jelly)
                    {
                        CharacterMaster master = jelly.GetComponent<CharacterMaster>();
                        jelly.GetComponent<Inventory>().SetEquipmentIndex(base.characterBody.inventory.currentEquipmentIndex);
                    }
                }
            }

            if (base.healthComponent) base.healthComponent.Suicide(null, null, DamageType.Generic);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
