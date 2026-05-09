using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AniWorld.Food
{
    public class FoodBarView : MonoBehaviour
    {
        [SerializeField] private FoodBar foodBar;
        [SerializeField] private List<Image> slotImages = new List<Image>();
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
            }
        }
    }
}
