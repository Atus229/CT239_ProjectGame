// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// namespace _Project.Scripts.Ui
// {
//     public class TutorialUI : MonoBehaviour
//     {
//         [Header("Nút điều hướng & Đóng Popup")]
//         [SerializeField] private Button btnClose;       // Nút "X" góc trên bên phải
//         [SerializeField] private Button btnPrev;        // Nút "< Trước"
//         [SerializeField] private Button btnNext;        // Nút "Tiếp >"
//         [SerializeField] private TextMeshProUGUI txtNextLabel;

//         [Header("Thành phần hiển thị nội dung trang")]
//         [SerializeField] private Image imgIllustration;
//         [SerializeField] private TextMeshProUGUI txtTitle;
//         [SerializeField] private TextMeshProUGUI txtDescription;
//         [SerializeField] private TextMeshProUGUI txtPageCounter; // VD: "1 / 4"

//         [Header("Chỉ báo chấm tròn (Pagination Dots - Tuỳ chọn)")]
//         [SerializeField] private List<Image> pageDots = new List<Image>();
//         [SerializeField] private Color activeDotColor = new Color(0.95f, 0.45f, 0.22f, 1f); // Màu cam chủ đạo
//         [SerializeField] private Color inactiveDotColor = new Color(0.6f, 0.65f, 0.7f, 0.5f);

//         [Header("Dữ liệu 4 bước hướng dẫn")]
//         [SerializeField] private List<TutorialPageData> pages = new List<TutorialPageData>();

//         private int currentPageIndex = 0;
//         private bool isInitialized = false;

//         private void Awake()
//         {
//             EnsureInitialized();
//         }

//         private void OnEnable()
//         {
//             EnsureInitialized();
//             ShowPage(0);
//         }

//         private void Update()
//         {
//             // Hỗ trợ phím mũi tên Trái/Phải và ESC khi test trên PC
//             if (!gameObject.activeInHierarchy) return;

//             if (Input.GetKeyDown(KeyCode.RightArrow))
//             {
//                 NextPage();
//             }
//             else if (Input.GetKeyDown(KeyCode.LeftArrow))
//             {
//                 PrevPage();
//             }
//             else if (Input.GetKeyDown(KeyCode.Escape))
//             {
//                 ClosePopup();
//             }
//         }

//         private void EnsureInitialized()
//         {
//             if (isInitialized) return;

//             if (btnClose != null)
//                 btnClose.onClick.AddListener(ClosePopup);

//             if (btnPrev != null)
//                 btnPrev.onClick.AddListener(PrevPage);

//             if (btnNext != null)
//                 btnNext.onClick.AddListener(NextPage);

//             isInitialized = true;
//         }

//         /// <summary>
//         /// Mở Popup Hướng dẫn và luôn bắt đầu từ Trang 1
//         /// </summary>
//         public void OpenPopup()
//         {
//             gameObject.SetActive(true);
//             ShowPage(0);
//         }

//         /// <summary>
//         /// Đóng Popup Hướng dẫn và quay lại màn hình MainMenu
//         /// </summary>
//         public void ClosePopup()
//         {
//             gameObject.SetActive(false);
//         }

//         public void NextPage()
//         {
//             if (pages == null || pages.Count == 0) return;

//             if (currentPageIndex < pages.Count - 1)
//             {
//                 ShowPage(currentPageIndex + 1);
//             }
//             else
//             {
//                 // Ở trang cuối (4/4), bấm "Xong" sẽ đóng Popup về Menu
//                 ClosePopup();
//             }
//         }

//         public void PrevPage()
//         {
//             if (pages == null || pages.Count == 0) return;

//             if (currentPageIndex > 0)
//             {
//                 ShowPage(currentPageIndex - 1);
//             }
//         }

//         public void ShowPage(int index)
//         {
//             if (pages == null || pages.Count == 0) return;

//             currentPageIndex = Mathf.Clamp(index, 0, pages.Count - 1);
//             TutorialPageData data = pages[currentPageIndex];

//             // 1. Cập nhật tiêu đề, mô tả & hình minh họa
//             if (txtTitle != null)
//                 txtTitle.text = data.stepTitle;

//             if (txtDescription != null)
//                 txtDescription.text = data.stepDescription;

//             if (imgIllustration != null)
//             {
//                 imgIllustration.sprite = data.stepIllustration;
//                 imgIllustration.enabled = (data.stepIllustration != null);
//             }

//             // 2. Cập nhật bộ đếm trang (1 / 4)
//             if (txtPageCounter != null)
//                 txtPageCounter.text = $"{currentPageIndex + 1} / {pages.Count}";

//             // 3. Cập nhật trạng thái nút Trước / Tiếp
//             if (btnPrev != null)
//                 btnPrev.interactable = (currentPageIndex > 0);

