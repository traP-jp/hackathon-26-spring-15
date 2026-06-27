namespace MyProject.Model
{
    public readonly struct ScoreSaveData
    {
        public int Score { get; }

        public ScoreSaveData(int score)
        {
            Score = score;
        }
    }
}
