using UnityEngine;



namespace SpeedyBoat
{
    public class Tutorial : MonoBehaviour
    {
        void LateUpdate()
        {
            var ps = m_boatSpray.GetComponent<ParticleSystem>();

            var psMain = ps.main;
            var max = psMain.maxParticles;

            var psEmission = m_boatSpray.GetComponent<ParticleSystem>().emission;
            psEmission.rateOverTime = m_hand.localScale.x < 1 ? max : 0;
        }



        private void Awake()
        {
            m_boatSpray = transform.FindIncludingInactive("Spray");
            m_hand = transform.FindIncludingInactive("HandIcon");
        }



        private Transform m_hand;
        private Transform m_boatSpray;
    }
}