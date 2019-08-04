

namespace SpeedyBoat
{
    public class UpgradePlayerActionInitialiser : PlayerActionInitialiser
    {
        public readonly float DropSpeed, DropHeight;



        public UpgradePlayerActionInitialiser(float dropSpeed, float dropHeight)
        {
            DropSpeed = dropSpeed;
            DropHeight = dropHeight;
        }
    }
}