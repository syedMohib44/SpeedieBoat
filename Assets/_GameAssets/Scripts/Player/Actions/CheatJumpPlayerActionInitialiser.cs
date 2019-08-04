using UnityEngine;



namespace SpeedyBoat
{
    public class CheatJumpPlayerActionInitialiser : PlayerActionInitialiser
    {
        public readonly Vector3 LandPos;
        public readonly float   LaunchAngle, JumpHeight, JumpTime, JumpScale, JumpBounces, FlipTime;



        public CheatJumpPlayerActionInitialiser(Vector3 landPos, float launchAngle, float jumpHeight, float jumpTime, int jumpBounces)
        {
            LandPos     = landPos;
            LaunchAngle = launchAngle;
            JumpHeight  = jumpHeight;
            JumpTime    = jumpTime;
            JumpBounces = jumpBounces;
        }
    }
}