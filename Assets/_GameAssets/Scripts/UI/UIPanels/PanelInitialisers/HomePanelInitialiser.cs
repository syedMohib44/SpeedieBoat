


namespace SpeedyBoat
{
    public class HomePanelInitialiser : PanelInitialiser
    {
        public string BestScore { get; private set; }



        public HomePanelInitialiser(int bestScore)
        {
            BestScore = bestScore.ToString();
        }
    }
}
