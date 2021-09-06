using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace NebbysWrath.VariantEntityStates.GreaterWisp.DeathStates
{
    public class AmalgamatedGreaterWisp : GenericCharacterDeath
    {
		public GameObject initialEffect;

		[SerializeField]
		public GameObject deathEffect;

		private static float duration = 2f;

		private GameObject initialEffectInstance;

		public override void OnEnter()
		{
			base.OnEnter();
			if (!base.modelLocator)
			{
				return;
			}
			ChildLocator component = base.modelLocator.modelTransform.GetComponent<ChildLocator>();
			if ((bool)component)
			{
				Transform transform = component.FindChild("Mask");
				transform.gameObject.SetActive(value: true);
				transform.GetComponent<AnimateShaderAlpha>().timeMax = duration;
				if ((bool)initialEffect)
				{
					initialEffectInstance = Object.Instantiate(initialEffect, transform.position, transform.rotation, transform);
				}
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= duration && NetworkServer.active)
			{
				if ((bool)deathEffect)
				{
					EffectManager.SpawnEffect(deathEffect, new EffectData
					{
						origin = base.transform.position
					}, transmit: true);
				}
				SpawnWisps();
				EntityState.Destroy(base.gameObject);
			}
		}
		public void SpawnWisps()
        {
			if (NetworkServer.active)
			{
				for (int i = 0; i < 5; i++)
				{
					Vector3 position = base.characterBody.corePosition + (5 * UnityEngine.Random.insideUnitSphere);

					DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest((SpawnCard)Resources.Load(string.Format("SpawnCards/CharacterSpawnCards/cscLesserWisp")), new DirectorPlacementRule
					{
						placementMode = DirectorPlacementRule.PlacementMode.Direct,
						minDistance = 0f,
						maxDistance = 0f,
						position = position
					}, RoR2Application.rng);

					directorSpawnRequest.summonerBodyObject = base.gameObject;

					GameObject jelly = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
					if(jelly)
                    {
						jelly.GetComponent<CharacterBody>().AddTimedBuff(RoR2Content.Buffs.Immune, 1);
					}
				}
				DestroyBodyAsapServer();
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			if ((bool)initialEffectInstance)
			{
				EntityState.Destroy(initialEffectInstance);
			}
		}
	}
}