//             if (txtNextLabel != null)
//                 txtNextLabel.text = (currentPageIndex == pages.Count - 1) ? "XONG" : "TIẾP >";

//             // 4. Cập nhật màu các chấm tròn trang
//             for (int i = 0; i < pageDots.Count; i++)
//             {
//                 if (pageDots[i] != null)
//                 {
//                     pageDots[i].color = (i == currentPageIndex) ? activeDotColor : inactiveDotColor;
//                 }
//             }
//         }
//     }
//     [Serializable]
//     public class TutorialPageData
//     {
//         public string stepTitle;
//         [TextArea(3, 6)]
//         public string stepDescription;
//         public Sprite stepIllustration;
//     }
// }

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace _Project.Scripts.Ui
{
    public class TutorialUI : MonoBehaviour
    {
        [Header("Nút điều hướng & Đóng Popup")]
        [SerializeField] private Button btnClose;       // Nút "X" góc trên bên phải
        [SerializeField] private Button btnPrev;        // Nút "< Trước"
        [SerializeField] private Button btnNext;        // Nút "Tiếp >"
        [SerializeField] private TextMeshProUGUI txtNextLabel;

        [Header("Thành phần hiển thị nội dung trang")]
        [SerializeField] private Image imgIllustration;
        [SerializeField] private TextMeshProUGUI txtTitle;
        [SerializeField] private TextMeshProUGUI txtDescription;
        [SerializeField] private TextMeshProUGUI txtPageCounter; // VD: "1 / 4"

        [Header("Chỉ báo chấm tròn (Pagination Dots - Tuỳ chọn)")]
        [SerializeField] private List<Image> pageDots = new List<Image>();
        [SerializeField] private Color activeDotColor = new Color(0.95f, 0.45f, 0.22f, 1f);
        [SerializeField] private Color inactiveDotColor = new Color(0.6f, 0.65f, 0.7f, 0.5f);

        [Header("Dữ liệu 4 bước hướng dẫn")]
        [SerializeField] private List<TutorialPageData> pages = new List<TutorialPageData>();

        private int currentPageIndex = 0;
        private bool isInitialized = false;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnEnable()
        {
            EnsureInitialized();
            ShowPage(0);
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy) return;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
                    NextPage();
                else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
                    PrevPage();
                else if (Keyboard.current.escapeKey.wasPressedThisFrame)
                    ClosePopup();
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.RightArrow))
                NextPage();
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                PrevPage();
            else if (Input.GetKeyDown(KeyCode.Escape))
                ClosePopup();
#endif
        }

        private void EnsureInitialized()
        {
            if (isInitialized) return;

            if (btnClose != null) btnClose.onClick.AddListener(ClosePopup);
            if (btnPrev != null) btnPrev.onClick.AddListener(PrevPage);
            if (btnNext != null) btnNext.onClick.AddListener(NextPage);

            isInitialized = true;
        }

        public void OpenPopup()
        {
            gameObject.SetActive(true);
            ShowPage(0);
        }

        public void ClosePopup()
        {
            gameObject.SetActive(false);
        }

        public void NextPage()
        {
            if (pages == null || pages.Count == 0) return;

            if (currentPageIndex < pages.Count - 1)
                ShowPage(currentPageIndex + 1);
            else
                ClosePopup();
        }

        public void PrevPage()
        {
            if (pages == null || pages.Count == 0) return;

            if (currentPageIndex > 0)
                ShowPage(currentPageIndex - 1);
        }

        public void ShowPage(int index)
        {
            if (pages == null || pages.Count == 0) return;

            currentPageIndex = Mathf.Clamp(index, 0, pages.Count - 1);
            TutorialPageData data = pages[currentPageIndex];

            if (txtTitle != null) txtTitle.text = data.stepTitle;
            if (txtDescription != null) txtDescription.text = data.stepDescription;

            if (imgIllustration != null)
            {
                imgIllustration.sprite = data.stepIllustration;
                imgIllustration.preserveAspect = true;
                imgIllustration.enabled = (data.stepIllustration != null);
            }

            if (txtPageCounter != null)
                txtPageCounter.text = $"{currentPageIndex + 1} / {pages.Count}";

            if (btnPrev != null)
                btnPrev.interactable = (currentPageIndex > 0);

            if (txtNextLabel != null)
                txtNextLabel.text = (currentPageIndex == pages.Count - 1) ? "XONG" : "TIẾP >";

            for (int i = 0; i < pageDots.Count; i++)
            {
                if (pageDots[i] != null)
                    pageDots[i].color = (i == currentPageIndex) ? activeDotColor : inactiveDotColor;
            }
        }
    }

    [Serializable]
    public class TutorialPageData
    {
        public string stepTitle;
        [TextArea(3, 6)]
        public string stepDescription;
        public Sprite stepIllustration;
    }
}