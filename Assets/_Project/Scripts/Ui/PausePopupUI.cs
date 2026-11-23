using _Project.Scripts.Core;
using _Project.Scripts.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Project.Scripts.Ui
{
    public class PausePopupUI : MonoBehaviour
    {
        public static PausePopupUI Instance { get; private set; }

        [Header("UI Control")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Audio Sliders (Chỉnh âm lượng khi Pause)")]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Action Buttons")]
        [SerializeField] private Button resumeButton;    // Nút Tiếp tục (hoặc nút X đóng)
        [SerializeField] private Button mainMenuButton;  // Nút Về Menu Chính (thay cho Reset Data)
        [SerializeField] private Button quitGameButton;  // Nút Thoát Game (tuỳ chọn)

        private bool _isSyncing;
        private bool _isPaused;

        public bool IsPaused => _isPaused;

        private void Awake()
        {
            Instance = this;

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            // Gán sự kiện cho các nút
            if (resumeButton != null)
                resumeButton.onClick.AddListener(ResumeGame);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(GoToMainMenu);

            if (quitGameButton != null)
                quitGameButton.onClick.AddListener(QuitGame);

            // Gán sự kiện chỉnh âm thanh
            if (bgmSlider != null)
                bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);

            if (sfxSlider != null)
                sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }

        private void Start()
        {
            // Mặc định ẩn bảng Pause khi bắt đầu game (vẫn giữ GameObject Active để Update bắt được phím Esc)
            SetVisible(false);
        }

        private void Update()
        {
            // Bắt phím Escape theo New Input System
            bool isEscPressed = false;
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                isEscPressed = UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame;
            }

            if (isEscPressed)
            {
                if (_isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }

        public void PauseGame()
        {
            Time.timeScale = 0f; // ĐÓNG BĂNG THỜI GIAN GAME
            SetVisible(true);
            SyncAudioSliders();
            AudioManager.Instance?.PlayButtonClick();
            Debug.Log("[PausePopupUI] Game đã TẠM DỪNG (TimeScale = 0).");
        }

        public void ResumeGame()
        {
            Time.timeScale = 1f; // KHÔI PHỤC THỜI GIAN GAME
            PlayerPrefs.Save();
            SetVisible(false);
            AudioManager.Instance?.PlayButtonClick();
            Debug.Log("[PausePopupUI] Game đã TIẾP TỤC (TimeScale = 1).");
        }

        private void SetVisible(bool visible)
        {
            _isPaused = visible;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.blocksRaycasts = visible;
                canvasGroup.interactable = visible;
            }
            else
            {
                // Fallback: bật/tắt toàn bộ các con bên trong nếu thiếu CanvasGroup
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(visible);
                }
            }

            if (visible)
            {
                transform.SetAsLastSibling(); // Luôn nổi lên trên cùng màn hình
            }
        }

        private void GoToMainMenu()
        {
            Time.timeScale = 1f; // Bắt buộc trả lại 1 trước khi đổi Scene
            PlayerPrefs.Save();
            AudioManager.Instance?.PlayButtonClick();
            LoadingScreenUI.LoadScene("MainMenu");
        }

        private void QuitGame()
        {
            Time.timeScale = 1f;
            PlayerPrefs.Save();
            AudioManager.Instance?.PlayButtonClick();
            Debug.Log("[PauseMenu] Đang thoát game...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void SyncAudioSliders()
        {
            if (AudioManager.Instance != null)
            {
                _isSyncing = true;
                if (bgmSlider != null) bgmSlider.value = AudioManager.Instance.BGMVolume;
                if (sfxSlider != null) sfxSlider.value = AudioManager.Instance.SFXVolume;
                _isSyncing = false;
            }
        }

        private void OnBgmSliderChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetBGMVolume(value);
            }
        }

        private void OnSfxSliderChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(value);
            }
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f; // Đảm bảo không bị kẹt timeScale khi huỷ Scene
        }
    }
}