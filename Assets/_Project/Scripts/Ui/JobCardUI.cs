using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using _Project.Scripts.Data;
using _Project.Scripts.Core;

namespace _Project.Scripts.Ui
{
    public class JobCardUI : MonoBehaviour
    {
        private static JobCardUI currentlyExpandedCard;

        [Header("2 Chế độ hiển thị")]
        [SerializeField] private GameObject collapsedView;
        [SerializeField] private GameObject expandedView;

        [Header("Thành phần Canvas Group (Khoá/Mờ thẻ)")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Nút tương tác (Click)")]
        [SerializeField] private Button btnCollapsed; 
        [SerializeField] private Button btnCloseExpand; 
        [SerializeField] private Button btnAccept; 
        [SerializeField] private TextMeshProUGUI txtAcceptButton; // Nhãn chữ trên nút Nhận đơn

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

        private JobData currentJob;
        private bool isExpanded = false;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

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

            if (txtAcceptButton == null && btnAccept != null)
            {
                txtAcceptButton = btnAccept.GetComponentInChildren<TextMeshProUGUI>();
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

        // Hàm nạp dữ liệu chính quy nhận trực tiếp đối tượng JobData từ JobBoardUI
        public void SetData(JobData jobData, float dayTimeRemaining)
        {
            currentJob = jobData;

            if (jobData != null)
            {
                string custName = string.IsNullOrEmpty(jobData.CustomerName) ? "Khách hàng" : jobData.CustomerName;
                int items = (jobData.ItemQueue != null) ? jobData.ItemQueue.Count : 0;
                float tMin = jobData.TimeMin;
                float tMax = jobData.TimeMax;

                // Tính tiền thưởng ước tính từ tổng giá trị các item trong JobData
                int rew = 0;
                if (jobData.ItemQueue != null)
                {
                    foreach (var item in jobData.ItemQueue)
                    {
                        if (item != null) rew += item.Value;
                    }
                }

                // Kiểm tra luật BR-02: Khoá thẻ nếu quỹ thời gian còn lại < TimeMin của đơn
                bool locked = dayTimeRemaining < tMin;

                ApplyData(custName, items, tMin, tMax, rew, locked);
            }
            else
            {
                ApplyData(customerName, itemCount, timeMin, timeMax, reward, dayTimeRemaining < timeMin);
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
            // UpdateLockState(isLocked);
        }
        
        ///summary
        ///Đề xuất: Cập nhật trạng thái khoá/mở thẻ dựa trên quỹ thời gian còn lại trong ngày theo luật BR-02 (nếu còn < TimeMin thì khoá thẻ)
        ///summary
       
        // Cập nhật trạng thái khoá/mở thẻ dựa trên quỹ thời gian còn lại trong ngày
        //public void UpdateLockState(bool locked)
        // {
        //     isLocked = locked;

        //     if (canvasGroup != null)
        //     {
        //         if (isLocked)
        //         {
        //             canvasGroup.alpha = 0.5f;
        //             canvasGroup.interactable = false; // Vô hiệu hoá tương tác nút con (btnAccept)
        //             canvasGroup.blocksRaycasts = true; // Vẫn cho phép raycast để mở/xem Accordion

        //             if (txtAcceptButton != null)
        //                 txtAcceptButton.text = "Không đủ thời gian";
        //         }
        //         else
        //         {
        //             canvasGroup.alpha = 1.0f;
        //             canvasGroup.interactable = true;

        //             if (txtAcceptButton != null)
        //                 txtAcceptButton.text = "Nhận đơn";
        //         }
        //     }
        //     else
        //     {
        //         if (btnAccept != null) btnAccept.interactable = !isLocked;
        //     }
        // }

        //Cập nhật trực tiếp trạng thái khoá dựa theo quỹ thời gian ngày còn lại
        // public void UpdateLockStateWithTime(float dayTimeRemaining)
        // {
        //     bool locked = (currentJob != null) ? (dayTimeRemaining < currentJob.TimeMin) : (dayTimeRemaining < timeMin);
        //     UpdateLockState(locked);
        // }

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
            RebuildLayout();
        }

        // Thu gọn thẻ này lại
        public void Collapse()
        {
            if (currentlyExpandedCard == this)
            {
                currentlyExpandedCard = null;
            }
            SetExpandedState(false);
            RebuildLayout();
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

        // private void OnAcceptClicked()
        // {
        //     Debug.Log($"<color=green>[JobCard] ĐÃ NHẬN ĐƠN:</color> <b>{customerName}</b> | Thưởng: <b>${reward}</b> | Khung giờ: <b>[{timeMin}s - {timeMax}s]</b>");
        // }

        //Ép parent layout tính toán lại kích thước ScrollView ngay lập tức
        private void RebuildLayout()
        {
            if (transform.parent is RectTransform parentRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            }
        }

        // Xử lý sự kiện bấm nút "Nhận đơn" (btnAccept)
        private void OnAcceptClicked()
        {
            if (isLocked) return;

            Debug.Log($"<color=green>[JobCard] ĐÃ NHẬN ĐƠN:</color> <b>{customerName}</b> | Thưởng: <b>${reward}</b> | Khung giờ: <b>[{timeMin}s - {timeMax}s]</b>");

            // Phát sự kiện nhận việc qua EventBus
            if (currentJob != null)
            {
                EventBus.Publish(new GameEvent.JobSelectedEvent(currentJob));
            }
        }
    }
}
