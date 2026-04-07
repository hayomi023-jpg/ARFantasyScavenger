using UnityEngine;
using UnityEngine.SceneManagement;
using ARFantasy.Core;

namespace ARFantasy.UI
{
    /// <summary>
    /// Central UI manager that coordinates all UI panels and responds to game state changes
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject scanningPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject winPanel;

        [Header("HUD Controller")]
        [SerializeField] private HUDController hudController;

        [Header("Settings")]
        [SerializeField] private bool showMenuOnStart = true;

        public static UIManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Subscribe to game state changes
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }

            // Initialize UI
            HideAllPanels();

            if (showMenuOnStart)
            {
                ShowMainMenu();
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        private void OnGameStateChanged(GameState newState)
        {
            HideAllPanels();

            switch (newState)
            {
                case GameState.Menu:
                    ShowMainMenu();
                    break;
                case GameState.Scanning:
                    ShowScanningUI();
                    break;
                case GameState.Playing:
                    ShowHUD();
                    break;
                case GameState.Paused:
                    ShowPauseMenu();
                    break;
                case GameState.Completed:
                    ShowWinScreen();
                    break;
            }
        }

        private void HideAllPanels()
        {
            if (mainMenuPanel) mainMenuPanel.SetActive(false);
            if (scanningPanel) scanningPanel.SetActive(false);
            if (hudPanel) hudPanel.SetActive(false);
            if (pausePanel) pausePanel.SetActive(false);
            if (winPanel) winPanel.SetActive(false);
        }

        #region Panel Show Methods

        public void ShowMainMenu()
        {
            HideAllPanels();
            if (mainMenuPanel) mainMenuPanel.SetActive(true);
        }

        public void ShowScanningUI()
        {
            HideAllPanels();
            if (scanningPanel) scanningPanel.SetActive(true);
        }

        public void ShowHUD()
        {
            HideAllPanels();
            if (hudPanel) hudPanel.SetActive(true);
            hudController?.UpdateHUD();
        }

        public void ShowPauseMenu()
        {
            if (pausePanel) pausePanel.SetActive(true);
        }

        public void HidePauseMenu()
        {
            if (pausePanel) pausePanel.SetActive(false);
        }

        public void ShowWinScreen()
        {
            HideAllPanels();
            if (winPanel) winPanel.SetActive(true);
        }

        #endregion

        #region Button Actions

        public void OnStartButtonClicked()
        {
            AudioManager.Instance?.PlayUIClickSound();
            GameManager.Instance?.StartNewHunt();
        }

        public void OnPauseButtonClicked()
        {
            AudioManager.Instance?.PlayUIClickSound();
            GameManager.Instance?.PauseGame();
        }

        public void OnResumeButtonClicked()
        {
            AudioManager.Instance?.PlayUIClickSound();
            GameManager.Instance?.ResumeGame();
        }

        public void OnRestartButtonClicked()
        {
            AudioManager.Instance?.PlayUIClickSound();
            GameManager.Instance?.StartNewHunt();
        }

        public void OnMainMenuButtonClicked()
        {
            AudioManager.Instance?.PlayUIClickSound();
            GameManager.Instance?.SetGameState(GameState.Menu);
        }

        public void OnQuitButtonClicked()
        {
            AudioManager.Instance?.PlayUIClickSound();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void OnSettingsButtonClicked()
        {
            AudioManager.Instance?.PlayUIClickSound();
            // TODO: Show settings panel
        }

        #endregion
    }
}
