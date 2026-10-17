using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Ui
{
    public class StarRatingUI : MonoBehaviour
    {
        [SerializeField] private Image[] starImages;
        [SerializeField] private Sprite starFilledSprite;
        [SerializeField] private Sprite starEmptySprite;

        public void SetStars(int starCount)
        {
            if (starImages == null) return;
            
            starCount = Mathf.Clamp(starCount, 0, starImages.Length);
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] != null)
                {
                    bool isFilled = i < starCount;

                    // Nếu có gán 2 Sprite khác nhau trong Inspector thì đổi Sprite
                    if (starFilledSprite != null && starEmptySprite != null && starFilledSprite != starEmptySprite)
                    {
                        starImages[i].sprite = isFilled ? starFilledSprite : starEmptySprite;
                    }

                    // Luôn đổi màu rõ ràng:
                    // Sao đạt: Vàng sáng (#FFD700)
                    // Sao bị trừ: Xám tối có độ trong suốt để phân biệt rõ ràng
                    starImages[i].color = isFilled 
                        ? new Color(0.95f, 0.85f, 0.1f, 1f) 
                        : new Color(0.35f, 0.35f, 0.35f, 0.3f);
                }
            }
        }
    }
}