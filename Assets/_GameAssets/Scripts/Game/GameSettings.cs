using System;
using UnityEngine;
using UnityEngine.Serialization;



// A instanced prefab that holds global values, can be modified through inspector during gameplay for tweaking purposes
// Available from anywhere in code, usage example: GameSettings.Instance().Player.Health_ETC;
namespace SpeedyBoat
{
    public class GameSettings : MonoBehaviour
    {
        public string           ItunesAppID, AppsflyerDevKey;

        public int              FlipBonusBaseScore = 5;
        public int[]            PositionBonuses;

        public AudioClip[]      SoundFX;

        public Sprite           SoundOn, SoundOff, HapticOn, HapticOff;

        public PhysicMaterial   WaterMaterial, StructureMaterial;

        public Texture2D        ChequerTexture;

        public GameObject[]     BoatModels;

        public int              MaxTrackDecals = 16;
        public GameObject       TrackDecalsPrefab;

        public int              MaxAIPlayers = 8;
        public GameObject       AIPlayerPrefab;

        public int              MaxSmokePuffs = 8;
        public GameObject       SmokePuffPrefab;

        public int              MaxBonusEffects = 8;
        public GameObject       BonusEffectPrefab;

        public int              MaxProps = 8;
        public GameObject       PropsPrefab;

        public int              MaxEnemies = 8;
        public GameObject       EnemiesPrefab;

        public int              MaxPickups = 8;
        public GameObject       PickupsPrefab;

        public Color[]          PlayerColors;

        public PlayerDetails    Player;
        public LevelDetails[]   Levels;

        public AIDetails[]      AIPlayers;

        public GameObject       TrackCurvePrefab, TrackCurveSegmentPrefab;



        [Serializable]
        public class PlayerDetails
        {
            public float        BoostTime = 4;
            public float        BoostSpeedScale = 1.5f;

            public float        HitSpeedScale = .5f;
            public float        PerfectSpeedScale = .5f;

            public GameObject   Prefab;
            public Vector3      CameraOffset;
        }



        [Serializable]
        public class LevelDetails
        {
            public string _;

            public int          AIPlayerCount = 3;

            [FormerlySerializedAs("Prefab")]
            public GameObject   TrackPrefab;
            public GameObject   ScenePrefab;
        }



        [Serializable]
        public class AIDetails
        {
            public string   Name;
            public float    Speed = 1;
        }



        public static GameSettings Instance()
        {
            if (ms_settings == null)
            {
#if UNITY_EDITOR
                // More prefab viewing shinnanigans
                if(!Application.isPlaying)
                {
                    var existing = GameObject.Find("GameSettings");
                    if(existing)
                    {
                        DestroyImmediate(existing);
                    }
                }
#endif
                ms_settings = Instantiate(Resources.Load<GameSettings>("GameSettings"));
                ms_settings.name = "GameSettings";
            }

            return ms_settings;
        }



        private static GameSettings ms_settings;
    }
}