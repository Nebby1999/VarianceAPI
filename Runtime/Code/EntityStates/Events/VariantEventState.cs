using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAPI;
using VAPI.Components;

namespace EntityStates.Events.VariantEvents
{
    public abstract class VariantEventState : EventState
    {
        protected VariantSpawnManager SpawnManager { get; private set; }

        public override void OnEnter()
        {
            SpawnManager = Run.instance.GetComponentInChildren<VariantSpawnManager>();
            if(!SpawnManager)
            {
                VAPILog.Fatal($"A {GetType().Name} is trying to begin, but there is no VariantSpawnManager! the Event Card {eventCard} should add the VarianceAPI Expansion Def to it's required expansions array to avoid this issue! returning to main state.");
                outer.SetNextStateToMain();
                return;
            }
            base.OnEnter();
        }
        public override void OnExit()
        {
            if (!SpawnManager)
                return;

            base.OnExit();
        }
    }
}
