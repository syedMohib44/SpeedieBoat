

namespace SpeedyBoat
{
    public class SinkPlayerActionInitialiser : PlayerActionInitialiser
    {
        public readonly float DeathDelay;



        public SinkPlayerActionInitialiser(float deathDelay)
        {
            DeathDelay = deathDelay;
        }
    }
}