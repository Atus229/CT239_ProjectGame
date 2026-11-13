using _Project.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Ui
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Menu Buttons")] [SerializeField]
        private Button startButton;
        [SerializeField] private Button btnHowToPlay;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        [Header("Popups")] [Tooltip("Kéo Component SettingsPopupUI vào đây")] [SerializeField]
        private SettingsPopupUI settingsPopup;

        [Tooltip("Kéo Component TutorialUI vào đây")]
        [SerializeField] private TutorialUI tutorialPopup;

        [Header("Scene Transition")] [SerializeField]
        private string targetSceneName = "PackingScene";

        private void Awake()
        {
            #if UNITY_ANDROID || UNITY_IOS
            if (exitButton != null)
            {
                exitButton.gameObject.SetActive(false);
            }
            #endif

            if (startButton != null) startButton.onClick.AddListener(OnStartClicked);
            if (btnHowToPlay != null) btnHowToPlay.onClick.AddListener(OnHowToPlayClicked);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
            if (exitButton != null) exitButton.onClick.AddListener(OnExitClicked);

        }

        private void OnDestroy()
        {
            if (startButton != null) startButton.onClick.RemoveListener(OnStartClicked);
            if (btnHowToPlay != null) btnHowToPlay.onClick.RemoveListener(OnHowToPlayClicked);
            if (settingsButton != null) settingsButton.onClick.RemoveListener(OnSettingsClicked);
            if (exitButton != null) exitButton.onClick.RemoveListener(OnExitClicked);
        }

        private void OnStartClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (startButton != null) startButton.interactable = false;
            SceneManager.LoadScene(targetSceneName);
        }

        private void OnHowToPlayClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (tutorialPopup != null)
            {
                tutorialPopup.OpenPopup();
            }
            else
            {
                Debug.LogWarning("[MainMenuUI] Chưa gán TutorialUI vào Inspector!");
            }
        }

        private void OnSettingsClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (settingsPopup != null)
            {
                settingsPopup.Open();
            }
            else
            {
                Debug.LogWarning("[MainMenuUI] Chưa gán SettingsPopupUI vào Inspector!");
            }
        }

        private void OnExitClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            Debug.Log("[MainMenuUI] Đang thoát trò chơi...");

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}