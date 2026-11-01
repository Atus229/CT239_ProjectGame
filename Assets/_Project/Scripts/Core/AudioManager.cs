using UnityEngine;

namespace _Project.Scripts.Core
{
    /// <summary>
    /// Quản lý âm thanh tập trung (BGM & SFX) xuyên suốt toàn bộ trò chơi (DontDestroyOnLoad).
    /// Hỗ trợ lưu trữ cấu hình âm lượng tự động qua PlayerPrefs.
    /// </summary>
    [DefaultExecutionOrder(-90)]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private const string PrefKeyBgmVolume = "AudioManager_BGM_Volume";
        private const string PrefKeySfxVolume = "AudioManager_SFX_Volume";

        private const float DefaultVolume = 0.75f;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Runtime Volumes")]
        [Range(0f, 1f)] [SerializeField] private float bgmVolume = DefaultVolume;
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = DefaultVolume;

        public float BGMVolume => bgmVolume;
        public float SFXVolume => sfxVolume;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSources();
                LoadSavedVolumes();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Khởi tạo và kiểm tra các AudioSource nếu chưa được gán thủ công qua Inspector.
        /// </summary>
        private void InitializeAudioSources()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }
        }

        /// <summary>
        /// Tải cài đặt âm lượng đã lưu từ PlayerPrefs.
        /// </summary>
        private void LoadSavedVolumes()
        {
            bgmVolume = PlayerPrefs.GetFloat(PrefKeyBgmVolume, DefaultVolume);
            sfxVolume = PlayerPrefs.GetFloat(PrefKeySfxVolume, DefaultVolume);

            ApplyVolumes();
        }

        /// <summary>
        /// Áp dụng mức âm lượng thực tế vào các AudioSource.
        /// </summary>
        private void ApplyVolumes()
        {
            if (bgmSource != null)
            {
                bgmSource.volume = bgmVolume;
            }

            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume;
            }
        }

        #region Volume Control APIs

        /// <summary>
        /// Điều chỉnh âm lượng Nhạc nền (BGM) và lưu vào PlayerPrefs.
        /// </summary>
        /// <param name="volume">Mức âm lượng từ 0.0f đến 1.0f</param>
        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            if (bgmSource != null)
            {
                bgmSource.volume = bgmVolume;
            }
            PlayerPrefs.SetFloat(PrefKeyBgmVolume, bgmVolume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Điều chỉnh âm lượng Hiệu ứng âm thanh (SFX) và lưu vào PlayerPrefs.
        /// </summary>
        /// <param name="volume">Mức âm lượng từ 0.0f đến 1.0f</param>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume;
            }
            PlayerPrefs.SetFloat(PrefKeySfxVolume, sfxVolume);
            PlayerPrefs.Save();
        }

        #endregion

        #region BGM Playback APIs

        /// <summary>
        /// Phát bài nhạc nền mới. Nếu bài nhạc đã đang phát thì không khởi động lại từ đầu.
        /// </summary>
        public void PlayBGM(AudioClip clip, bool loop = true)
        {
            if (clip == null || bgmSource == null) return;

            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }

        /// <summary>
        /// Dừng hẳn bài nhạc nền.
        /// </summary>
        public void StopBGM()
        {
            if (bgmSource != null)
            {
                bgmSource.Stop();
            }
        }

        /// <summary>
        /// Tạm dừng nhạc nền (phù hợp khi mở menu Pause).
        /// </summary>
        public void PauseBGM()
        {
            if (bgmSource != null && bgmSource.isPlaying)
            {
                bgmSource.Pause();
            }
        }

        /// <summary>
        /// Tiếp tục phát nhạc nền đã tạm dừng (phù hợp khi ấn Resume).
        /// </summary>
        public void ResumeBGM()
        {
            if (bgmSource != null && !bgmSource.isPlaying && bgmSource.clip != null)
            {
                bgmSource.UnPause();
            }
        }

        #endregion

        #region SFX Playback APIs

        /// <summary>
        /// Phát một hiệu ứng âm thanh (SFX) dạng One-Shot.
        /// </summary>
        public void PlaySFX(AudioClip clip)
        {
            if (clip == null || sfxSource == null) return;

            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        /// <summary>
        /// Phát một hiệu ứng âm thanh với tuỳ chỉnh độ cao tần số âm (pitch).
        /// Hữu ích cho các hiệu ứng bấm phím hoặc tương tác lặp lại nhiều lần.
        /// </summary>
        public void PlaySFX(AudioClip clip, float pitch)
        {
            if (clip == null || sfxSource == null) return;

            float originalPitch = sfxSource.pitch;
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(clip, sfxVolume);
            sfxSource.pitch = originalPitch;
        }

        #endregion
    }
}

