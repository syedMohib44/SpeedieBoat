


namespace SpeedyBoat
{
    public class RampPropInitialiser : PropInitialiser
    {
        public readonly float DistStart;



        public RampPropInitialiser(Track track, float distStart)
            : base(track)
        {
            DistStart = distStart;
        }
    }
}