using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Ui
{
    /// <summary>
    /// Script phát chuỗi ảnh / GIF động trên giao diện UI (Image)
    /// Dùng Time.unscaledTime để chạy mượt mà ngay cả khi Time.timeScale = 0 (lúc chuyển cảnh / loading)
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class GifUIPlayer : MonoBehaviour
    {
        [Header("UI Image Target")]
        [SerializeField] private Image targetImage;

        [Header("Animation Frames")]
        [Tooltip("Kéo danh sách các frame ảnh PNG đã tách từ GIF vào đây")]
        [SerializeField] private Sprite[] frames;

        [Header("Cài đặt tốc độ")]
        [Tooltip("Số khung hình trên giây (FPS), mặc định 12 FPS")]
        [SerializeField] private float frameRate = 12f;

        [SerializeField] private bool loop = true;
        [SerializeField] private bool playOnAwake = true;

        private int currentFrameIndex = 0;
        private float timer = 0f;
        private bool isPlaying = true;

        private void Awake()
        {
            if (targetImage == null)
            {
                targetImage = GetComponent<Image>();
            }

            isPlaying = playOnAwake;
        }

        private void Update()
        {
            if (!isPlaying || frames == null || frames.Length <= 1) return;

            timer += Time.unscaledDeltaTime;
            float timePerFrame = 1f / Mathf.Max(1f, frameRate);

            if (timer >= timePerFrame)
            {
                timer -= timePerFrame;
                currentFrameIndex++;

                if (currentFrameIndex >= frames.Length)
                {
                    if (loop)
                    {
                        currentFrameIndex = 0;
                    }
                    else
                    {
                        currentFrameIndex = frames.Length - 1;
                        isPlaying = false;
                        return;
                    }
                }

                if (targetImage != null && frames[currentFrameIndex] != null)
                {
                    targetImage.sprite = frames[currentFrameIndex];
                }
            }
        }

        public void Play()
        {
            isPlaying = true;
        }

        public void Pause()
        {
            isPlaying = false;
        }

        public void Restart()
        {
            currentFrameIndex = 0;
            timer = 0f;
            isPlaying = true;
            if (targetImage != null && frames != null && frames.Length > 0)
            {
                targetImage.sprite = frames[0];
            }
        }

        public void SetFrames(Sprite[] newFrames)
        {
            frames = newFrames;
            Restart();
        }
    }
}
