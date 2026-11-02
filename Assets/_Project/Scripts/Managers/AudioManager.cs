using UnityEngine;

namespace _Project.Scripts.Managers
{
    [DefaultExecutionOrder(-90)]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private const string PrefKeyBgmVolume = "AudioManager_BGM_Volume";
        private const string PrefKeySfxVolume = "AudioManager_SFX_Volume";
        private const float DefaultVolume = 0.75f;

        [Header("BGM Clips")]
        public AudioClip bgmGameplay;

        [Header("Gameplay SFX Clips")]
        public AudioClip sfxPlaceSuccess;
        public AudioClip sfxPlaceFail;
        public AudioClip sfxButtonClick;
        public AudioClip sfxRewardCoins;
        public AudioClip sfxTruckHorn;

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

        private void Start()
        {
            if (bgmGameplay != null && bgmSource != null && !bgmSource.isPlaying)
            {
                PlayBGM(bgmGameplay, true);
            }
        }

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

        private void LoadSavedVolumes()
        {
            bgmVolume = PlayerPrefs.GetFloat(PrefKeyBgmVolume, DefaultVolume);
            sfxVolume = PlayerPrefs.GetFloat(PrefKeySfxVolume, DefaultVolume);

            ApplyVolumes();
        }

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

        public void PlayBGM(AudioClip clip, bool loop = true)
        {
            if (clip == null || bgmSource == null) return;

            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }

        public void StopBGM()
        {
            if (bgmSource != null) bgmSource.Stop();
        }

        public void PauseBGM()
        {
            if (bgmSource != null && bgmSource.isPlaying) bgmSource.Pause();
        }

        public void ResumeBGM()
        {
            if (bgmSource != null && !bgmSource.isPlaying && bgmSource.clip != null) bgmSource.UnPause();
        }

        #endregion

        #region SFX Playback APIs (Generic)

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        public void PlaySFX(AudioClip clip, float pitch)
        {
            if (clip == null || sfxSource == null) return;

            float originalPitch = sfxSource.pitch;
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(clip, sfxVolume);
            sfxSource.pitch = originalPitch;
        }

        #endregion

        #region Gameplay Semantic SFX Helpers

        public void PlayPlaceSuccess()
        {
            PlaySFX(sfxPlaceSuccess);
        }

        public void PlayPlaceFail()
        {
            PlaySFX(sfxPlaceFail);
        }
         
        public void PlayButtonClick()
        {
            PlaySFX(sfxButtonClick);
        }
        
        public void PlayRewardCoins()
        {
            PlaySFX(sfxRewardCoins);
        }

        public void PlayTruckHorn()
        {
            PlaySFX(sfxTruckHorn);
        }

        #endregion
    }
}

