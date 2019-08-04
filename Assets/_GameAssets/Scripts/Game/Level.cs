using UnityEngine;



namespace SpeedyBoat
{
    public class Level : MonoBehaviour
    {
        public Color    AmbientLight        = Color.white;

        public float    BaseHeight          = 1;
        public float    CheatRampChance     = 100;
        public float    CheatRampInterval   = 10;
        public Vector2  CoinIntervalRange   = new Vector2(1, 16);
        public Vector2  CoinCountRange      = new Vector2(3, 5);


        public Vector3 LevelEndFocalPoint
        {
            get; private set;
        }



        public void OnSegmentTraversed()
        {
            m_game.PlaySound(SoundFXType.Coin);
        }



        public void OnPickupCollected(Pickup pickup)
        {
            m_pickups.FreeObject(pickup.Container.gameObject);
        }



        public int      Index           { get; private set; }
        public Track    Track           { get; private set; }
        public Vector3  StartLinePos    { get { return Track.StartLinePos; } }
        public float    StartLineDist   { get { return Track.StartLineDist; } }



        public float GroundHeight
        {
            get { return m_ground.position.y; }
        }



        public float FinishLineDist
        {
            get { return TrackData.EndLineDist; }
        }



        public float TrackWidth
        {
            get { return Track.TrackData.BaseWidth; }
        }



        public Vector3 GetTrackEndPos(out Vector3 forward)
        {
            return Track.GetEndPos(out forward);
        } 



        public TrackBuilder.TrackSegment GetTrackSegmentAtIndex(int index)
        {
            return Track.TrackSegments[index % Track.TrackSegments.Length];
        }



        public TrackBuilder.TrackSegment GetTrackSegmentAtDist(float dist)
        {
            float posNormal;
            return GetTrackSegmentAtDist(dist, out posNormal);
        }



        public TrackBuilder.TrackSegment GetTrackSegmentAtDist(float dist, out float posNormal)
        {
            return TrackBuilder.GetTrackSegment(dist, Track, out posNormal);
        }



        public int AIPlayerCount
        {
            get { return m_levelDetails.AIPlayerCount; }
        }



        public void OnPerfectCurve()
        {
            m_game.OnPerfectCurve();
        }

        public void OnLevelStart()
        {            
            foreach(var pole in m_celebrationPoles)
            {
                pole.ShowConfetti(false);
            }
        }



        public void OnLevelComplete()
        {
            foreach (var pole in m_celebrationPoles)
            {
                pole.ShowConfetti(true);
            }
        }



        public TrackDetails TrackData
        {
            get { return Track != null ? Track.TrackData : null; }
        }



        public Vector2 GetTrackLateralLimits(float dist)
        {
            return Track.GetLateralLimits(dist);
        }



        public Vector2 GetTrackLateralLimits(float dist, out bool isExtended)
        {
            return Track.GetLateralLimits(dist, out isExtended);
        }



        public Vector3 GetTrackPos(float dist)
        {
            Vector3 lookDir;
            return Track.GetPosWithDir(dist, out lookDir);
        }



        public Vector3 GetTrackPosWithDir(float dist, out Vector3 lookDir)
        {
            return Track.GetPosWithDir(dist, out lookDir);
        }



        // Lateral range is -1 to 1
        public Vector3 GetTrackPosWithDirAndUp(float dist, out Vector3 lookDir, out Vector3 up)
        {
            return Track.GetPosWithDirAndUp(dist, out lookDir, out up);
        }



        public void SetActive(bool active)
        {
            if (m_active != active)
            {
                m_active = active;

                if (m_active)
                {
                    CreateProps();
                    CreateEnemies();
                    CreatePickups();
                }

                m_ground.gameObject.SetActive(active);
            }
        }



        public void Destroy()
        {
            Debug.Log("Destroying Level " + Index);
            Track.Destroy();

            m_props.Reset();
            m_enemies.Reset();
            m_pickups.Reset();
        }



        public void Setup(int level, Game game, Level prevLevel = null)
        {
            Index = level;

            m_ground = transform.FindIncludingInactive("Ground");
            m_ground.gameObject.SetActive(false);

            var pos = transform.position;
            pos.y = BaseHeight;
            transform.position = pos;

            m_game = game;
            m_cheatRampTimer = CheatRampInterval;

            CreateObjectPools();

            var levels = GameSettings.Instance().Levels;
            m_levelDetails = levels[level % levels.Length];

            Track = Instantiate(m_levelDetails.TrackPrefab).GetComponent<Track>();
            Track.name = m_levelDetails.TrackPrefab.name;
            Track.transform.parent = transform;

            var connectingTrack = prevLevel ? prevLevel.Track : null;


            Track.Setup(this);

            if (prevLevel)
            {
                Vector3 endForward;
                var endPos = prevLevel.GetTrackEndPos(out endForward);
                transform.position = endPos + endForward * 16 - StartLinePos;

                Debug.Log("Level placed at " + transform.position);
            }

            SetActive(false);
        }



        private void Update()
        {
            if(!m_active)
            {
                return;
            }


            var color = RenderSettings.ambientLight;
            color.r = Mathf.MoveTowards(color.r, AmbientLight.r, Time.deltaTime);
            color.g = Mathf.MoveTowards(color.g, AmbientLight.g, Time.deltaTime);
            color.b = Mathf.MoveTowards(color.b, AmbientLight.b, Time.deltaTime);

            RenderSettings.ambientLight = color;

            Track.AdvanceWaterOffset(0, Time.deltaTime * .1f);
            Track.AdvanceWaterOffset(1, Time.deltaTime * .05f);

            /* Cheat ramps out, could convert to teleporter though at some point so leaving code in
            if(m_game.Player.Position > 0)
            {
                m_cheatRampTimer -= Time.deltaTime;
                if(m_cheatRampTimer <= 0)
                {
                    if(Random.Range(0, 100) < CheatRampChance)
                    {
                        CreateProp(PropType.CheatRamp, new CheatRampPropInitialiser(Track, m_game.Player.TravelDist + 8));
                    }

                    m_cheatRampTimer += CheatRampInterval;
                }
            }
            */
        }



