using UnityEngine;



namespace SpeedyBoat
{
    public class Enemy : MonoBehaviour
    {
        public EnemyType Type        { get; private set; }
        public Enemies   Container   { get; private set; }



        public virtual void Setup(EnemyType type, Enemies container, EnemyInitialiser initialiser)
        {
            Type = type;

            Container = container;
            m_initialiser = initialiser;

            gameObject.SetActive(true);
        }



        protected Track Track
        {
            get { return m_initialiser.Track; }
        }



        protected EnemyInitialiser m_initialiser;
    }
}