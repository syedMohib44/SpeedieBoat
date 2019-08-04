


namespace SpeedyBoat
{
    public class CoinPickupInitialiser : PickupInitialiser
    {
        public readonly int   GroupIndex;
        public readonly float DistStart, ForwardSpeed, LateralTime, ActiveDelay;



        public CoinPickupInitialiser(Track track, int groupIndex, float distStart, float forwardSpeed, float lateralTime, float activeDelay)
            : base(track)
        {
            GroupIndex = groupIndex;
            DistStart = distStart;
            ForwardSpeed = forwardSpeed;
            LateralTime = lateralTime;
            ActiveDelay = activeDelay;
        }
    }
}