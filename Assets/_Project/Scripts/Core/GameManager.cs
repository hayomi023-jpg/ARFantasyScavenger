using UnityEngine;

namespace ARFantasy.Core
{
    public enum GameState
    {
        Menu,
        Scanning,
        Playing,
        Paused,
        Completed
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.Menu;
        public GameState CurrentState => currentState;

        [Header("Hunt Settings")]
        [SerializeField] private int totalItemsToCollect = 5;
        [SerializeField] private int itemsCollected = 0;
        public int TotalItemsToCollect => totalItemsToCollect;
        public int ItemsCollected => itemsCollected;

        [Header("Score")]
        [SerializeField] private int currentScore = 0;
        public int CurrentScore => currentScore;

        public delegate void GameStateChanged(GameState newState);
        public event GameStateChanged OnGameStateChanged;

        public delegate void ScoreChanged(int newScore);
        public event ScoreChanged OnScoreChanged;

        public delegate void ItemCollected(int collected, int total);
        public event ItemCollected OnItemCollected;

        public delegate void TimeTick(int secondsRemaining);
        public event TimeTick OnTimeTick;

        public delegate void TimeExpired();
        public event TimeExpired OnTimeExpired;

        [Header("Timer")]
        [SerializeField] private float timeLimit = 0f;
        private float timeRemaining = 0f;
        private bool isTimerActive = false;

        public float TimeLimit => timeLimit;
        public float TimeRemaining => timeRemaining;
        public bool HasTimeLimit => timeLimit > 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetGameState(GameState newState)
        {
            if (currentState == newState) return;
            currentState = newState;
            OnGameStateChanged?.Invoke(newState);
        }

        public void StartNewHunt()
        {
            itemsCollected = 0;
            currentScore = 0;
            SetGameState(GameState.Scanning);
            OnScoreChanged?.Invoke(currentScore);
            OnItemCollected?.Invoke(itemsCollected, totalItemsToCollect);
        }

        public void ConfigureHunt(int itemCount, float timeLimit = 0f)
        {
            totalItemsToCollect = itemCount;
            this.timeLimit = timeLimit;
            Debug.Log($"Hunt configured: {itemCount} items, time limit: {timeLimit}s");
        }

        public void StartPlaying()
        {
            if (timeLimit > 0f)
            {
                timeRemaining = timeLimit;
                isTimerActive = true;
            }
            SetGameState(GameState.Playing);
        }

        public void CollectItem(int points)
        {
            itemsCollected++;
            currentScore += points;

            OnScoreChanged?.Invoke(currentScore);
            OnItemCollected?.Invoke(itemsCollected, totalItemsToCollect);

            if (itemsCollected >= totalItemsToCollect)
            {
                CompleteHunt();
            }
        }

        private void CompleteHunt()
        {
            SetGameState(GameState.Completed);
        }

        public void PauseGame()
        {
            if (currentState == GameState.Playing)
            {
                SetGameState(GameState.Paused);
                Time.timeScale = 0f;
            }
        }

        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
            {
                Time.timeScale = 1f;
                SetGameState(GameState.Playing);
            }
        }

        private void Update()
        {
            if (isTimerActive && currentState == GameState.Playing)
            {
                timeRemaining -= Time.deltaTime;

                if (timeRemaining <= 0f)
                {
                    timeRemaining = 0f;
                    isTimerActive = false;
                    OnTimeExpired?.Invoke();
                    // Time's up - end the hunt (could be failure or partial completion)
                    CompleteHunt();
                }
                else
                {
                    // Fire time tick every second
                    int seconds = Mathf.CeilToInt(timeRemaining);
                    OnTimeTick?.Invoke(seconds);
                }
            }
        }

        /// <summary>
        /// Get time bonus based on remaining time
        /// </summary>
        public int CalculateTimeBonus(int bonusPerSecond)
        {
            if (!HasTimeLimit) return 0;
            return Mathf.CeilToInt(timeRemaining) * bonusPerSecond;
        }
    }
}
