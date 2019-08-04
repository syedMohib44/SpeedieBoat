using UnityEngine;



// Panel baseclass 
namespace SpeedyBoat
{
    public abstract class UIPanel
    {
        public virtual Vector3 CurrencyCoinPosition
        {
            get { return Vector3.zero; }
        }



        public virtual void UpdateCoins(int count)
        {
        }



        public virtual void Update()
        {
        }



        public virtual void Open(PanelInitialiser initialiser)
        {
            transform.gameObject.SetActive(true);
        }



        public virtual void Close()
        {
            transform.gameObject.SetActive(false);
        }



        protected UIPanel(UIManager uiManager, Transform t)
        {
            transform = t;
            m_uiManager = uiManager;
        }



        protected readonly Transform transform;
        protected readonly UIManager m_uiManager;
    }
}