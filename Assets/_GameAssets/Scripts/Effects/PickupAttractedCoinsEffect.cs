

namespace SpeedyBoat
{
    public class PickupAttractedCoinsEffect : AttractedCoinsEffect
    {
        public void SpawnCoin()
        {
            m_particleSystem.Emit(1);
        }
    }
}