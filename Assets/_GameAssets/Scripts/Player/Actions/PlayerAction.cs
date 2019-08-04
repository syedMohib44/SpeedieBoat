using UnityEngine;



namespace SpeedyBoat
{
    public abstract class PlayerAction
    {
        public virtual void Destroy()
        {
        }



        public abstract void Update();



        protected PlayerAction(Player player, PlayerActionInitialiser initialiser)
        {
            m_player = player;
            m_initialiser = initialiser;
        }



        protected readonly Player m_player;
        protected readonly PlayerActionInitialiser m_initialiser;
    }
}