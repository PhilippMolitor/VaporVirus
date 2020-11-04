namespace Score
{
    public class ScoreManager
    {
        // singleton instance
        private static ScoreManager _instance;
        
        // state
        private int _visitedWindows = 0;
        private int _destroyedFiles = 0;
        
        // singleton constructor
        public static ScoreManager Instance => _instance ?? (_instance = new ScoreManager());

        public int GetScore()
        {
            return 
                (_visitedWindows * 100)
                + (_destroyedFiles * 10);
        }

        public void ClearScore()
        {
            _visitedWindows = 0;
            _destroyedFiles = 0;
        }

        public void IncrementVisitedWindowCount() =>_visitedWindows += 1;

        public void IncrementDestroyedFileCount() => _destroyedFiles += 1;
    }
}
