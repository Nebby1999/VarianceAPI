namespace EntityStates
{
    /// <summary>
    /// An entity state that immedaitly sets the next state to main. A skilldef with this state is assigned to a generic slot if <see cref="VAPI.VariantSkillReplacement.skillDef"/> is left null
    /// </summary>
    public sealed class GoToMain : EntityState
    {
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}