using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using _Project.Scripts.Core;
using _Project.Scripts.Data;

namespace _Project.Scripts.Ui
{
    /// <summary>
    /// Màn hình Loading chuyển cảnh mượt mà:
    /// 1. Khi chuyển Scene giữa MainMenu và PackingScene
    /// 2. Khi chọn nhận đơn hàng mới từ Bảng công việc (JobBoard)
    /// 3. Hỗ trợ cả Slider hoặc Image Fill (TimeGaugeUI)
    /// 4. Hỗ trợ Icon / Gif Xe tải di chuyển theo thanh Loading kèm hiệu ứng nhấp nhô sống động
    /// </summary>
    public class LoadingScreenUI : MonoBehaviour
    {
        public static LoadingScreenUI Instance { get; private set; }

        [Header("UI Component References")]
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("Thanh tiến trình (Hỗ trợ cả Slider hoặc Image Fill của TimeGauge)")]
        [Tooltip("Nếu dùng Slider thông thường")]
        [SerializeField] private Slider progressSlider;
        [Tooltip("Nếu dùng Image Fill (như đối tượng con 'Fill' của TimeGaugeUI)")]
        [SerializeField] private Image fillImage;
        [Tooltip("Khung thanh đo độ dài (Kéo GaugeBackground hoặc TimeGaugeUI vào đây để đo đường đi của xe tải)")]
        [SerializeField] private RectTransform progressBarRect;

        [Header("UI Text References")]
        [SerializeField] private TextMeshProUGUI txtProgress;
        [SerializeField] private TextMeshProUGUI txtTip;

        [Header("Truck Icon (Xe tải chạy theo thanh Loading)")]
        [Tooltip("Kéo RectTransform của icon xe tải vào đây để xe chạy dọc theo thanh tiến trình")]
        [SerializeField] private RectTransform truckIcon;
        [Tooltip("Độ cao nhấp nhô của xe tải khi chạy")]
        [SerializeField] private float truckBobbingAmount = 3.5f;
        [Tooltip("Tốc độ lắc lư/nhấp nhô của xe tải")]
        [SerializeField] private float truckBobbingSpeed = 14f;

        [Header("Cài đặt")]
        [Tooltip("Bật hiệu ứng chuyển cảnh khi chọn nhận đơn hàng mới (mặc định tắt để vào đơn tức thì không phải đợi)")]
        [SerializeField] private bool enableJobTransition = false;

        [Tooltip("Thời gian hiển thị tối thiểu khi đổi Scene (giây)")]
        [SerializeField] private float minLoadingTime = 1.2f;

        [Tooltip("Thời gian chuyển cảnh khi chọn đơn hàng (giây)")]
        [SerializeField] private float jobTransitionDuration = 0.85f;

        private float initialTruckY = 0f;
        private bool hasRecordedInitialY = false;

        private static readonly string[] GameplayTips = new string[]
        {
            "💡 Mẹo: Nhấn phím R khi đang kéo món đồ để xoay chiều!",
            "💡 Mẹo: Đừng bao giờ đè đồ nặng lên hàng dễ vỡ nhé!",
            "💡 Mẹo: Hoàn thành đơn hàng trước thời hạn để nhận đủ 5 sao uy tín!",
            "💡 Mẹo: Mua nâng cấp mở rộng thùng xe trong Cửa hàng để chở được nhiều đồ hơn!",
            "💡 Mẹo: Nhấn phím ESC để tạm dừng trò chơi bất kỳ lúc nào!",
            "💡 Mẹo: Đạt đủ số sao để mở khóa các Đơn hàng VIP với tiền thưởng cực khủng!"
        };

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            // Tự động tìm kiếm thành phần nếu chưa kéo vào Inspector
            AutoDetectProgressComponents();

            if (truckIcon != null && !hasRecordedInitialY)
            {
                initialTruckY = truckIcon.anchoredPosition.y;
                hasRecordedInitialY = true;
            }

            if (progressSlider != null)
            {
                progressSlider.onValueChanged.AddListener(OnSliderValueChanged);
            }

