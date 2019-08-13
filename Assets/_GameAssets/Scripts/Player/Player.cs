using UnityEngine;



namespace SpeedyBoat
{
    public abstract class Player : MonoBehaviour
    {
        public float TravelDist;
        public int Position;

        public float SpeedScale = 1;
        public float YawMax = 12;
        public float YawThreshold = 1;
        public float YawSpeed = 2;

        public float WakeInterval = .25f;
        public float WakeLength = .5f;
        public float WakeWidthStart = .1f;
        public float WakeHeight = .01f;

        public float TargetYaw, RotationDeltaY;


        public float BouyancyLevel = .05f;

        public float JumpHeight = 3;
        public float JumpTime = 2;
        public int JumpBounces = 1;

        public float DropHeight = 4;
        public float DropSpeed = 4;

        public float LateralSpeedScale = .075f;
        public float ExtendedLateralSpeedScale = .01f;
        public float LateralReturnSpeedScale = .02f;

        public float Velocity, LateralVelocity;



        public Game Game { get; private set; }



        public abstract void OnBoatCollision();



        public virtual void OnLevelJumpComplete()
        {
        }



        public void OnFlipped(int flipCount)
        {
            if (!Game.Main.PlayerData.ShownHoldFlip)
            {
                Game.ShowHoldFlip();
            }
            Game.ApplyFlipBonus(transform.position, flipCount);

            //Updated by Syed Mohib...
            //if (GetComponent<HumanPlayer>().isFirst)
            //  Game.ApplyFlipBonus(transform.position, flipCount);

            //else
            //Game.ApplyFlipBonus(transform.position, 0);
            //Till here...
        }



        public void OnDeath(bool fell = false)
        {
            ShowSpray(false);
            ShowBubbles(false);

            ChangeAction(PlayerActionType.None);

            if (fell)
            {
                ShowSmokePuff(1, false);
                m_boat.Explode();
            }

            Game.OnPlayerDeath(this);
        }



        public void ShowCrown(bool show)
        {
            m_boat.ShowCrown(show);
        }



        public void ShowTrail(bool show)
        {
            m_boat.ShowTrail(show);
        }



        public float JumpProgress
        {
            get { return (m_action is JumpPlayerAction) ? (m_action as JumpPlayerAction).JumpProgress : 0; }
        }



        public Color PrimaryColor
        {
            get { return m_boat.Color1; }
        }



        public Vector3 TrackPos
        {
            get; protected set;
        }



        public Vector3 WorldVelocity
        {
            get; private set;
        }



        public void ShowSpray(bool show)
        {
            if (m_spray != null)
            {
                m_spray.gameObject.SetActive(true);

                var emission = m_spray.GetComponent<ParticleSystem>().emission;
                emission.enabled = show;
            }
        }



        public void ShowBubbles(bool show)
        {
            if (m_bubbles != null)
            {
                m_bubbles.gameObject.SetActive(true);

                var emission = m_bubbles.GetComponent<ParticleSystem>().emission;
                emission.enabled = show;
            }
        }



        public float LateralWidth
        {
            get
            {
                var trackWidth = Game.Level.TrackWidth;
                var playerWidth = GetComponentInChildren<Collider>().bounds.size.z;

                return playerWidth / trackWidth * 2;
            }
        }



        public virtual float FlipTime
        {
            get { return 0; }
        }



        public Quaternion BoatRotation
        {
            get { return m_boat.transform.localRotation; }
            set { m_boat.transform.localRotation = value; }
        }



        public void ShowSmokePuff(float scale, bool showStars)
        {
            Game.ShowSmokePuff(transform.position, scale, showStars);
        }



        public virtual void UpgradeBoat(GameObject boatModel)
        {

        }



        public virtual void Boost(BoostType type, float scale = 1)
        {

        }



        public bool IsFalling
        {
            get { return m_action is FallPlayerAction; }
        }



        public bool IsDead
        {
            get { return m_action == null; }
        }



        public bool IsOnRamp
        {
            get { return m_activeRamp != null; }
        }



        public bool IsJumping
        {
            get { return m_action is JumpPlayerAction; }
        }



        public float SpeedNormal
        {
            get { return Mathf.Clamp01(Velocity / m_boat.MaxSpeed); }
        }



        public Vector3 PositionTextPos
        {            
            get { return m_positionText.transform.position; }
        }



