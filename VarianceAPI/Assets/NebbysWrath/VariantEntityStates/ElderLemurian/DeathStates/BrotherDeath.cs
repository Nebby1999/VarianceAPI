using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace NebbysWrath.VariantEntityStates.ElderLemurian.DeathStates
{
    public class BrotherDeath : GenericCharacterDeath
    {
        public override void OnEnter()
        {
            base.OnEnter();
            if(NetworkServer.active)
            {
                for(int i = 0; i < 3; i++)
                {
                    Vector3 position = base.characterBody.corePosition + (5 * UnityEngine.Random.insideUnitSphere);

                    DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest((SpawnCard)Resources.Load(string.Format("SpawnCards/CharacterSpawnCards/cscLemurian")), new DirectorPlacementRule
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                        minDistance = 3f,
                        maxDistance = 6f,
                        position = position
                    }, RoR2Application.rng);

                    directorSpawnRequest.summonerBodyObject = base.gameObject;

                    GameObject lemmy = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
                    if (lemmy)
                    {
                        CharacterMaster master = lemmy.GetComponent<CharacterMaster>();
                        lemmy.GetComponent<Inventory>().SetEquipmentIndex(base.characterBody.inventory.currentEquipmentIndex);
                        lemmy.GetComponent<Inventory>().GetComponentInParent<CharacterMaster>().GetBody().modelLocator.modelTransform.GetComponent<CharacterModel>().baseRendererInfos[0].defaultMaterial = Resources.Load<Material>("Materials/matGhostEffect");
                        lemmy.GetComponent<CharacterBody>().AddTimedBuff(RoR2Content.Buffs.Immune, 1);
                    }
                }
                DestroyBodyAsapServer();
            }
        }
    }
}
