using System;
using UnityEngine;



namespace SpeedyBoat
{
    public class ParticleEffect : MonoBehaviour
    {
        public void Setup(Vector3 pos, float scale, bool showStars, Action<GameObject> callback)
        {
            gameObject.SetActive(true);

            m_callback = callback;
            transform.position = pos;

            var ps = GetComponent<ParticleSystem>();
            var psMain = ps.main;
            psMain.startSize = new ParticleSystem.MinMaxCurve(m_startsize.x * scale, m_startsize.y * scale);

            transform.FindIncludingInactive("Stars").gameObject.SetActive(showStars);
        }



        public void OnParticleSystemStopped()
        {
            if (m_callback != null)
            {
                m_callback(gameObject);
            }
        }



        private void Awake()
        {
            var ps = GetComponent<ParticleSystem>();
            var psMain = ps.main;
            m_startsize = new Vector2(psMain.startSize.constantMin, psMain.startSize.constantMax);
        }



        private Vector2             m_startsize;
        private Action<GameObject>  m_callback;
    }
}