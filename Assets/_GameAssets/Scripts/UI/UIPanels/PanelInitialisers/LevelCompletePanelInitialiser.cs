


namespace SpeedyBoat
{
    public class LevelCompletePanelInitialiser : PanelInitialiser
    {
        public readonly int Coins;
        public readonly int Bonus;
        public readonly int Position;



        public LevelCompletePanelInitialiser(int coins, int bonus, int position)
        {
            Coins = coins;
            Bonus = bonus;
            Position = position + 1;
        }
    }
}