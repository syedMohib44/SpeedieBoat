#if false
using UnityEngine;



namespace SpeedyBoat
{
    public class HumanPlayer_Physics : Player
    {
        public float TurnSpeed      = 720;
        public float FlipTime       = .33f;

        public float JumpHeight     = 3;
        public float JumpTime       = 2;
        public float JumpBounces    = 1;

        public float DropHeight     = 4;
        public float DropSpeed      = 4;

        public float LateralSpeedScale          = .075f;
        public float ExtendedLateralSpeedScale  = .1f;
        public float LateralReturnSpeedScale    = .2f;


        private Vector3 TargetPosition;
        private Quaternion TargetRotation;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, TargetPosition);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + TargetRotation * Vector3.forward * 10);
        }



        public override void OnBoatCollision()
        {
            if (m_state == State.Driving)
            {
                SpeedScale = 0;
            }
        }



        public override bool ShowPosition
        {
            get { return m_state != State.Exploding; }
        }



        public void Boost(BoostType type, float scale = 1)
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
            SetBoat(boatPrefab);
            ChangeState(State.Upgrading);
        }



        public override Vector3 CameraTrackingPos
        {
            get { return transform.position; }
        }



        public override void Setup(Game game, GameObject boatPrefab, float startDist, float startLateral)
        {
            base.Setup(game, boatPrefab, startDist, startLateral);

            m_boat.gameObject.layer = LayerMask.NameToLayer("Player");

            Vector3 trackDir;
            var startPos = m_game.Level.Track.GetPosWithDir(startDist, out trackDir);
            transform.position = startPos;// + Vector3.up;
            transform.localRotation = Quaternion.LookRotation(trackDir);

            m_rigidBody = GetComponent<Rigidbody>();
            m_rigidBody.ResetCenterOfMass();
            m_rigidBody.ResetInertiaTensor();
            m_rigidBody.velocity = m_rigidBody.angularVelocity = Vector3.zero;

            ChangeState(State.Entering);

            Update();
        }



        private float MaxSpeed
        {
            get { return m_boat.MaxSpeed * .75f; }
        }



        private void LateUpdate()
        {
            Vector3 closestLookDir;
            var closestDist = m_game.Level.Track.GetClosestTravelDist(transform.position, out closestLookDir);

            var segment = m_game.Level.GetTrackSegmentAtDist(closestDist);
            segment = m_game.Level.GetTrackSegmentAtIndex(segment.Index + 3);
            var projectionScale = segment.IsExtended ? 2f : 1f;

            TargetPosition = m_game.Level.Track.GetPosWithDir(closestDist, out m_trackDir);
            var projectedPos = TargetPosition + m_trackDir * projectionScale;
            var projectedPosNormal = (projectedPos - transform.position).normalized;

            var splitDir = Vector3.Lerp(m_trackDir, projectedPosNormal, .5f);
            TargetRotation = Quaternion.LookRotation(splitDir);

            // Maintain correct x rotation
            //var angles = TargetRotation.eulerAngles;
            //angles.x = closestLookDir.x;
            //TargetRotation.eulerAngles = angles;

            m_rigidBody.MoveRotation(Quaternion.Euler(transform.eulerAngles.x, TargetRotation.eulerAngles.y, transform.eulerAngles.z));
            m_rigidBody.rotation = TargetRotation;

            if (Input.GetMouseButton(0))
            {
                m_rigidBody.AddForce(splitDir * MaxSpeed);
            }

            if (m_rigidBody.velocity.magnitude > MaxSpeed)
            {
                m_rigidBody.velocity = m_rigidBody.velocity.normalized * MaxSpeed;
            }

            var angles = m_rigidBody.rotation.eulerAngles;
            if (Mathf.Abs(angles.z) > 8)
            {
                angles.z = Mathf.Sign(angles.z) * 8;
                m_rigidBody.rotation = Quaternion.Euler(angles);
            }
        }



        protected override void Update()
        {
            //base.Update();

            //UpdateState();
        }



        protected override bool ShowWake
        {
            get { return m_state != State.Jumping; }
        }



        protected override void UpdatePosition()
        {
            /*
            m_trackPos = m_game.Level.GetTrackPosWithDirAndUp(TravelDist, out m_lookDir, out m_up);

            transform.position = m_trackPos;
            transform.localRotation = Quaternion.LookRotation(m_lookDir);

            var right = -Vector3.Cross(m_lookDir, m_up);
            transform.position += right * m_lateralAbs;

            transform.position += m_up * Height;
            */
        }



        protected override float GetLateralAbs(float dist)
        {
            return m_lateralAbs;
        }


        /*
        private void UpdateState()
        {
            switch (m_state)
            {
                case State.Driving:
                    {
                        UpdateMovement(true);

                        if(m_activeRamp)
                        {
                            m_boat.transform.localRotation = Quaternion.Euler(m_activeRamp.BaseLaunchAngle, 0, 0);

                            var boatLength = m_boat.Length;

                            var p1 = transform.position + transform.forward * boatLength * .5f;
                            var p2 = transform.position;
                            var p3 = transform.position - transform.forward * boatLength * .5f;

                            //var pos = transform.position;
                            //pos.y = m_activeRamp.transform.position.y + m_activeRamp.BoatYOffset;
                            //transform.position = pos;
                        }
                        break;
                    }

                case State.Entering:
                case State.Upgrading:
                    {
                        UpdateMovement(false);

                        m_dropAccel += DropSpeed * Time.deltaTime;
                        m_dropSpeed += m_dropAccel;
                        Height = Mathf.Max(0, Height - m_dropSpeed * Time.deltaTime);

                        if (Height <= 0)
                        {
                            ChangeState(State.Driving);

                            if (m_state == State.Upgrading)
                            {
                                Boost(BoostType.Upgrade);
                            }
                        }
                        break;
                    }

                case State.Jumping:
                    {
                        UpdateMovement(m_jumpBounceCount > 0);

                        if (m_flipTime <= 0 && Input.GetMouseButtonDown(0))
                        {
                            m_flipTime = FlipTime;
                        }

                        m_jumpTime = Mathf.Max(0, m_jumpTime - Time.deltaTime);

                        var progress = 1f - (m_jumpTime / m_jumpTimeStart);
                        var progress010 = Mathf.Sin(Mathf.PI * progress);

                        var pos = transform.position;
                        pos.y += m_jumpHeight * progress010;
                        transform.position = pos;

                        var flipProgress = Mathf.Sin(Mathf.PI * .5f * (1f - (m_flipTime / FlipTime)));
                        var xRotation = m_rampAngle - flipProgress * 360;
                        m_boat.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

                        m_flipTime = Mathf.Max(0, m_flipTime - Time.deltaTime);

                        if (m_jumpTime <= 0)
                        {
                            if (Mathf.Abs(Mathf.DeltaAngle(0, xRotation)) > 15)
                            {
                                ChangeState(State.Exploding);
                            }
                            else
                            {
                                if (++m_jumpBounceCount <= JumpBounces)
                                {
                                    m_jumpTime = m_jumpTimeStart = JumpTime * m_jumpScale * .075f;
                                    m_jumpHeight = JumpHeight * m_jumpScale * .075f;
                                }
                                else
                                {
                                    m_boat.transform.localRotation = Quaternion.identity;
                                    ChangeState(State.Driving);
                                }
                            }
                        }
                        break;
                    }

                case State.Exploding:
                    {
                        m_deathDelay -= Time.deltaTime;
                        if (m_deathDelay <= 0)
                        {
                            m_game.OnPlayerDeath();
                            ChangeState(State.None);
                        }
                        break;
                    }
            }
        }
        */


        private void UpdateMovement(bool useInput)
        {
            if (useInput && Input.GetMouseButton(0))
            {
                m_rigidBody.AddForce(transform.forward * m_boat.Acceleration);
            }


            TravelDist = m_game.Level.Track.GetClosestTravelDist(transform.position);
        }



       private void ChangeState(State state)
        {
            switch(state)
            {
                case State.Driving:
                    Height = 0;
                    break;

                case State.Entering:
                    Height = DropHeight;
                    m_rigidBody.velocity = Vector3.zero;
                    break;

                case State.Upgrading:
                    m_game.ShowSmokePuff(m_trackPos);

                    Height = DropHeight;
                    m_dropSpeed = m_dropAccel = 0;

                    //m_velocity = 0;
                    //m_lateralAbs = 0;
                    break;

                case State.Jumping:
                    m_flipTime = 0;
                    m_jumpBounceCount = 0;

                    m_jumpHeight = JumpHeight * m_jumpScale;
                    m_jumpTime = m_jumpTimeStart = JumpTime * m_jumpScale;
                    break;

                case State.Exploding:
                    m_boat.Explode();
                    m_deathDelay = 2;
                    break;
            }

            m_state = state;
        }



        private void OnCollisionEnter(Collision collision)
        {
            if(gameObject.activeInHierarchy && collision.gameObject.activeInHierarchy)
            {
                if(collision.gameObject.layer == LayerMask.NameToLayer("Pickup"))
                {
                    OnPickupCollision(collision.gameObject.GetComponent<Pickup>());
                }
                else if(collision.gameObject.layer == LayerMask.NameToLayer("Prop"))
                {
                    OnPropCollision(collision.gameObject.GetComponent<Prop>());
                }
                else if (!m_game.Main.NoDeath && collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
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
                    if (!IsJumping)
                    {
                        var ramp = (JumpRampProp)prop;
                        m_rampAngle = ramp.BaseLaunchAngle;

                        Boost(BoostType.Ramp, ramp.BoostScale);

                        m_jumpScale = SpeedNormal * Mathf.Clamp01(Mathf.InverseLerp(ramp.SpeedBoostRange.x, ramp.SpeedBoostRange.y, ramp.BoostScale));

                        if (m_jumpScale > 0)
                        {
                            ChangeState(State.Jumping);
                        }
                    }
                    break;

                case PropType.CheatRamp:
                    if (!IsOnRamp)
                    {
                        m_activeRamp = (RampProp)prop;
                    }
                    break;

                default:
                    Debug.LogError("OnPropCollision did not handle type " + prop.Type);
                    return;
            }
        }



        private bool IsOnRamp
        {
            get { return m_activeRamp != null; }
        }



        private bool IsJumping
        {
            get { return m_state == State.Jumping; }
        }



        private float SpeedNormal
        {
            get { return Mathf.Clamp01(m_rigidBody.velocity.magnitude / m_boat.MaxSpeed); }
        }



        private void OnEnemyCollision(Enemy enemy)
        {
            ChangeState(State.Exploding);
        }



        private void OnPickupCollision(Pickup pickup)
        {
            m_game.OnPickupCollision(pickup);
        }



        private enum State
        {
            None = -1,
            Driving,
            Entering,
            Jumping,
            Upgrading,
            Exploding,
            Count
        }



        public enum BoostType
        {
            None = -1,
            Upgrade,
            Ramp,
            Count
        }



        private State       m_state;
        private Vector3     m_trackPos;

        private RampProp    m_activeRamp;

        private Rigidbody   m_rigidBody;

        private float       m_rampAngle;
        private int         m_jumpBounceCount;
        private float       m_jumpScale, m_jumpTimeStart, m_jumpTime, m_jumpHeight, m_lateralAbs, m_boostTime, m_boostScale, m_dropSpeed, m_dropAccel, m_flipTime, m_deathDelay;
    }
}
#endif