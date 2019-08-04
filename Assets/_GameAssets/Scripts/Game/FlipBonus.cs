using TMPro;
using UnityEngine;



namespace SpeedyBoat
{
    public class FlipBonus : MonoBehaviour
    {
        public float LifeTime = 1f;



        public void Setup(Player player, int score, int multiplier, ObjectPool pool)
        {
            gameObject.SetActive(true);

            transform.parent = player.transform;
            transform.localPosition = Vector3.up;

            m_Pool = pool;

            var numberRing = transform.FindIncludingInactive("Number");
            numberRing.GetComponent<SpriteRenderer>().material.renderQueue = 4000;

            m_value = transform.FindIncludingInactive("Value").GetComponent<TMP_Text>();
            m_value.text = "+" + score;

            m_multiplier = transform.FindIncludingInactive("Multiplier").GetComponent<TMP_Text>();
            m_multiplier.text = "x" + multiplier; 

            m_lifeTime = LifeTime;
        }



        private void Update()
        {
            m_lifeTime = Mathf.Max(0, m_lifeTime - Time.deltaTime);

            if(m_lifeTime <= 0)
            {
                m_Pool.FreeObject(gameObject);
            }
        }



        private ObjectPool  m_Pool;
        private float       m_lifeTime;
        private TMP_Text    m_value, m_multiplier;
    }
}