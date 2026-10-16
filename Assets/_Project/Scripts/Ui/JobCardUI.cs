using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Project.Scripts.Ui
{
    public class JobCardUI : MonoBehaviour
    {
        private static JobCardUI currentlyExpandedCard;

        [Header("2 Chế độ hiển thị")]
        [SerializeField] private GameObject collapsedView;
        [SerializeField] private GameObject expandedView;

        [Header("Nút tương tác (Click)")]
        [SerializeField] private Button btnCollapsed; 
        [SerializeField] private Button btnCloseExpand; 
        [SerializeField] private Button btnAccept; 

        [Header("Thành phần - Collapsed View (Hình 2)")]
        [SerializeField] private Image imgCollapsedAvatar;
        [SerializeField] private TextMeshProUGUI txtCollapsedTitle;
        [SerializeField] private TextMeshProUGUI txtCollapsedTime;
        [SerializeField] private TextMeshProUGUI txtCollapsedReward;

        [Header("Thành phần - Expanded View (Hình 1)")]
        [SerializeField] private Image imgExpandedAvatar;
        [SerializeField] private TextMeshProUGUI txtDialogue;
        [SerializeField] private Image imgHouse;
        [SerializeField] private TextMeshProUGUI txtMaxReward;
        [SerializeField] private TextMeshProUGUI txtTimeWindow;
        [SerializeField] private TextMeshProUGUI txtItemCount;

        [Header("Dữ liệu Test thử (Chỉnh trực tiếp trên Inspector)")]
        [SerializeField] private bool testInPlayMode = true;
        [SerializeField] private string customerName = "Khách A";
        [SerializeField] private int itemCount = 5;
        [SerializeField] private float timeMin = 50f;
        [SerializeField] private float timeMax = 60f;
        [SerializeField] private int reward = 120;
        [SerializeField] private bool isLocked = false;

        private bool isExpanded = false;

        private void Awake()
        {
            if (btnCollapsed != null)
            {
                btnCollapsed.onClick.AddListener(OnCollapsedClicked);
            }

            if (btnCloseExpand != null)
            {
                btnCloseExpand.onClick.AddListener(Collapse);
            }

            if (btnAccept != null)
            {
                btnAccept.onClick.AddListener(OnAcceptClicked);
            }
        }

        private void Start()
        {
            if (testInPlayMode)
            {
                ApplyData(customerName, itemCount, timeMin, timeMax, reward, isLocked);
            }

            SetExpandedState(false);
        }

        private void OnDestroy()
        {
            if (currentlyExpandedCard == this)
            {
                currentlyExpandedCard = null;
            }
        }

        // Nạp dữ liệu vào cả 2 View của thẻ
        public void ApplyData(string custName, int items, float tMin, float tMax, int rew, bool locked)
        {
            customerName = custName;
            itemCount = items;
            timeMin = tMin;
            timeMax = tMax;
            reward = rew;
            isLocked = locked;

            // 1. Nạp Collapsed View
            if (txtCollapsedTitle != null) txtCollapsedTitle.text = $"{customerName} - {itemCount} món";
            if (txtCollapsedTime != null) txtCollapsedTime.text = $"[{timeMin:0}s - {timeMax:0}s]";
            if (txtCollapsedReward != null) txtCollapsedReward.text = $"${reward}";

            // 2. Nạp Expanded View
            if (txtDialogue != null) txtDialogue.text = $"\"Nhờ bạn chuyển giúp {customerName} nhé!\"";
            if (txtMaxReward != null) txtMaxReward.text = $"Thưởng tối đa: ${reward}";
            if (txtTimeWindow != null) txtTimeWindow.text = $"[{timeMin:0}s - {timeMax:0}s]";
            if (txtItemCount != null) txtItemCount.text = $"Số lượng item: {itemCount}";

            // 3. Khóa tương tác nếu bị khóa
            if (btnCollapsed != null) btnCollapsed.interactable = !isLocked;
            if (btnAccept != null) btnAccept.interactable = !isLocked;
        }

        // Mở rộng thẻ này (TỰ ĐỘNG ĐÓNG THẺ KHÁC NẾU ĐANG MỞ)
        public void Expand()
        {
            if (isLocked) return;

            // Nếu đang có thẻ khác mở -> bắt thẻ đó đóng lại!
            if (currentlyExpandedCard != null && currentlyExpandedCard != this)
            {
                currentlyExpandedCard.Collapse();
            }

            currentlyExpandedCard = this;
            SetExpandedState(true);
        }

        // Thu gọn thẻ này lại
        public void Collapse()
        {
            if (currentlyExpandedCard == this)
            {
                currentlyExpandedCard = null;
            }
            SetExpandedState(false);
        }

        // Xử lý click thanh nhỏ: Nếu đang mở thì đóng, đang đóng thì mở
        private void OnCollapsedClicked()
        {
            if (isExpanded)
            {
                Collapse();
            }
            else
            {
                Expand();
            }
        }

        // Thiết lập trạng thái mở rộng
        private void SetExpandedState(bool expanded)
        {
            isExpanded = expanded;
            if (collapsedView != null) collapsedView.SetActive(!isExpanded);
            if (expandedView != null) expandedView.SetActive(isExpanded);
        }

        private void OnAcceptClicked()
        {
            Debug.Log($"<color=green>[JobCard] ĐÃ NHẬN ĐƠN:</color> <b>{customerName}</b> | Thưởng: <b>${reward}</b> | Khung giờ: <b>[{timeMin}s - {timeMax}s]</b>");
        }
    }
}
