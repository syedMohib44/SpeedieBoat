using UnityEngine;



namespace SpeedyBoat
{
    public class HumanPlayer : Player
    {
        public bool isFirst { get; private set; }
        public bool Upgraded
        {
            get { return m_boat.Enlarged; }
            set
            {
                if (m_boat.Enlarged != value)
                {
                    m_boat.Enlarged = value;
                    if (value)
                    {
                        Game.PlaySound(SoundFXType.Upgrade);
                    }
                }
            }
        }



        public override void OnLevelJumpComplete()
        {
            ChangeAction(PlayerActionType.None);
        }



        public override void OnBoatCollision()
        {
            if (m_action is DrivePlayerAction)
            {
                SpeedScale = .5f;
            }
        }



        public override float FlipTime
        {
            get { return .5f; }
        }



        public override bool ShowPosition
        {
            get { return base.ShowPosition && !Game.IsShowingBonusEffect; }
        }



        public override void Boost(BoostType type, float scale = 1)
        {
            switch (type)
            {
                case BoostType.Upgrade:
                    var playerSetup = GameSettings.Instance().Player;
                    m_boostTime = playerSetup.BoostTime;
                    m_boostScale = playerSetup.BoostSpeedScale * scale;
                    break;

                case BoostType.Ramp:
                    m_boostTime = 1;
                    m_boostScale = scale;
                    break;

                default:
                    Debug.LogError("Boost did not handle type " + type);
                    return;
            }
        }



        public override void UpgradeBoat(GameObject boatPrefab)
        {
            SetBoat(boatPrefab, Color.red);
            Upgraded = true;

            ShowSmokePuff(2, true);
        }



        public override Vector3 CameraTrackingPos
        {
            get { return TrackPos; }
        }



        public override void Setup(Game game, GameObject boatPrefab, Color color, float startDist, float startLateral)
        {
            base.Setup(game, boatPrefab, color, startDist, startLateral);

            m_audioSource = GetComponent<AudioSource>();
            m_audioSource.volume = 0;

            if (Game.Main.PlayerData.SoundOn)
            {
                if (!m_audioSource.isPlaying)
                {
                    m_audioSource.loop = true;
                    m_audioSource.Play();
                }
            }

            m_lateralPos = m_lateralAbs = 0;
            TravelDist = game.Level.Track.TrackData.StartLineDist;

            m_boat.gameObject.layer = LayerMask.NameToLayer("Player");

            Upgraded = false;

            ChangeAction(PlayerActionType.Enter, new EnterPlayerActionInitialiser(DropSpeed, DropHeight));

            Update();
        }



        private void IncreaseMotorSound()
        {
            if (Game.Main.PlayerData.SoundOn)
            {
                m_audioSource.volume = Mathf.MoveTowards(m_audioSource.volume, 4, Time.deltaTime * 2);
            }
        }



        private void DecreaseMotorSound()
        {
            m_audioSource.volume = Mathf.MoveTowards(m_audioSource.volume, 0, Time.deltaTime * 2);
        }


        protected override void Update()
        {
            base.Update();

            if(Game.Mode != Game.GameMode.LevelComplete && TravelDist > Game.Level.FinishLineDist)
            {
                Game.OnLevelComplete();
            }

            if (this.Position == 0)
            {
                isFirst = true;
            }
            else
                isFirst = false;


#if UNITY_EDITOR
            if(Input.GetKey(KeyCode.E))
            {
                m_boat.Explode();
            }
#endif
        }



        protected override void UpdateWake()
        {
            return;

            // Far too expensive
            /*
            if (ShowWake && Height <= 0)
            {
                var wakeCount = Mathf.FloorToInt((TravelDist - m_lastWakeDist) / WakeInterval);
                for (int i = 0; i < wakeCount; ++i)
                {
                    m_lastWakeDist += WakeInterval;

                    // Each Wake segment destroys itself once complete
                    var wakeDist = m_lastWakeDist - .25f;
                    var lateralAbs = GetLateralAbs(wakeDist);

                    m_game.Level.Track.CreateDecal(TrackDecalType.Wake, wakeDist, lateralAbs, WakeWidthStart, WakeHeight, WakeInterval);
                }
            }
            */
        }



        protected override bool ShowWake
        {
            get { return !IsJumping; }
        }



        protected override float GetLateralAbs(float dist)
        {
            return m_lateralAbs;
        }



        public Vector3 LateralNormal
        {
            get; private set;
        }



