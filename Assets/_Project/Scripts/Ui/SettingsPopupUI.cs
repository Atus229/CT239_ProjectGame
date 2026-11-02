using _Project.Scripts.Core;
using _Project.Scripts.Managers;
using UnityEngine;
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
                if (bgmSlider != null) bgmSlider.value = AudioManager.Instance.BGMVolume;
                if (sfxSlider != null) sfxSlider.value = AudioManager.Instance.SFXVolume;
            }
        }

        public void Open()
        {
            gameObject.SetActive(true);
            SyncAudioSliders();
        }

        public void Close()
        {
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
            }
        }

        private void OnResetDataClicked()
        {
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
            if (confirmResetModal != null)
            {
                confirmResetModal.SetActive(false);
            }
        }
    }
}

