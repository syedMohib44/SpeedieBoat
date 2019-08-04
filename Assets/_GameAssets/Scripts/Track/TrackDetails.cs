using System;
using System.Collections.Generic;
using UnityEngine;



namespace SpeedyBoat
{
    [Serializable]
    public class TrackDetails
    {
        public int                  PoleCount = 32;

        public float                CurveWidthScale = .5f;

        public float                Height, StartLineDist, EndLineDist;

        public float                WaterLevel      = .065f;
        public float                WaterWidth      = 1.85f;

        public float                BaseWidth       = 2f;
        public float                BaseHeight      = .01f;

        public float                WallWidth       = .075f;
        public float                WallHeight      = .1f;

        public int                  SegmentsCount   = 256;

        public GameObject           Splines;
        public Material             StructureMaterial, WaterMaterial, WaterMaterial2, CurveMaterial;

        public List<Curve>          Curves = new List<Curve>();
        public List<Prop>           Props = new List<Prop>();
        public List<Enemy>          Enemies = new List<Enemy>();
        public List<PickupSpawner>  PickupSpawners = new List<PickupSpawner>();



        // 0 is no Curve, -1 is left and 1 is right
        public int GetExtendedSide(int segmentIndex)
        {
            foreach(var curve in Curves)
            {
                if(segmentIndex >= curve.Start && segmentIndex < curve.Start + curve.Length)
                {
                    return curve.Side;
                }
            }

            return 0;
        }



        [Serializable]
        public class Curve
        {
            public int Side = 1;
            public int Start, Length;
        }



        [Serializable]
        public class Prop
        {
            public PropType     Type;
            public float        DistStart;
        }



        [Serializable]
        public class Enemy
        {
            public EnemyType    Type;
            public int          Start, Count;
            public float        DistStart, ActivateInterval, ForwardSpeed, LateralTime;
        }



        [Serializable]
        public class PickupSpawner
        {
            public PickupType   Type;
            public int          Start, Count;
            public float        DistStart, ActivateInterval, ForwardSpeed, LateralTime;
        }
    }
}