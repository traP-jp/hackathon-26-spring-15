namespace MyProject.Model
{
    public readonly struct RankingData
    {
        public int Score { get; }

        public RankingData(int score)
        {
            Score = score;
        }
    }
}
