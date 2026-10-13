using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Project.Scripts.Ui
{
    public class ProgressBarUI : MonoBehaviour
    {
        [SerializeField] private Image imgFill;
        [SerializeField] private TextMeshProUGUI txtLabel;
        [SerializeField] private TextMeshProUGUI txtPercent;

        public void SetProgress(float current, float max, string label = "Cargo bed capacity")
        {
            float fillAmount = max > 0 ? Mathf.Clamp01(current / max) : 0f;
            
            if (imgFill != null) imgFill.fillAmount = fillAmount;
            if (txtLabel != null) txtLabel.text = label;
            if (txtPercent != null) txtPercent.text = $"{Mathf.RoundToInt(fillAmount * 100f)}%";
        }
    }
}