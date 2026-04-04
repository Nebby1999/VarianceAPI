namespace EntityStates
{
    public class GoToMain : EntityState
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