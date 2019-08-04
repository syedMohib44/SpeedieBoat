


namespace SpeedyBoat
{
    public class GameOverPanelInitialiser : PanelInitialiser
    {
        public string Score     { get; private set; }
        public string BestScore { get; private set; }



        public GameOverPanelInitialiser(int score, int bestScore)
        {
            Score = score.ToString();
            BestScore = bestScore.ToString();
        }
    }
}