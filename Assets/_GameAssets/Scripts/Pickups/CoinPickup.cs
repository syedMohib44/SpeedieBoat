using UnityEngine;



namespace SpeedyBoat
{
    public class CoinPickup : Pickup
    {
        public float WakeInterval       = .25f;
        public float WakeLength         = .25f;
        public float WakeWidthStart     = .1f;
        public float WakeHeight         = .01f;



        public override void Setup(PickupType type, Pickups container, PickupInitialiser initialiser)
        {
            base.Setup(type, container, initialiser);

            var coinInitialiser = (CoinPickupInitialiser)m_initialiser;

            m_sprite = transform.FindIncludingInactive("Sprite");

            m_lifeTime = 0;
            m_startSpin = Random.value * 360;

            m_travelDist = m_lastWakeDist = coinInitialiser.DistStart;
            m_activeDelay = coinInitialiser.ActiveDelay * coinInitialiser.GroupIndex;

            m_sprite.gameObject.SetActive(m_activeDelay <= 0);
        }



        private void Update()
        {
            if(m_activeDelay > 0)
            {
                m_activeDelay = Mathf.Max(0, m_activeDelay - Time.deltaTime);
                if(m_activeDelay <= 0)
                {
                    m_sprite.gameObject.SetActive(true);
                }
                return;
            }

            var coinInitialiser = (CoinPickupInitialiser)m_initialiser;

            Vector3 lookDir, up;
            var pos = Track.GetPosWithDirAndUp(m_travelDist, out lookDir, out up);

            m_heightOffset = Mathf.Sin(Mathf.PI * 2 * m_lifeTime) * .05f;
            pos.y += m_heightOffset;

            transform.position = pos;

            transform.localRotation = Quaternion.LookRotation(lookDir) * Quaternion.Euler(0, m_startSpin + m_lifeTime * 360, 0);

            var lateralAbs = GetLateralAbs(m_travelDist);
            transform.position += Vector3.Cross(lookDir, up) * -lateralAbs;

            UpdateWake();

            m_lifeTime += Time.deltaTime;
            m_travelDist += coinInitialiser.ForwardSpeed * Time.deltaTime;
        }



        private float GetLateralAbs(float dist)
        {
            /*
            var coinInitialiser = (CoinPickupInitialiser)m_initialiser;
            var bounceTime = coinInitialiser.LateralTime;

            var lateralPos = (Mathf.Sin(Mathf.PI * 2 * (m_lifeTime / bounceTime)) + 1) * .5f;

            var lateralLmits = Track.GetLateralLimits(dist);

            // Prevent moving to the absolute edge
            lateralLmits *= .9f;
            */

            return 0;// Mathf.Lerp(lateralLmits.x, lateralLmits.y, lateralPos);
        }



        private void UpdateWake()
        {
            if (m_heightOffset < 0)
            {
                var wakeCount = Mathf.FloorToInt((m_travelDist - m_lastWakeDist) / WakeInterval);
                for (int i = 0; i < wakeCount; ++i)
                {
                    m_lastWakeDist += WakeInterval;

                    // Each Wake segment destroys itself once complete
                    var lateralAbs = GetLateralAbs(m_lastWakeDist);

                    //Track.CreateDecal(TrackDecalType.Wake, m_lastWakeDist, lateralAbs, WakeWidthStart, WakeHeight, WakeLength);
                }
            }
            else
            {
                m_lastWakeDist = m_travelDist;
            }
        }



        private Transform   m_sprite;
        private float       m_activeDelay, m_travelDist, m_lifeTime, m_startSpin, m_lastWakeDist, m_heightOffset;
    }
}