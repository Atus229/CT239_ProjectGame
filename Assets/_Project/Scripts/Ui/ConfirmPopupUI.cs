using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Ui
{
    public class ConfirmPopupUI : MonoBehaviour
    {
        public static ConfirmPopupUI Instance { get; private set; }

        [SerializeField] private Button buttonYes;
        [SerializeField] private Button buttonNo;
        [SerializeField] private CanvasGroup canvasGroup;

        private Action _onConfirmAction;

        private void Awake()
        {
            Instance = this;
            SetupComponents();
            Hide();
        }

        private void SetupComponents()
        {
            // 1. Tự động tìm ButtonYes và ButtonNo
            if (buttonYes == null)
            {
                var yesTrans = transform.Find("PopupCard/ButtonYes");
                if (yesTrans != null) buttonYes = yesTrans.GetComponent<Button>();
            }

            if (buttonNo == null)
            {
                var noTrans = transform.Find("PopupCard/ButtonNo");
                if (noTrans != null) buttonNo = noTrans.GetComponent<Button>();
            }

            // Fallback tìm kiếm trong toàn bộ con nếu cấu trúc có thay đổi
            if (buttonYes == null || buttonNo == null)
            {
                var allButtons = GetComponentsInChildren<Button>(true);
                foreach (var b in allButtons)
                {
                    string bName = b.gameObject.name.ToLower();
                    if (buttonYes == null && (bName.Contains("yes") || bName.Contains("co") || bName.Contains("xacnhan") || bName.Contains("ok")))
                    {
                        buttonYes = b;
                    }
                    else if (buttonNo == null && (bName.Contains("no") || bName.Contains("khong") || bName.Contains("cancel") || bName.Contains("huy")))
                    {
                        buttonNo = b;
                    }
                }
            }

            // Gán sự kiện click cho các nút
            if (buttonYes != null)
            {
                buttonYes.onClick.RemoveAllListeners();
                buttonYes.onClick.AddListener(OnYesClicked);
            }

            if (buttonNo != null)
            {
                buttonNo.onClick.RemoveAllListeners();
                buttonNo.onClick.AddListener(OnNoClicked);
            }

            // 2. CanvasGroup
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // 3. Đảm bảo căn chỉnh RectTransform vừa khít toàn màn hình ở chính giữa
            RectTransform rt = GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = Vector2.zero;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.localScale = Vector3.one;
                rt.localPosition = Vector3.zero;
            }
        }

        private void OnYesClicked()
        {
            var action = _onConfirmAction;
            Hide();
            action?.Invoke();
        }

        private void OnNoClicked()
        {
            Hide();
        }

        public void Show(Action onConfirm)
        {
            _onConfirmAction = onConfirm;

            // Đảm bảo được gắn dưới 1 Canvas hợp lệ
            if (transform.parent == null || GetComponentInParent<Canvas>() == null)
            {
                Canvas mainCanvas = FindMainCanvas();
                if (mainCanvas != null)
                {
                    transform.SetParent(mainCanvas.transform, false);
                }
            }

            SetupComponents();

            gameObject.SetActive(true);
            transform.SetAsLastSibling(); // Luôn nổi lên trên cùng màn hình

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        public void Hide()
        {
            _onConfirmAction = null;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Hiển thị ConfirmPopup từ bất kỳ đâu.
        /// Tự động tìm Popup trong Scene hoặc Instantiate từ Resources nếu chưa có.
        /// </summary>
        public static void ShowConfirm(Action onConfirm)
        {
            if (Instance == null)
            {
                Instance = FindObjectOfType<ConfirmPopupUI>(true);
            }

            if (Instance == null)
            {
                // Tìm kiếm GameObject tên ConfirmPopup trong Scene (kể cả đang bị ẩn)
                var allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
                foreach (var t in allTransforms)
                {
                    if (t.gameObject.scene.isLoaded && t.gameObject.name == "ConfirmPopup")
                    {
                        Instance = t.GetComponent<ConfirmPopupUI>() ?? t.gameObject.AddComponent<ConfirmPopupUI>();
                        break;
                    }
                }
            }

            // Nếu vẫn chưa có trong Scene, nạp và tạo từ Resources
            if (Instance == null)
            {
                var prefab = Resources.Load<GameObject>("ConfirmPopup");
                if (prefab != null)
                {
                    Canvas canvas = FindMainCanvas();
                    Transform parent = canvas != null ? canvas.transform : null;
                    var spawned = Instantiate(prefab, parent);
                    spawned.name = "ConfirmPopup";
                    Instance = spawned.GetComponent<ConfirmPopupUI>() ?? spawned.AddComponent<ConfirmPopupUI>();
                }
            }

            if (Instance != null)
            {
                Instance.Show(onConfirm);
            }
            else
            {
                Debug.LogWarning("[ConfirmPopupUI] Không tìm thấy popup hoặc prefab ConfirmPopup! Thực hiện hành động trực tiếp.");
                onConfirm?.Invoke();
            }
        }

        private static Canvas FindMainCanvas()
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (var c in canvases)
            {
                if (c.isRootCanvas && c.enabled && c.gameObject.activeInHierarchy)
                {
                    return c;
                }
            }

            return canvases.Length > 0 ? canvases[0] : null;
        }
    }
}
