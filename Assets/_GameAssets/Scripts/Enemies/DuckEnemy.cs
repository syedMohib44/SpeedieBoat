using UnityEngine;



namespace SpeedyBoat
{
    public class DuckEnemy : Enemy
    {
        public float WakeInterval       = .25f;
        public float WakeLength         = .25f;
        public float WakeWidthStart     = .1f;
        public float WakeHeight         = .01f;



        public override void Setup(EnemyType type, Enemies container, EnemyInitialiser initialiser)
        {
            base.Setup(type, container, initialiser);

            var duckInitialiser = (DuckEnemyInitialiser)m_initialiser;

            m_model = transform.FindIncludingInactive("Model");

            m_lifeTime = 0;

            m_travelDist = m_lastWakeDist = duckInitialiser.DistStart;
            m_activeDelay = duckInitialiser.ActiveDelay * duckInitialiser.GroupIndex;

            m_model.gameObject.SetActive(m_activeDelay <= 0);
            m_model.transform.localScale = Vector3.one * (duckInitialiser.GroupIndex > 0 ? .5f : 1.5f);
        }



        private void Update()
        {
            if(m_activeDelay > 0)
            {
                m_activeDelay = Mathf.Max(0, m_activeDelay - Time.deltaTime);
                if(m_activeDelay <= 0)
                {
                    m_model.gameObject.SetActive(true);
                }
                return;
            }

            var duckInitialiser = (DuckEnemyInitialiser)m_initialiser;

            var oldPos = transform.position;

            Vector3 lookDir, up;
            var pos = Track.GetPosWithDirAndUp(m_travelDist, out lookDir, out up);

            m_heightOffset = .1f + Mathf.Sin(Mathf.PI * 2 * m_lifeTime) * .01f;
            pos.y += m_heightOffset;

            transform.position = pos;

            var lateralAbs = GetLateralAbs(m_travelDist);
            transform.position += Vector3.Cross(lookDir, up) * -lateralAbs;

            transform.localRotation = Quaternion.LookRotation((transform.position - oldPos).normalized);

            UpdateWake();

            m_lifeTime += Time.deltaTime;
            m_travelDist += duckInitialiser.ForwardSpeed * Time.deltaTime;
        }



        private float GetLateralAbs(float dist)
        {
            var duckInitialiser = (DuckEnemyInitialiser)m_initialiser;
            var bounceTime = duckInitialiser.LateralTime;

            var lateralPos = (Mathf.Sin(Mathf.PI * 2 * (m_lifeTime / bounceTime)) + 1) * .5f;

            var lateralLmits = Track.GetLateralLimits(dist);

            // Prevent moving to the absolute edge
            lateralLmits *= .9f;

            return Mathf.Lerp(lateralLmits.x, lateralLmits.y, lateralPos);
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



        private Transform   m_model;
        private float       m_activeDelay, m_travelDist, m_lifeTime, m_startSpin, m_lastWakeDist, m_heightOffset;
    }
}