        private void CreateEnemies()
        {
            var enemyCount = Index / 3;

            for(int i=0;i < enemyCount && i < TrackData.Enemies.Count;++i)
            {
                var enemy = TrackData.Enemies[i];

                switch (enemy.Type)
                {
                    case EnemyType.DuckEnemy:
                        {
                            for (int k = 0; k < enemy.Count; ++k)
                            {
                                var go = m_enemies.AllocateObject();
                                if (go)
                                {
                                    var initialiser = new DuckEnemyInitialiser(Track, k, enemy.DistStart, enemy.ForwardSpeed,
                                                                                     enemy.LateralTime, enemy.ActivateInterval);

                                    go.GetComponent<Enemies>().Setup(enemy.Type, initialiser);
                                }
                            }
                            break;
                        }

                    default:
                        Debug.LogError("CreateEnemies did ont handle type " + enemy.Type);
                        return;
                }
            }
        }



        private void CreateProps()
        {
            foreach (var propDetails in TrackData.Props)
            {
                PropInitialiser initialiser = null;

                switch (propDetails.Type)
                {
                    case PropType.JumpRamp:
                    case PropType.StaticJumpRamp:
                    case PropType.HighJumpRamp:
                        {
                            var elevate = propDetails.Type != PropType.StaticJumpRamp;
                            initialiser = new JumpRampPropInitialiser(Track, propDetails.DistStart, elevate);
                            break;
                        }

                    case PropType.LevelJumpRamp:
                        {
                            initialiser = new LevelJumpRampPropInitialiser(Track, propDetails.DistStart);
                            break;
                        }

                    default:
                        Debug.LogError("CreateProp did ont handle type " + propDetails.Type);
                        break;
                }

                CreateProp(propDetails.Type, initialiser);
            }

            var toalLength = Track.TrackLength;

            int i = 1;
            var dist = 0f;
            var distStep = toalLength / TrackData.PoleCount;

            for (;i < TrackData.PoleCount;i+=2)
            {
                dist = distStep * i;

                CreateProp(PropType.Post, new PostPropInitialiser(Track, GroundHeight, dist, false));
                CreateProp(PropType.Post, new PostPropInitialiser(Track, GroundHeight, dist, true));
            }

            // Last 2 that will pop celebration confetti
            dist = toalLength - .0001f;

            m_celebrationPoles[0] = (PostProp)CreateProp(PropType.Post, new PostPropInitialiser(Track, GroundHeight, dist, false));
            m_celebrationPoles[1] = (PostProp)CreateProp(PropType.Post, new PostPropInitialiser(Track, GroundHeight, dist, true));

            LevelEndFocalPoint = Vector3.Lerp(m_celebrationPoles[0].transform.position, m_celebrationPoles[1].transform.position, .5f);
        }



        private Prop CreateProp(PropType type, PropInitialiser initialiser)
        {
            var go = m_props.AllocateObject();
            if (go)
            {
                return go.GetComponent<Props>().Setup(type, initialiser, m_props);
            }

            return null;
        }



        private void CreatePickups()
        {
            var dist = StartLineDist + CoinIntervalRange.y;

            while(dist < Track.TrackLength - CoinIntervalRange.y)
            {
                var count = Random.Range((int)CoinCountRange.x, (int)CoinCountRange.y + 1);
                for (int i = 0; i < count; ++i)
                {
                    var go = m_pickups.AllocateObject();
                    if (go)
                    {
                        var lateralTime = Random.Range(1f, 2f);
                        var initialiser = new CoinPickupInitialiser(Track, 0, dist, 0, lateralTime, 0);
                        go.GetComponent<Pickups>().Setup(PickupType.CoinPickup, initialiser);

                        dist += .25f;
                    }
                }

                dist += Mathf.Max(1, Random.Range(CoinIntervalRange.x, CoinIntervalRange.y));
            }
        }



        private void CreateObjectPools()
        {
            if (m_props == null)
            {
                var poolParent = new GameObject("Props").transform;
                poolParent.parent = transform;
                m_props = new ObjectPool("Props", GameSettings.Instance().PropsPrefab, poolParent, GameSettings.Instance().MaxProps);
            }
            else
            {
                m_props.Reset();
            }

            if (m_enemies == null)
            {
                var poolParent = new GameObject("Enemies").transform;
                poolParent.parent = transform;
                m_enemies = new ObjectPool("Enemies", GameSettings.Instance().EnemiesPrefab, poolParent, GameSettings.Instance().MaxEnemies);
            }
            else
            {
                m_enemies.Reset();
            }

            if (m_pickups == null)
            {
                var poolParent = new GameObject("Pickups").transform;
                poolParent.parent = transform;
                m_pickups = new ObjectPool("Pickups", GameSettings.Instance().PickupsPrefab, poolParent, GameSettings.Instance().MaxPickups);
            }
            else
            {
                m_pickups.Reset();
            }
        }



        private Game                        m_game;
        private bool                        m_active;

        private GameSettings.LevelDetails   m_levelDetails;
        private float                       m_cheatRampTimer;

        private Transform                   m_ground;
        private ObjectPool                  m_props, m_enemies, m_pickups;
        private readonly PostProp[]         m_celebrationPoles = new PostProp[2];
    }
}