        public float Height
        {
            get; set;
        }



        public virtual bool ShowPosition
        {
            get { return !(m_action is SinkPlayerAction) && !(m_action is LevelJumpPlayerAction); }
        }



        public virtual Vector3 CameraTrackingPos
        {
            get { return transform.position; }
        }



        public virtual Vector3 CameraTrackingForward
        {
            get { return m_trackDir; }
        }



        public void SetBoat(GameObject boatPrefab)
        {
            SetBoat(boatPrefab, Color.white);
        }



        public void SetBoat(GameObject boatPrefab, Color color)
        {
            m_boat = Instantiate(boatPrefab).GetComponent<Boat>();
            m_boat.gameObject.SetActive(true);

            while (m_character.childCount > 0)
            {
                var child = m_character.GetChild(0);
                child.parent = null;
                Destroy(child.gameObject);
            }

            m_boat.transform.SetParent(m_character, false);
            m_boat.transform.localPosition = Vector3.zero;
            m_boat.name = boatPrefab.name;

            m_boat.Setup(color, Color.white);
        }



        public virtual void Setup(Game game, GameObject boatPrefab, Color color, float startDist, float startLateral)
        {
            gameObject.SetActive(true);

            Game = game;

            m_bubbles = transform.FindIncludingInactive("Bubbles");
            if (m_bubbles)
            {
                m_bubbles.gameObject.SetActive(false);
            }

            m_character = transform.FindIncludingInactive("Character");
            m_positionText = transform.FindIncludingInactive("Position");

            SetBoat(boatPrefab, color);
            TravelDist = m_lastWakeDist = startDist;

            m_targetLateral = 0;
            m_lateralPos = startLateral;

            SpeedScale = 1;
            Velocity = LateralVelocity = 0;

            ShowSpray(true);
        }



        public virtual void UpdateMovement(bool allowInput, bool decellerate = true)
        {
        }



        public virtual void UpdateLateralPos()
        {
        }


        protected virtual void Update()
        {
            SpeedScale = Mathf.MoveTowards(SpeedScale, 1, Time.deltaTime);

            var oldPos = transform.position;

            var oldDist = TravelDist;
            var oldRotation = transform.localEulerAngles;

            if (!IsFalling && !IsDead)
            {
                SetTrackPosition();
                UpdateRampPositioning();
            }

            if (m_action != null)
            {
                m_action.Update();
            }

            UpdateWake();

            var rotation = transform.localEulerAngles;
            RotationDeltaY = Mathf.DeltaAngle(rotation.y, oldRotation.y);
            if (Mathf.Abs(RotationDeltaY) < YawThreshold)
            {
                RotationDeltaY = 0;
            }

            var rotationSign = RotationDeltaY == 0 ? 0 : Mathf.Sign(RotationDeltaY);

            TargetYaw = Mathf.MoveTowards(TargetYaw, rotationSign, YawSpeed * Time.deltaTime);
            transform.localRotation *= Quaternion.Euler(0, 0, TargetYaw * YawMax);

            WorldVelocity = transform.position - oldPos;
        }



        public void ChangeAction(PlayerActionType type, PlayerActionInitialiser initialiser = null)
        {
            var wasJumping = false;

            if (m_action != null)
            {
                wasJumping = m_action is JumpPlayerAction;

                m_action.Destroy();
                m_action = null;
            }

            switch (type)
            {
                case PlayerActionType.Enter:
                    m_action = new DrivePlayerAction(this, initialiser);
                    break;

                case PlayerActionType.Drive:
                    m_action = new DrivePlayerAction(this, initialiser);
                    break;

                case PlayerActionType.FlipJump:
                    m_action = new FlipJumpPlayerAction(this, initialiser);
                    break;

                case PlayerActionType.Upgrade:
                    m_action = new UpgradePlayerAction(this, initialiser);
                    break;

                case PlayerActionType.Sink:
                    m_action = new SinkPlayerAction(this, initialiser);
                    break;

                case PlayerActionType.LevelJump:
                    m_action = new LevelJumpPlayerAction(this, initialiser);
                    break;

                case PlayerActionType.CheatJump:
                    m_action = new CheatJumpPlayerAction(this, initialiser);
                    break;

                case PlayerActionType.Fall:
                    m_action = new FallPlayerAction(this, initialiser);
                    break;
            }

            if (wasJumping)
            {
                ShowTrail(true);
            }
            else if (m_action is JumpPlayerAction)
            {
                ShowTrail(false);
            }
        }



