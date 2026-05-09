using System.Collections.Generic;
using AniWorld.Cards.UI;
using UnityEngine;
using UnityEngine.UI;

namespace AniWorld.Food
{
    public class FoodBarView : MonoBehaviour
    {
        [SerializeField] private FoodBar foodBar;
        [SerializeField] private CardVisualStyle visualStyle;
        [SerializeField] private Image barBackground;
        [SerializeField] private List<Image> slotImages = new List<Image>();
        [SerializeField] private List<Image> slotFrames = new List<Image>();
        [SerializeField] private List<Image> slotRarityFrames = new List<Image>();
        [SerializeField] private List<Image> slotEmptyIcons = new List<Image>();
        [SerializeField] private List<Image> slotDurationOverlays = new List<Image>();
        [SerializeField] private List<Text> slotLabels = new List<Text>();

        private void Awake()
        {
            if (foodBar == null)
            {
                foodBar = FindObjectOfType<FoodBar>();
            }
        }

        private void OnEnable()
        {
            if (foodBar != null)
            {
                foodBar.FoodBarChanged += Refresh;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (foodBar != null)
            {
                foodBar.FoodBarChanged -= Refresh;
            }
        }

        public void Refresh()
        {
            SetImageEnabledIfSprite(barBackground);

            for (int i = 0; i < slotImages.Count; i++)
            {
                bool hasFood = foodBar != null && i < foodBar.Count && foodBar.Slots[i] != null;
                if (slotImages[i] != null)
                {
                    slotImages[i].sprite = hasFood ? foodBar.Slots[i].artwork : null;
                    slotImages[i].enabled = hasFood && slotImages[i].sprite != null;
                }

                if (i < slotLabels.Count && slotLabels[i] != null)
                {
                    slotLabels[i].text = hasFood ? foodBar.Slots[i].cardName : string.Empty;
                }

                SetListImageEnabled(slotFrames, i, true);
                SetListImageEnabled(slotEmptyIcons, i, !hasFood);
                SetListImageEnabled(slotDurationOverlays, i, hasFood);

                if (i < slotRarityFrames.Count && slotRarityFrames[i] != null)
                {
                    Sprite rarityFrame = hasFood && visualStyle != null
                        ? visualStyle.GetRarityFrame(foodBar.Slots[i].rarity)
                        : null;
                    if (rarityFrame != null)
                    {
                        slotRarityFrames[i].sprite = rarityFrame;
                    }

                    slotRarityFrames[i].enabled = hasFood && slotRarityFrames[i].sprite != null;
                }
            }
        }

        private static void SetListImageEnabled(List<Image> images, int index, bool enabled)
        {
            if (index < images.Count && images[index] != null)
            {
                images[index].enabled = enabled && images[index].sprite != null;
            }
        }

        private static void SetImageEnabledIfSprite(Image image)
        {
            if (image != null)
            {
                image.enabled = image.sprite != null;
            }
        }
    }
}