        protected override void SetTrackPosition()
        {
            TrackPos = Game.Level.GetTrackPosWithDirAndUp(TravelDist, out m_trackDir, out m_up);

            var pos = TrackPos;
            if (IsOnRamp)
            {
                pos.y = transform.position.y;
            }
            else if (!IsJumping)
            {
                pos += m_up * Height;
            }

            transform.position = pos;

            var rotation = Quaternion.LookRotation(m_trackDir).eulerAngles;

            if (IsJumping || IsOnRamp)
            {
                rotation.x = transform.localEulerAngles.x;
            }

            transform.localEulerAngles = rotation;

            LateralNormal = -Vector3.Cross(m_trackDir, m_up);
            transform.position += LateralNormal * m_lateralAbs;

        }



        public override void UpdateMovement(bool allowInput, bool decellerate = true)
        {
            if (Game.Mode == Game.GameMode.LevelStart)
            {
                return;
            }

            if (m_boostTime > 0)
            {
                m_boostTime = Mathf.Max(0, m_boostTime - Time.deltaTime);
            }
            else
            {
                SpeedScale = Mathf.MoveTowards(SpeedScale, m_boostTime > 0 ? GameSettings.Instance().Player.BoostSpeedScale : 1, Time.deltaTime);
            }

            if (allowInput && Game.Mode != Game.GameMode.Demo && Input.GetMouseButton(0))
            {
                Velocity = Mathf.Min(m_boat.MaxSpeed, Velocity + m_boat.Acceleration * Time.deltaTime);
                
                //Updated by Syed Mohib
                if (Velocity > 6)
                    Velocity = 6;
                //Till here..

                IncreaseMotorSound();
            }
            else
            {
                DecreaseMotorSound();

                if (decellerate)
                {
                    Velocity = Mathf.Max(0, Velocity - m_boat.Decceleration * Time.deltaTime);
                }
            }

            TravelDist += Velocity * SpeedScale * Time.deltaTime;
        }



