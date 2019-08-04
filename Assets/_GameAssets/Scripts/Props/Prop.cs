using UnityEngine;



namespace SpeedyBoat
{
    public class Prop : MonoBehaviour
    {
        public PropType Type { get; private set; }



        public virtual void Setup(PropType type, Props container, PropInitialiser initialiser)
        {
            Type = type;

            m_container = container;
            m_initialiser = initialiser;

            gameObject.SetActive(true);
        }



        protected Track Track
        {
            get { return m_initialiser.Track; }
        }



        protected Props             m_container;
        protected PropInitialiser   m_initialiser;
    }
}