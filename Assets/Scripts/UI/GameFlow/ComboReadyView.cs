using AniWorld.Cards.Data;
using AniWorld.Combos;
using AniWorld.Events;
using AniWorld.Tokens;
using UnityEngine;
using UnityEngine.UI;

namespace AniWorld.UI.GameFlow
{
    public class ComboReadyView : MonoBehaviour
    {
        [SerializeField] private ComboTargetingController comboTargetingController;

        [Header("Art Slots")]
        [SerializeField] private Image panelFrame;
        [SerializeField] private Image comboIcon;
        [SerializeField] private Image costBadge;
        [SerializeField] private Image readyGlow;
        [SerializeField] private Image targetTypeIcon;

        [Header("Text")]
        [SerializeField] private Text comboNameText;
        [SerializeField] private Text comboDescriptionText;
        [SerializeField] private Text apCostText;
        [SerializeField] private Text targetTypeText;

        [Header("Actions")]
        [SerializeField] private Button fastEatButton;

        private GameToken token;
        private FoodComboData combo;

        private void Awake()
        {
            if (comboTargetingController == null)
            {
                comboTargetingController = FindObjectOfType<ComboTargetingController>();
            }
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<ComboReadyEvent>(OnComboReady);
            if (fastEatButton != null)
            {
                fastEatButton.onClick.AddListener(FastEat);
            }

            Refresh();
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<ComboReadyEvent>(OnComboReady);
            if (fastEatButton != null)
            {
                fastEatButton.onClick.RemoveListener(FastEat);
            }
        }

        public void Bind(GameToken readyToken, FoodComboData readyCombo)
        {
            token = readyToken;
            combo = readyCombo;
            Refresh();
        }

        public void Clear()
        {
            Bind(null, null);
        }

        public void FastEat()
        {
            if (token != null && comboTargetingController != null)
            {
                comboTargetingController.BeginCombo(token);
            }
        }

        private void OnComboReady(ComboReadyEvent gameEvent)
        {
            Bind(gameEvent.Token, gameEvent.Combo);
        }

        private void Refresh()
        {
            bool hasCombo = token != null && combo != null;
            gameObject.SetActive(hasCombo);
            if (!hasCombo)
            {
                return;
            }

            SetText(comboNameText, combo.comboName);
            SetText(comboDescriptionText, combo.description);
            SetText(apCostText, combo.apCost.ToString());
            SetText(targetTypeText, combo.targetType.ToString());
            SetImageEnabledIfSprite(panelFrame);
            SetImageEnabledIfSprite(comboIcon);
            SetImageEnabledIfSprite(costBadge);
            SetImageEnabledIfSprite(targetTypeIcon);

            if (readyGlow != null)
            {
                readyGlow.enabled = readyGlow.sprite != null;
            }

            if (fastEatButton != null)
            {
                fastEatButton.interactable = true;
            }
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
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