        public override void UpdateLateralPos()
        {
            bool isExtended;
            var lateralLmits = Game.Level.GetTrackLateralLimits(TravelDist, out isExtended);

            // Prevent moving to the absolute edge
            lateralLmits *= .9f;

#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.LeftBracket))
            {
                m_lateralAbs = Mathf.MoveTowards(m_lateralAbs, lateralLmits.x, Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.RightBracket))
            {
                m_lateralAbs = Mathf.MoveTowards(m_lateralAbs, lateralLmits.y, Time.deltaTime);
            }
#endif

            var lateralMovement = Velocity * RotationDeltaY * Time.deltaTime;
            m_lateralAbs += lateralMovement * (isExtended ? ExtendedLateralSpeedScale : LateralSpeedScale);

            m_lateralAbs = Mathf.MoveTowards(m_lateralAbs, 0, Velocity * LateralReturnSpeedScale * Time.deltaTime);

            var noFalling = Game.Level.Index <= 1;
            var offTrack = !noFalling && !isExtended && (SpeedNormal >= 1 && (m_lateralAbs < lateralLmits.x || m_lateralAbs > lateralLmits.y));

            if(offTrack)
            {
                if(m_offTrackTimer <= 0)
                {
                    m_offTrackTimer = OffTrackTime;
                }
                else
                {
                    m_offTrackTimer -= Time.deltaTime;
                    if(m_offTrackTimer <= 0)
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
        }

        private const float OffTrackTime = .33f;
        private float m_offTrackTimer;

        private void OnCollisionEnter(Collision collision)
        {
            if(gameObject.activeInHierarchy && collision.gameObject.activeInHierarchy)
            {
                //Updated by Syed Mohib...
                if(collision.gameObject.layer == LayerMask.NameToLayer("Pickup"))
                {
                    OnPickupCollision(collision.gameObject.GetComponent<Pickup>());
                }
                else if(collision.gameObject.layer == LayerMask.NameToLayer("Prop"))
                {
                    OnPropCollision(collision.gameObject.GetComponent<Prop>());
                }
                else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    var enemy = collision.gameObject.GetComponent<Enemy>();
                    if (enemy)
                    {
                        OnEnemyCollision(enemy);
                    }
                }
            }
        }



        private void OnPropCollision(Prop prop)
        {
            switch(prop.Type)
            {
                case PropType.JumpRamp:
                case PropType.StaticJumpRamp:
                case PropType.HighJumpRamp:
                case PropType.LevelJumpRamp:
                case PropType.Post:
                    break;

                default:
                    Debug.LogError("OnPropCollision did not handle type " + prop.Type);
                    return;
            }
        }



        private void OnEnemyCollision(Enemy enemy)
        {
            ChangeAction(PlayerActionType.Sink, new SinkPlayerActionInitialiser(2));
        }



        private void OnPickupCollision(Pickup pickup)
        {
            Game.OnPickupCollision(pickup);
        }



        protected override void UpdateRampPositioning()
        {
            if (IsJumping || Height > 0)
            {
                m_frontRamp = m_rearRamp = null;
                return;
            }

            var rampLayer = 1 << LayerMask.NameToLayer("Prop");
            var waterLayer = 1 << LayerMask.NameToLayer("Water");
            var layerMask = rampLayer | waterLayer;

            var frontPos = m_boat.FrontLift.transform.position;
            var ray = new Ray(frontPos + Vector3.up, Vector3.down);

            var m_frontHit = m_boat.FrontLift.transform.position;
            var m_backHit = m_boat.BackLift.transform.position;

            // Front
            RaycastHit hit;
            var frontHitWater = false;
            var frontHitRamp = Physics.Raycast(ray, out hit, 2, rampLayer);

            if (frontHitRamp)
            {
                var oldRamp = m_frontRamp;
                m_frontRamp = hit.transform.GetComponentInParents<RampProp>();

                if (oldRamp == null)
                {
                    OnBoatFrontEnterRamp(m_frontRamp);
                }
            }
            else
            {
                if (m_frontRamp)
                {
                    OnBoatFrontExitRamp(m_frontRamp);
                    m_frontRamp = null;
                }

                frontHitWater = Physics.Raycast(ray, out hit, 2, waterLayer);
            }

            if (frontHitRamp || frontHitWater)
            {
                m_frontHit = m_boat.FrontLift.transform.position;
                m_frontHit.y = hit.point.y;
            }

            // Rear
            var backPos = m_boat.BackLift.transform.position;
            ray = new Ray(backPos + Vector3.up, Vector3.down);

            var rearHitWater = false;
            var rearHitRamp = Physics.Raycast(ray, out hit, 2, rampLayer);

            if (rearHitRamp)
            {
                if (frontHitRamp)
                {
                    m_lastFrontOffset = m_frontHit - m_backHit;
                }

                var oldRamp = m_rearRamp;
                m_rearRamp = hit.transform.GetComponentInParents<RampProp>();

                if (oldRamp == null)
                {
                    OnBoatRearEnterRamp(m_rearRamp);
                }
            }
            else
            {
                if (m_rearRamp)
                {
                    OnBoatRearExitRamp(m_rearRamp);
                    m_rearRamp = null;
                    return;
                }

                rearHitWater = Physics.Raycast(ray, out hit, 2, waterLayer);
            }

            if (rearHitRamp || rearHitWater)
            {
                m_backHit = m_boat.BackLift.transform.position;
                m_backHit.y = hit.point.y;
            }

            m_activeRamp = m_frontRamp ?? m_rearRamp;
            if (m_activeRamp)
            {
                if (m_frontRamp == null)
                {
                    m_frontHit = m_backHit + m_lastFrontOffset;
                }

                var opposite = (m_frontHit - m_backHit).magnitude;
                var adjacent = m_backHit.y - m_frontHit.y;
                m_rampAngle = Mathf.Rad2Deg * Mathf.Atan2(adjacent, opposite);

                var angles = transform.localEulerAngles;
                angles.x = m_rampAngle;
                transform.localEulerAngles = angles;

                var playerPos = transform.position;
                playerPos.y = m_backHit.y + (m_frontHit.y - m_backHit.y) * .5f;
                transform.position = playerPos;
            }
            else
            {
                var opposite = (m_frontHit - m_backHit).magnitude;
                var adjacent = m_backHit.y - m_frontHit.y;
                m_rampAngle = Mathf.Rad2Deg * Mathf.Atan2(adjacent, opposite);

                var angles = transform.localEulerAngles;
                angles.x = m_rampAngle;
                transform.localEulerAngles = angles;

                var playerPos = transform.position;
                playerPos.y = m_backHit.y + (m_frontHit.y - m_backHit.y) * .5f;
                transform.position = playerPos;
            }
        }



        private AudioSource m_audioSource;
    }
}