using UnityEngine;



namespace SpeedyBoat
{
    public class AIPlayer : Player
    {
        public float AISpeedScale = .7f; //.9f
        public float LateralChangeTime = 1f;
        public bool isDestroed { get; private set; }


        public override void OnBoatCollision()
        {
            if (m_action is DrivePlayerAction)
            {
                SpeedScale = .5f;
            }
        }


        public override void Setup(Game game, GameObject modelPrefab, Color color, float startDist, float startLateral)
        {
            base.Setup(game, modelPrefab, color, startDist, startLateral);

            m_lateralChangeTime = LateralChangeTime;

            ChangeAction(PlayerActionType.Enter, new EnterPlayerActionInitialiser(DropSpeed, DropHeight));

            Update();
        }



        public override void UpdateMovement(bool allowInput, bool decellerate = true)
        {
            if (Game.Mode == Game.GameMode.LevelStart)
            {
                return;
            }

            var trackWidth = Game.Level.TrackWidth;
            var playerWidth = m_boat.Model.lossyScale.z * m_boat.Model.GetComponent<MeshFilter>().sharedMesh.bounds.size.z;

            var playerLateralWidth = playerWidth / trackWidth * 2;

            var LateralLimits = new Vector2(-.9f, .9f); //-.9, .9

            var baseVelocity = m_boat.MaxSpeed * AISpeedScale;
            if (isDestroed == false)
            {

                Velocity = Mathf.MoveTowards(Velocity, m_boat.MaxSpeed * AISpeedScale, Time.deltaTime);

                //Updated by Syed Mohib
                if (Velocity > 6)
                    Velocity = 6;
                //Till here..

                LateralVelocity = Mathf.MoveTowards(LateralVelocity, Velocity, Time.deltaTime);

                m_lateralChangeTime -= Time.deltaTime;
                if (m_lateralChangeTime <= 0)
                {
                    m_targetLateral = Random.Range(LateralLimits.x, LateralLimits.y);
                    m_lateralChangeTime = LateralChangeTime;
                }

                TravelDist += Velocity * SpeedScale * Time.deltaTime;
                m_lateralPos = Mathf.MoveTowards(m_lateralPos, m_targetLateral, (LateralVelocity / baseVelocity) * SpeedScale * Time.deltaTime);

                m_lateralPos = Mathf.Clamp(m_lateralPos, LateralLimits.x, LateralLimits.y);
            }

            else
            {
                if (Game.Mode == Game.GameMode.GamePlay)
                {
                    //OnDeath(true);
                }
                isDestroed = false;
                return;
            }
        }
        protected override void Update()
        {
            base.Update();
        }
        private const float OffTrackTime = .33f;
        private float m_offTrackTimer;

        public Vector3 LateralNormal
        {
            get; private set;
        }
        public override void OnHitWithPlayer()
        {
            bool isExtended;
            var lateralLmits = Game.Level.GetTrackLateralLimits(TravelDist, out isExtended);

            // Prevent moving to the absolute edge
            lateralLmits *= .9f;
            m_lateralAbs = Mathf.MoveTowards(m_lateralAbs, lateralLmits.x, Time.deltaTime);

            m_lateralAbs = Mathf.MoveTowards(m_lateralAbs, lateralLmits.y, Time.deltaTime);

            var lateralMovement = Velocity * RotationDeltaY * Time.deltaTime;
            m_lateralAbs += lateralMovement * (isExtended ? ExtendedLateralSpeedScale : LateralSpeedScale);

            m_lateralAbs = Mathf.MoveTowards(m_lateralAbs, 0, Velocity * LateralReturnSpeedScale * Time.deltaTime);

            var noFalling = Game.Level.Index <= 1;
            var offTrack = !noFalling && !isExtended && (SpeedNormal >= 1 && (m_lateralAbs < lateralLmits.x || m_lateralAbs > lateralLmits.y));

            if (offTrack)
            {
                if (m_offTrackTimer <= 0)
                {
                    m_offTrackTimer = OffTrackTime;
                }
                else
                {
                    m_offTrackTimer -= Time.deltaTime;
                    if (m_offTrackTimer <= 0)
                    {
                        ChangeAction(PlayerActionType.Fall, new FallPlayerActionInitialiser(LateralNormal, Game.Level.GroundHeight));
                    }
                }
            }
            else
            {
                m_offTrackTimer = 0;
            }

            m_lateralAbs = Mathf.Clamp(m_lateralAbs, lateralLmits.x, lateralLmits.y);

            LateralNormal = -Vector3.Cross(m_trackDir, m_up);

            //this.AISpeedScale = 0;
            //this.transform.position = Vector3.left * 5;
            if (this.transform.position.x > this.transform.position.x + 5)
                //OnDeath(true);
            ChangeAction(PlayerActionType.Fall, new FallPlayerActionInitialiser(LateralNormal, Game.Level.GroundHeight));


            isDestroed = true;
        }
    }
}