        protected virtual bool ShowWake
        {
            get { return true; }
        }



        protected virtual float GetLateralAbs(float dist)
        {
            var lateralLimits = Game.Level.GetTrackLateralLimits(dist);
            return Mathf.Lerp(lateralLimits.x, lateralLimits.y, (m_lateralPos + 1) * .5f);
        }



        protected virtual void SetTrackPosition()
        {
            TrackPos = Game.Level.GetTrackPosWithDirAndUp(TravelDist, out m_trackDir, out m_up);

            var pos = TrackPos;

            if (IsOnRamp)
            {
                pos.y = transform.position.y;
            }

            transform.position = pos;

            // Splt the look between forward and the tracks center to get a more exacerbated rotation on the boat

            var rotation = Quaternion.LookRotation(m_trackDir).eulerAngles;

            if (IsJumping)
            {
                rotation.x = transform.localEulerAngles.x;
            }

            transform.localEulerAngles = rotation;

            var right = Vector3.Cross(m_trackDir, m_up);
            transform.position += right * -m_lateralPos * Game.Level.TrackData.WaterWidth * .5f;

            if (!IsJumping && !IsOnRamp)
            {
                transform.position += m_up * Height;
            }
        }



        protected virtual void UpdateRampPositioning()
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

        protected float m_rampAngle;
        protected Vector3 m_frontHit, m_backHit, m_lastFrontOffset;


        protected void OnBoatRearEnterRamp(RampProp prop)
        {
            Debug.Log(transform.name + " OnBoatRearEnterRamp " + prop.name);
        }



        protected void OnBoatFrontEnterRamp(RampProp prop)
        {
            Debug.Log(transform.name + " OnBoatFrontEnterRamp " + prop.name);
        }



        protected void OnBoatFrontExitRamp(RampProp prop)
        {
            Debug.Log(transform.name + " OnBoatFrontExitRamp " + prop.name);
        }



        protected void OnBoatRearExitRamp(RampProp ramp)
        {
            Debug.Log(transform.name + " OnBoatRearExitRamp " + ramp.name);

            if (ramp is JumpRampProp)
            {
                var rampAngle = ramp.BaseLaunchAngle;

                Boost(BoostType.Ramp, ramp.BoostScale);

                var jumpScale = SpeedNormal * Mathf.Clamp01(Mathf.InverseLerp(ramp.SpeedBoostRange.x, ramp.SpeedBoostRange.y, ramp.BoostScale));
                if (jumpScale > 0)
                {
                    ChangeAction(PlayerActionType.FlipJump, new FlipJumpPlayerActionInitialiser(rampAngle, ramp.JumpHeight, ramp.JumpTime, jumpScale, JumpBounces, FlipTime));
                }
            }
            else if (ramp is LevelJumpRampProp)
            {
                var launchAngle = ((RampProp)ramp).BaseLaunchAngle;
                ChangeAction(PlayerActionType.LevelJump, new LevelJumpPlayerActionInitialiser(Game.NextLevel.StartLinePos, launchAngle, JumpHeight, JumpTime, JumpBounces));
            }
            else if (ramp is CheatRampProp)
            {
                var launchAngle = ((RampProp)ramp).BaseLaunchAngle;
                ChangeAction(PlayerActionType.CheatJump, new CheatJumpPlayerActionInitialiser(Game.NextLevel.StartLinePos, launchAngle, JumpHeight, JumpTime, JumpBounces));
            }
        }



        protected virtual void UpdateWake()
        {
        }

        public virtual void OnHitWithPlayer()
        {

        }


