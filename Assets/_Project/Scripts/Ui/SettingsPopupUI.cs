using _Project.Scripts.Core;
using _Project.Scripts.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Project.Scripts.Ui
{
    public class SettingsPopupUI : MonoBehaviour
    {
        [Header("Audio Sliders")]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Action Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button resetDataButton;

        [Header("Reset Confirmation Modal")]
        [SerializeField] private GameObject confirmResetModal;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;

        private float _lastSfxPreviewTime;
        private const float SfxPreviewCooldown = 0.15f;
        private bool _isSyncing;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }

            if (resetDataButton != null)
            {
                resetDataButton.onClick.AddListener(OnResetDataClicked);
            }

            if (confirmYesButton != null)
            {
                confirmYesButton.onClick.AddListener(ConfirmResetData);
            }

            if (confirmNoButton != null)
            {
                confirmNoButton.onClick.AddListener(CancelResetData);
            }

            if (bgmSlider != null)
            {
                bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
                AddPointerUpListener(sfxSlider.gameObject, () =>
                {
                    if (!_isSyncing) AudioManager.Instance?.PlayButtonClick();
                });
            }
        }

        private void OnEnable()
        {
            SyncAudioSliders();

            if (confirmResetModal != null)
            {
                confirmResetModal.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (resetDataButton != null) resetDataButton.onClick.RemoveListener(OnResetDataClicked);
            if (confirmYesButton != null) confirmYesButton.onClick.RemoveListener(ConfirmResetData);
            if (confirmNoButton != null) confirmNoButton.onClick.RemoveListener(CancelResetData);

            if (bgmSlider != null) bgmSlider.onValueChanged.RemoveListener(OnBgmSliderChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
        }
        
        public void SyncAudioSliders()
        {
            if (AudioManager.Instance != null)
            {
                _isSyncing = true;
                if (bgmSlider != null) bgmSlider.value = AudioManager.Instance.BGMVolume;
                if (sfxSlider != null) sfxSlider.value = AudioManager.Instance.SFXVolume;
                _isSyncing = false;
            }
        }

        public void Open()
        {
            AudioManager.Instance?.PlayButtonClick();
            gameObject.SetActive(true);
            SyncAudioSliders();
        }

        public void Close()
        {
            AudioManager.Instance?.PlayButtonClick();
            PlayerPrefs.Save(); //Lưu khi đóng bảng cài đặt
            gameObject.SetActive(false);
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

                // Phát âm thanh xem trước âm lượng SFX vừa chỉnh (có cooldown để không bị dồn dập khi kéo nhanh)
                if (!_isSyncing && Time.unscaledTime - _lastSfxPreviewTime > SfxPreviewCooldown)
                {
                    _lastSfxPreviewTime = Time.unscaledTime;
                    AudioManager.Instance.PlayButtonClick();
                }
            }
        }

        private void AddPointerUpListener(GameObject target, System.Action onPointerUp)
        {
            EventTrigger trigger = target.GetComponent<EventTrigger>();
            if (trigger == null) trigger = target.AddComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            entry.callback.AddListener((_) => onPointerUp?.Invoke());
            trigger.triggers.Add(entry);
        }

        private void OnResetDataClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (confirmResetModal != null)
            {
                confirmResetModal.SetActive(true);
            }
            else
            {
                ConfirmResetData();
            }
        }

        private void ConfirmResetData()
        {
            AudioManager.Instance?.PlayButtonClick();
            Debug.Log("[SettingsPopupUI] Đang xóa toàn bộ dữ liệu lưu (Reset Game về Ngày 1)...");

            // Lưu lại mức âm lượng hiện tại trước khi xóa PlayerPrefs
            float currentBgm = AudioManager.Instance != null ? AudioManager.Instance.BGMVolume : 0.75f;
            float currentSfx = AudioManager.Instance != null ? AudioManager.Instance.SFXVolume : 0.75f;

            // Xóa dữ liệu game
            PlayerPrefs.DeleteAll();

            // Khôi phục lại cấu hình âm lượng để người chơi không bị mất cài đặt âm thanh
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetBGMVolume(currentBgm);
                AudioManager.Instance.SetSFXVolume(currentSfx);
            }

            PlayerPrefs.Save();

            if (confirmResetModal != null)
            {
                confirmResetModal.SetActive(false);
            }

            Debug.Log("[SettingsPopupUI] Đã xóa dữ liệu thành công! Người chơi sẽ bắt đầu lại từ Ngày 1.");
        }

        private void CancelResetData()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (confirmResetModal != null)
            {
                confirmResetModal.SetActive(false);
            }
        }
    }
}
