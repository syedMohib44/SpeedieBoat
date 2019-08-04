


namespace SpeedyBoat
{
    public class JumpRampPropInitialiser : RampPropInitialiser
    {
        public readonly bool Elevate;



        public JumpRampPropInitialiser(Track track, float distStart, bool elevate)
            : base(track, distStart)
        {
            Elevate = elevate;
        }
    }
}