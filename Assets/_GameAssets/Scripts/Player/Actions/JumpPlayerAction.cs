using UnityEngine;



namespace SpeedyBoat
{
    public abstract class JumpPlayerAction : PlayerAction
    {
        public abstract float JumpProgress { get; }



        protected JumpPlayerAction(Player player, PlayerActionInitialiser initialiser)
            : base(player, initialiser)
        {
        }
    }
}