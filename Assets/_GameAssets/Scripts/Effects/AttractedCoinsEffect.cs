using UnityEngine;



namespace SpeedyBoat
{
    public class AttractedCoinsEffect : MonoBehaviour
    {
        protected virtual int CoinValue
        {
            get { return 1; }
        }



        public virtual void Setup(Game game, float coinScale)
        {
            gameObject.SetActive(true);

            m_game = game;
            m_lastParticleCount = 0;

            var psMain = m_particleSystem.main;
            psMain.startSize = m_startSize * coinScale;
        }



        private void OnDisable()
        {
            if (m_game)
            {
                // Award any remaining coins still in the air
                var remainingToAdd = m_particleSystem.particleCount;
                for (int i = 0; i < remainingToAdd; ++i)
                {
                    OnCoinComplete(CoinValue);
                }

                m_particleSystem.Clear();
                m_lastParticleCount = 0;
            }
        }



        protected virtual void LateUpdate()
        {
            var particles = new ParticleSystem.Particle[m_particleSystem.particleCount];
            if (particles.Length > 0)
            {
                m_particleSystem.GetParticles(particles);

                var target = m_game.Main.UIManager.CurrencyCoinPosition;

                for (int i = 0; i < particles.Length; ++i)
                {
                    var p = particles[i];

                    var progress = 1f - p.remainingLifetime / p.startLifetime;

                    if (progress > .25f)
                    {
                        progress = (progress - .25f) / .75f;

                        var pos = Vector3.Lerp(p.position, target, Mathf.Pow(progress, 2));
                        pos.z = target.z;

                        particles[i].position = pos;
                    }
                }

                m_particleSystem.SetParticles(particles, particles.Length);
            }

            var finishedCount = Mathf.Max(0, m_lastParticleCount - particles.Length);
            for(int i=0;i < finishedCount;++i)
            {
                OnCoinComplete(CoinValue);
            }

            m_lastParticleCount = particles.Length;
        }



        protected virtual void OnCoinComplete(int value)
        {
            Debug.Log("OnCoinComplete: value = " + value);
            m_game.OnAttractedCoinsCollected(value);
        }



        private void Awake()
        {
            m_particleSystem = GetComponent<ParticleSystem>();

            var psMain = m_particleSystem.main;
            m_startSize = psMain.startSize.constant;
        }



        protected Game              m_game;
        protected ParticleSystem    m_particleSystem;

        private float               m_startSize;
        private int                 m_lastParticleCount;
    }
}