            SetVisible(false);
        }

        private void AutoDetectProgressComponents()
        {
            if (progressSlider == null && fillImage == null)
            {
                progressSlider = GetComponentInChildren<Slider>(true);
                if (progressSlider == null)
                {
                    // Tìm Image con có tên "Fill"
                    var images = GetComponentsInChildren<Image>(true);
                    foreach (var img in images)
                    {
                        if (img.gameObject.name.Equals("Fill", System.StringComparison.OrdinalIgnoreCase))
                        {
                            fillImage = img;
                            break;
                        }
                    }
                }
            }

            if (progressBarRect == null)
            {
                if (progressSlider != null)
                {
                    progressBarRect = progressSlider.GetComponent<RectTransform>();
                }
                else if (fillImage != null)
                {
                    progressBarRect = fillImage.transform.parent != null
                        ? fillImage.transform.parent.GetComponent<RectTransform>()
                        : fillImage.rectTransform;
                }
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameEvent.JobSelectedEvent>(OnJobSelected);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameEvent.JobSelectedEvent>(OnJobSelected);
        }

        private void OnDestroy()
        {
            if (progressSlider != null)
            {
                progressSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnSliderValueChanged(float value)
        {
            UpdateTruckPosition(value);
        }

        /// <summary>
        /// Cập nhật vị trí X của xe tải tương ứng với tiến trình loading (0.0 -> 1.0)
        /// </summary>
        public void UpdateTruckPosition(float progress)
        {
            if (truckIcon == null) return;

            RectTransform targetBarRect = progressBarRect;
            if (targetBarRect == null)
            {
                if (progressSlider != null) targetBarRect = progressSlider.GetComponent<RectTransform>();
                else if (fillImage != null) targetBarRect = fillImage.rectTransform;
            }
            if (targetBarRect == null) return;

            if (!hasRecordedInitialY)
            {
                initialTruckY = truckIcon.anchoredPosition.y;
                hasRecordedInitialY = true;
            }

            float width = targetBarRect.rect.width;
            float startX = -width * 0.5f;
            float targetX = startX + (width * Mathf.Clamp01(progress));

            // Hiệu ứng xe nhấp nhô nhẹ như đang chạy trên đường
            float bobbing = (progress > 0.02f && progress < 0.98f)
                ? Mathf.Sin(Time.unscaledTime * truckBobbingSpeed) * truckBobbingAmount
                : 0f;

            truckIcon.anchoredPosition = new Vector2(targetX, initialTruckY + bobbing);
        }

        /// <summary>
        /// Tự động kích hoạt hiệu ứng Loading xe tải di chuyển khi người chơi nhận đơn hàng mới
        /// </summary>
        private void OnJobSelected(GameEvent.JobSelectedEvent evt)
        {
            if (enableJobTransition && evt.SelectedJob != null)
            {
                StopAllCoroutines();
                StartCoroutine(JobTransitionRoutine(evt.SelectedJob));
            }
        }

        private IEnumerator JobTransitionRoutine(JobData job)
        {
            string customer = !string.IsNullOrEmpty(job.CustomerName) ? job.CustomerName : "Khách hàng";

            if (txtTip != null && GameplayTips.Length > 0)
            {
                txtTip.text = GameplayTips[Random.Range(0, GameplayTips.Length)];
            }

            if (txtProgress != null) txtProgress.text = $"Đang di chuyển đến: {customer}... 0%";
            SetProgress(0f);

            SetVisible(true);

            // 1. Fade In (0.2s)
            yield return StartCoroutine(FadeRoutine(0f, 1f, 0.2f));

            // 2. Giả lập xe chạy đến nơi (jobTransitionDuration)
            float elapsed = 0f;
            while (elapsed < jobTransitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / jobTransitionDuration);
                SetProgress(progress);
                if (txtProgress != null) txtProgress.text = $"Đang di chuyển đến: {customer}... {(int)(progress * 100)}%";
                yield return null;
            }

            SetProgress(1f);
            if (txtProgress != null) txtProgress.text = "Đã đến nơi! Bắt đầu dọn nhà!";
            yield return new WaitForSecondsRealtime(0.2f);

            // 3. Fade Out (0.25s)
            yield return StartCoroutine(FadeRoutine(1f, 0f, 0.25f));
            SetVisible(false);
        }

        /// <summary>
        /// Gọi chuyển Scene kèm màn hình Loading bất đồng bộ
        /// </summary>
        public static void LoadScene(string sceneName)
        {
            if (Instance != null)
            {
                Instance.StopAllCoroutines();
                Instance.StartCoroutine(Instance.LoadSceneRoutine(sceneName));
            }
            else
            {
                // Fallback nếu Scene chưa có LoadingScreenUI
                SceneManager.LoadScene(sceneName);
            }
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            // 1. Hiển thị Loading Panel và Random 1 Tip
            if (txtTip != null && GameplayTips.Length > 0)
            {
                txtTip.text = GameplayTips[Random.Range(0, GameplayTips.Length)];
            }

            SetProgress(0f);
            if (txtProgress != null) txtProgress.text = "Đang chuẩn bị chuyến xe... 0%";

            SetVisible(true);

            // 2. Fade in
            yield return StartCoroutine(FadeRoutine(0f, 1f, 0.25f));

            // 3. Tải Scene ngầm
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            float elapsedTime = 0f;

            while (!op.isDone)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float realProgress = Mathf.Clamp01(op.progress / 0.9f);
                float fakeTimeProgress = Mathf.Clamp01(elapsedTime / minLoadingTime);
                float displayProgress = Mathf.Min(realProgress, fakeTimeProgress);

                SetProgress(displayProgress);
                if (txtProgress != null) txtProgress.text = $"Đang tải... {(int)(displayProgress * 100)}%";

                // Khi nạp xong và đã đủ thời gian tối thiểu
                if (op.progress >= 0.9f && elapsedTime >= minLoadingTime)
                {
                    SetProgress(1f);
                    if (txtProgress != null) txtProgress.text = "Sẵn sàng lên đường! 100%";
                    yield return new WaitForSecondsRealtime(0.2f);
                    op.allowSceneActivation = true;
                }

                yield return null;
            }

            // 4. Fade out ẩn Loading Panel
            yield return StartCoroutine(FadeRoutine(1f, 0f, 0.3f));
            SetVisible(false);
        }

        private void SetProgress(float val)
        {
            if (progressSlider != null)
            {
                progressSlider.value = val;
            }
            if (fillImage != null)
            {
                fillImage.fillAmount = val;
            }
            UpdateTruckPosition(val);
        }

        private IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration)
        {
            if (canvasGroup == null) yield break;

            float time = 0f;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, time / duration);
                yield return null;
            }
            canvasGroup.alpha = toAlpha;
        }

        private void SetVisible(bool isVisible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = isVisible ? 1f : 0f;
                canvasGroup.blocksRaycasts = isVisible;
                canvasGroup.interactable = isVisible;
            }
            else
            {
                gameObject.SetActive(isVisible);
            }
        }
    }
}
