using System.Collections.Generic;
using AniWorld.Cards.Data;
using AniWorld.Combos;
using AniWorld.Food;
using AniWorld.Tokens;
using UnityEngine;
using UnityEngine.UI;

namespace AniWorld.UI.GameFlow
{
    public class CharacterPanelView : MonoBehaviour
    {
        [Header("Token")]
        [SerializeField] private GameToken currentToken;
        [SerializeField] private ComboTargetingController comboTargetingController;
        [SerializeField] private ClassTokenVisualDatabase classVisualDatabase;

        [Header("Art Slots")]
        [SerializeField] private Image panelFrame;
        [SerializeField] private Image portrait;
        [SerializeField] private Image rarityFrame;
        [SerializeField] private Image raceIcon;
        [SerializeField] private Image classIcon;
        [SerializeField] private Image hpIcon;
        [SerializeField] private Image patkIcon;
        [SerializeField] private Image matkIcon;
        [SerializeField] private Image pdefIcon;
        [SerializeField] private Image mdefIcon;
        [SerializeField] private Image movIcon;
        [SerializeField] private Image rageIcon;
        [SerializeField] private Image comboReadyIndicator;

        [Header("Text")]
        [SerializeField] private Text nameText;
        [SerializeField] private Text raceText;
        [SerializeField] private Text classText;
        [SerializeField] private Text rarityText;
        [SerializeField] private Text hpText;
        [SerializeField] private Text patkText;
        [SerializeField] private Text matkText;
        [SerializeField] private Text pdefText;
        [SerializeField] private Text mdefText;
        [SerializeField] private Text movText;
        [SerializeField] private Text rageText;
        [SerializeField] private Text comboNameText;

        [Header("Food Slots")]
        [SerializeField] private List<Image> foodSlotFrames = new List<Image>();
        [SerializeField] private List<Image> foodSlotIcons = new List<Image>();
        [SerializeField] private List<Text> foodSlotTurnsTexts = new List<Text>();

        [Header("Actions")]
        [SerializeField] private Button fastEatButton;

        private void Awake()
        {
            if (comboTargetingController == null)
            {
                comboTargetingController = FindObjectOfType<ComboTargetingController>();
            }
        }

        private void OnEnable()
        {
            if (fastEatButton != null)
            {
                fastEatButton.onClick.AddListener(FastEat);
            }

            SubscribeToken();
            Refresh();
        }

        private void OnDisable()
        {
            if (fastEatButton != null)
            {
                fastEatButton.onClick.RemoveListener(FastEat);
            }

            UnsubscribeToken();
        }

        public void Bind(GameToken token)
        {
            UnsubscribeToken();
            currentToken = token;
            SubscribeToken();
            Refresh();
        }

        public void Clear()
        {
            Bind(null);
        }

        public void FastEat()
        {
            if (currentToken != null && comboTargetingController != null)
            {
                comboTargetingController.BeginCombo(currentToken);
            }
        }

        public void Refresh()
        {
            bool hasToken = currentToken != null && currentToken.CardData != null;
            gameObject.SetActive(hasToken);
            if (!hasToken)
            {
                return;
            }

            CharacterCardData card = currentToken.CardData;
            SetText(nameText, card.cardName);
            SetText(raceText, card.race.ToString());
            SetText(classText, card.classType.ToString());
            SetText(rarityText, card.rarity.ToString());
            SetText(hpText, $"{currentToken.CurrentHP}/{currentToken.MaxHP}");
            SetText(patkText, currentToken.PATK.ToString());
            SetText(matkText, currentToken.MATK.ToString());
            SetText(pdefText, currentToken.PDEF.ToString());
            SetText(mdefText, currentToken.MDEF.ToString());
            SetText(movText, currentToken.MOV.ToString());
            SetText(rageText, currentToken.Rage.ToString());

            SetSprite(portrait, card.artwork);
            SetSprite(classIcon, classVisualDatabase != null ? classVisualDatabase.GetClassIcon(card.classType) : null);
            SetImageEnabledIfSprite(panelFrame);
            SetImageEnabledIfSprite(rarityFrame);
            SetImageEnabledIfSprite(raceIcon);
            SetImageEnabledIfSprite(hpIcon);
            SetImageEnabledIfSprite(patkIcon);
            SetImageEnabledIfSprite(matkIcon);
            SetImageEnabledIfSprite(pdefIcon);
            SetImageEnabledIfSprite(mdefIcon);
            SetImageEnabledIfSprite(movIcon);
            SetImageEnabledIfSprite(rageIcon);

            FoodComboData combo = currentToken.FindReadyCombo();
            SetText(comboNameText, combo != null ? combo.comboName : string.Empty);
            if (comboReadyIndicator != null)
            {
                comboReadyIndicator.enabled = combo != null && comboReadyIndicator.sprite != null;
            }

            if (fastEatButton != null)
            {
                fastEatButton.interactable = combo != null;
            }

            RefreshFoodSlots();
        }

        private void RefreshFoodSlots()
        {
            for (int i = 0; i < foodSlotFrames.Count; i++)
            {
                SetImageEnabledIfSprite(foodSlotFrames[i]);
            }

            for (int i = 0; i < foodSlotIcons.Count; i++)
            {
                FoodSlot slot = i < currentToken.FoodSlots.Count ? currentToken.FoodSlots[i] : null;
                bool hasFood = slot != null && !slot.IsEmpty;
                SetSprite(foodSlotIcons[i], hasFood ? slot.Food.artwork : null);

                if (i < foodSlotTurnsTexts.Count)
                {
                    SetText(foodSlotTurnsTexts[i], hasFood ? slot.RemainingTurns.ToString() : string.Empty);
                }
            }
        }

        private void SubscribeToken()
        {
            if (currentToken != null)
            {
                currentToken.StatsChanged += OnTokenChanged;
                currentToken.ComboReady += OnComboReady;
            }
        }

        private void UnsubscribeToken()
        {
            if (currentToken != null)
            {
                currentToken.StatsChanged -= OnTokenChanged;
                currentToken.ComboReady -= OnComboReady;
            }
        }

        private void OnTokenChanged(GameToken token)
        {
            Refresh();
        }

        private void OnComboReady(GameToken token, FoodComboData combo)
        {
            Refresh();
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static void SetSprite(Image image, Sprite sprite)
        {
            if (image != null)
            {
                image.sprite = sprite;
                image.enabled = sprite != null;
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
