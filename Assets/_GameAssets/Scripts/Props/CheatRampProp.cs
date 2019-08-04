using UnityEngine;



namespace SpeedyBoat
{
    public class CheatRampProp : RampProp
    {
        protected override void Update()
        {
            base.Update();

            if(m_state == State.HoldingDown)
            {
                m_container.Free();
            }
        }
    }
}