        private void OnCollisionEnter(Collision collision)
        {
            if (gameObject.activeInHierarchy && collision.gameObject.activeInHierarchy)
            {
                var player = collision.gameObject.GetComponent<Player>();
                if (player)
                {
                    // Extended so a new collision is created when re-entering
                    var extendedLateralWidth = LateralWidth * 1.05f;

                    var LateralLimits = new Vector2(.9f, .9f);

                    var myLeftLateral = m_lateralPos - extendedLateralWidth * .5f;
                    var myRightLateral = m_lateralPos + extendedLateralWidth * .5f;

                    var theirLeftLateral = player.m_lateralPos - extendedLateralWidth * .5f;
                    var theirRightLateral = player.m_lateralPos + extendedLateralWidth * .5f;

                    // We are on their right
                    if (m_lateralPos > player.m_lateralPos)
                    {
                        if (theirRightLateral > myLeftLateral)
                        {
                            var newLateral = myLeftLateral - extendedLateralWidth * .5f;
                            player.m_lateralPos = Mathf.Clamp(newLateral, LateralLimits.x, LateralLimits.y);
                            player.m_targetLateral = player.m_lateralPos;

                         
                        }
                    }
                    // We are on their left
                    else
                    {
                        //TODO: Make hit the player out of track...
                        if (collision.transform.gameObject.layer == LayerMask.NameToLayer("Player"))
                        {
                            //transform.position = Vector2.MoveTowards(collision.transform.position, transform.position, -.2f * Time.deltaTime);

                            this.transform.position -= collision.transform.position * Time.deltaTime * -.5f;                            //this.transform.position = Vector3.right * 10;
                            Debug.Log("Collided with AI");
                            //if (this.transform.position.x > this.transform.position.x + 5)
                                //OnDeath(true);
                                //this.transform.position = new Vector3(this.transform.position.x + 200 * Time.deltaTime, this.transform.position.y, this.transform.position.z);
                            OnHitWithPlayer();
                           
                        }

                        if (theirLeftLateral > myRightLateral)
                        {
                            var newLateral = myRightLateral + extendedLateralWidth * .5f;
                            player.m_lateralPos = Mathf.Clamp(newLateral, LateralLimits.x, LateralLimits.y);
                            player.m_targetLateral = player.m_lateralPos;
                        }
                    }

                    //TODO: Make hit the player out of track...
                    if (collision.transform.gameObject.layer == LayerMask.NameToLayer("Player"))
                    {
                        //transform.position = Vector2.MoveTowards(collision.transform.position, transform.position, -.2f * Time.deltaTime);

                        this.transform.position -= collision.transform.position * Time.deltaTime * -.5f;
                        // this.BoatRotation = Quaternion.FromToRotation(this.transform.position, this.transform.position * 10 * Time.deltaTime);

                        Debug.Log("Collided with Player");
                        //if (this.transform.position.x > this.transform.position.x + 5)
                            //OnDeath(true);
                        OnHitWithPlayer();
                        //ShowSmokePuff(2, false);
                        //this.gameObject.transform.position *= 2;
                    }


                    // Check to see if we need to move if they were up against the edge
                    theirLeftLateral = player.m_lateralPos - extendedLateralWidth * .5f;
                    theirRightLateral = player.m_lateralPos + extendedLateralWidth * .5f;

                    // We are on their right
                    if (m_lateralPos > player.m_lateralPos)
                    {
                        if (theirRightLateral > myLeftLateral)
                        {
                            var newLateral = theirRightLateral + extendedLateralWidth * .5f;
                            m_lateralPos = Mathf.Clamp(theirRightLateral + extendedLateralWidth * .5f, LateralLimits.x, LateralLimits.y);
                            m_targetLateral = m_lateralPos;
                        }
                    }
                    // We are on their left
                    else
                    {
                        if (theirLeftLateral < myRightLateral)
                        {
                            var newLateral = theirLeftLateral - extendedLateralWidth * .5f;
                            m_lateralPos = Mathf.Clamp(newLateral, LateralLimits.x, LateralLimits.y);
                            m_targetLateral = m_lateralPos;
                        }
                    }

                    LateralVelocity = player.LateralVelocity = 0;


                    if (TravelDist < player.TravelDist)
                    {
                        OnBoatCollision();
                    }
                    else
                    {
                        player.OnBoatCollision();
                    }
                }
            }
        }



        protected Boat          m_boat;

        protected PlayerAction  m_action;

        protected RampProp      m_activeRamp;

        protected RampProp      m_frontRamp, m_rearRamp;

        protected float         m_lateralAbs, m_boostTime, m_boostScale;

        protected Vector3       m_trackDir, m_up;

        protected float         m_lastWakeDist;

        protected float         m_lateralPos, m_targetLateral, m_lateralChangeTime;
        protected Transform     m_spray, m_bubbles, m_character, m_positionText;
    }
}