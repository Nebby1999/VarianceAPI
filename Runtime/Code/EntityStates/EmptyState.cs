using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates
{
    public class EmptyState : BaseState
    {
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            outer.SetNextStateToMain();
        }
    }
}