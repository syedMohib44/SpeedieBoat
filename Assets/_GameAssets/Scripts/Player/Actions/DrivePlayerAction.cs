using UnityEngine;



namespace SpeedyBoat
{
    public class DrivePlayerAction : PlayerAction
    {
        public override void Update()
        {
            var decelerate = m_player.Game.Mode != Game.GameMode.LevelComplete;
            m_player.UpdateMovement(true, decelerate);

            m_player.UpdateLateralPos();
        }



        public DrivePlayerAction(Player player, PlayerActionInitialiser initialiser)
             : base(player, initialiser)
        {
            m_player.Height = 0;
        }
    }
}