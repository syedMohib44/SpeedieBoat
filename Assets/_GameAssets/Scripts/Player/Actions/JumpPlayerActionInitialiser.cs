

namespace SpeedyBoat
{
    public class JumpPlayerActionInitialiser : PlayerActionInitialiser
    {
        public readonly float LaunchAngle, JumpHeight, JumpTime, JumpScale, JumpBounces, FlipTime;



        public JumpPlayerActionInitialiser(float launchAngle, float jumpHeight, float jumpTime, float jumpScale, int jumpBounces, float flipTime)
        {
            LaunchAngle = launchAngle;
            JumpHeight  = jumpHeight;
            JumpTime    = jumpTime;
            JumpScale   = jumpScale;
            JumpBounces = jumpBounces;
            FlipTime    = flipTime;
        }
    }
}