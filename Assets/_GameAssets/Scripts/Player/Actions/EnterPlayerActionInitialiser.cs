

namespace SpeedyBoat
{
    public class EnterPlayerActionInitialiser : PlayerActionInitialiser
    {
        public readonly float DropSpeed, DropHeight;



        public EnterPlayerActionInitialiser(float dropSpeed, float dropHeight)
        {
            DropSpeed = dropSpeed;
            DropHeight = dropHeight;
        }
    }
}