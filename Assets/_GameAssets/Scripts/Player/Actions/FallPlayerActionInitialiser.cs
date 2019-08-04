using UnityEngine;



namespace SpeedyBoat
{
    public class FallPlayerActionInitialiser : PlayerActionInitialiser
    {
        public readonly float   GroundHeight;
        public readonly Vector3 LateralNormal;



        public FallPlayerActionInitialiser(Vector3 lateralNormal, float groundHeight)
        {
            LateralNormal = lateralNormal;
            GroundHeight = groundHeight;
        }
    }
}