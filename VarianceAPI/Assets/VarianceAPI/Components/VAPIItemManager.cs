using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;
using VarianceAPI.Modules;

namespace VarianceAPI.Components
{
    [RequireComponent(typeof(CharacterBody))]
    public class VAPIItemManager : MonoBehaviour
    {
        private CharacterBody body;
        IStatItemBehavior[] statItemBehaviors = Array.Empty<IStatItemBehavior>();

        void Awake()
        {
            body = gameObject.GetComponent<CharacterBody>();
            body.onInventoryChanged += CheckForSS2Items;
        }

        public void CheckForSS2Items()
        {
            //It seems counter-intuitive to add an item behavior for something even if it has none of them, but the game actually destroys the behavior if there isn't one which is what we want and it doesn't add a component if it doesn't have any of the item
            foreach (var item in Pickups.items)
                item.Value.AddBehavior(ref body, body.inventory.GetItemCount(item.Key.itemIndex));
            GetInterfaces();
        }

        private void GetInterfaces()
        {
            statItemBehaviors = GetComponents<IStatItemBehavior>();
            if (NetworkServer.active)
            {
                body.healthComponent.onIncomingDamageReceivers = GetComponents<IOnIncomingDamageServerReceiver>();
                body.healthComponent.onTakeDamageReceivers = GetComponents<IOnTakeDamageServerReceiver>();
            }
        }


        public void RunStatRecalculationsStart()
        {
            foreach (var statBehavior in statItemBehaviors)
                statBehavior.RecalcStatsStart();
        }
        public void RunStatRecalculationsEnd()
        {
            foreach (var statBehavior in statItemBehaviors)
                statBehavior.RecalcStatsEnd();
        }

    }
}