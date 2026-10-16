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
                    starImages[i].sprite = (i < starCount) ? starFilledSprite : starEmptySprite;
                }
            }
        }
    }
}