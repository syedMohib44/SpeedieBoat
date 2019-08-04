using UnityEngine;



namespace SpeedyBoat
{
    public class LevelJumpRampProp : RampProp
    {
        public override void Setup(PropType type, Props container, PropInitialiser initialiser)
        {
            base.Setup(type, container, initialiser);
            ChangeState(State.HoldingUp);
        }



        protected virtual void Update()
        {
        }
    }
}