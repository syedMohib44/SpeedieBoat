using UnityEngine;



namespace SpeedyBoat
{
    public class JumpRampProp : RampProp
    {
        public override bool ShouldElevate
        {
            get { return ((JumpRampPropInitialiser)m_initialiser).Elevate; }
        }
    }
}