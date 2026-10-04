using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Project.Scripts.Ui
{
    public class TimeGaugeUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("The background container of the gauge to get the total width")]
        [SerializeField]
        private RectTransform gaugeBackground;

        [Tooltip("The Image that fills up. Image Type must be Filled -> Horizontal")] [SerializeField]
        private Image fillImage;

        [Tooltip("The green zone representing [Tmin, Tmax]")] [SerializeField]
        private RectTransform targetZone;

        [Tooltip("Marker for the minimum time required (Tmin)")] [SerializeField]
        private RectTransform tMinMarker;

        [Tooltip("Marker for the maximum time allowed (Tmax)")] [SerializeField]
        private RectTransform tMaxMarker;

        [Tooltip("Text to display current time vs target time")] [SerializeField]
        private TextMeshProUGUI timeText;

        [Header("Color Settings")] [SerializeField]
        private Color underMinColor = new Color(0.8f, 0.4f, 0.4f); // Xám / Đỏ nhạt

        [SerializeField] private Color validColor = new Color(0.4f, 0.8f, 0.4f); // Xanh lá
        [SerializeField] private Color warningColor = new Color(1f, 0.6f, 0f); // Cam (Bắt đầu trễ)
        [SerializeField] private Color overMaxColor = new Color(0.9f, 0.2f, 0.2f); // Đỏ đậm (Trễ nhiều)

        private float currentTotalTime = 0f;
        private float tMin = 0f;
        private float tMax = 0f;
        private float gaugeMaxLimit = 0f;
        private float tMinMarkerWidth;
        private float tMaxMarkerWidth;

        private void Start()
        {
            if (tMinMarker != null) tMinMarkerWidth = tMinMarker.sizeDelta.x;
            if (tMaxMarker != null) tMaxMarkerWidth = tMaxMarker.sizeDelta.x;
        }

        private void OnEnable()
        {
            // Đăng ký event thay đổi thời gian ở đây (Ví dụ)
            // JobEconomyManager.OnTotalTimeChanged += UpdateGauge;
        }

        private void OnDisable()
        {
            // Huỷ đăng ký event
            // JobEconomyManager.OnTotalTimeChanged -= UpdateGauge;
        }

        /// <summary>
        /// Khởi tạo thanh Gauge khi bắt đầu một đơn hàng mới
        /// </summary>
        public void SetupTargetWindow(float min, float max, float limit = -1f)
        {
            tMin = min;
            tMax = max;

            if (tMinMarkerWidth <= 0 && tMinMarker != null) tMinMarkerWidth = tMinMarker.sizeDelta.x;
            if (tMaxMarkerWidth <= 0 && tMaxMarker != null) tMaxMarkerWidth = tMaxMarker.sizeDelta.x;

            // Nếu không truyền limit, mặc định giới hạn tối đa của thanh gauge là Tmax * 1.5
            gaugeMaxLimit = limit > 0 ? limit : Mathf.Max(tMax * 1.5f, 1f);

            // Cập nhật vị trí và kích thước Target Zone
            UpdateMarkersAndZone();

            // Reset thời gian hiện tại về 0
            UpdateGauge(0f);
        }
        
        private void UpdateMarkersAndZone()
        {
            if (gaugeMaxLimit <= 0 || gaugeBackground == null) return;

            // Tính toán tỷ lệ phần trăm
            float minPercent = tMin / gaugeMaxLimit;
            float maxPercent = tMax / gaugeMaxLimit;

            if (tMinMarker != null)
            {
                tMinMarker.anchorMin = new Vector2(minPercent, 0f);
                tMinMarker.anchorMax = new Vector2(minPercent, 1f);
                tMinMarker.offsetMin = new Vector2(-tMinMarkerWidth / 2f, -5f);
                tMinMarker.offsetMax = new Vector2(tMinMarkerWidth / 2f, 5f);
            }

            if (tMaxMarker != null)
            {
                tMaxMarker.anchorMin = new Vector2(maxPercent, 0f);
                tMaxMarker.anchorMax = new Vector2(maxPercent, 1f);
                tMaxMarker.offsetMin = new Vector2(-tMaxMarkerWidth / 2f, -5f);
                tMaxMarker.offsetMax = new Vector2(tMaxMarkerWidth / 2f, 5f);
            }

            if (targetZone != null)
            {
                targetZone.anchorMin = new Vector2(minPercent, 0f);
                targetZone.anchorMax = new Vector2(maxPercent, 1f);
                targetZone.offsetMin = new Vector2(0f, 9.5f);
                targetZone.offsetMax = new Vector2(0f, -9.5f);
            }
        }
        
        public void UpdateGauge(float newTotalTime)
        {
            currentTotalTime = newTotalTime;

            if (gaugeMaxLimit <= 0) return;

            // Tính toán phần trăm lấp đầy
            float fillAmount = Mathf.Clamp01(currentTotalTime / gaugeMaxLimit);

            if (fillImage != null)
            {
                fillImage.fillAmount = fillAmount;

                // Cập nhật màu sắc dựa trên mốc thời gian
                if (currentTotalTime < tMin)
                {
                    fillImage.color = underMinColor;
                }
                else if (currentTotalTime >= tMin && currentTotalTime <= tMax)
                {
                    fillImage.color = validColor;
                }
                else
                {
                    // Chuyển từ Xanh -> Cam -> Đỏ
                    float overTime = currentTotalTime - tMax;
                    float buffer = 1f;

                    if (overTime < buffer && buffer > 0)
                    {
                        float t = overTime / buffer;
                        fillImage.color = Color.Lerp(validColor, warningColor, t);
                    }
                    else
                    {
                        float maxOverTime = Mathf.Max(gaugeMaxLimit - tMax - buffer, 0.01f);
                        float lerpFactor = Mathf.Clamp01((overTime - buffer) / maxOverTime);
                        fillImage.color = Color.Lerp(warningColor, overMaxColor, lerpFactor);
                    }
                }
            }

            if (timeText != null)
            {
                timeText.text = $"{currentTotalTime:F0}s / [{tMin:F0}s - {tMax:F0}s]";

                // Đổi màu text để cảnh báo khi trễ hẹn
                if (currentTotalTime > tMax)
                {
                    timeText.color = overMaxColor;
                }
                else
                {
                    timeText.color = Color.white;
                }
            }
        }
    }
}