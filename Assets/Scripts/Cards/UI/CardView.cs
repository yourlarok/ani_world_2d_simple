using AniWorld.Cards.Data;
using AniWorld.Cards.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AniWorld.Cards.UI
{
    public class CardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [Header("UI")]
        [SerializeField] private CardVisualStyle visualStyle;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image background;
        [SerializeField] private Image rarityFrame;
        [SerializeField] private Image cardTypeFrame;
        [SerializeField] private Image artwork;
        [SerializeField] private Image goldCostBadge;
        [SerializeField] private Image apCostBadge;
        [SerializeField] private Image disabledOverlay;
        [SerializeField] private Image playableGlow;
        [SerializeField] private Image selectedOverlay;
        [SerializeField] private Text nameText;
        [SerializeField] private Text goldCostText;
        [SerializeField] private Text apCostText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text blockReasonText;

        [Header("State Colors")]
        [SerializeField] private Color playableColor = Color.white;
        [SerializeField] private Color blockedColor = new Color(0.45f, 0.45f, 0.45f, 0.85f);
        [SerializeField] private Color affordableCostColor = Color.white;
        [SerializeField] private Color blockedCostColor = Color.red;

        private HandManager handManager;
        private CardDragController dragController;
        private RectTransform rectTransform;
        private Vector2 originalAnchoredPosition;
        private bool draggingCharacter;

        public int HandIndex { get; private set; }
        public GameCard Card { get; private set; }
        public CardPlayState PlayState { get; private set; }

        private void Awake()
        {
            rectTransform = transform as RectTransform;
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        public void Bind(HandManager handManager, CardDragController dragController, int handIndex, GameCard card, CardPlayState playState)
        {
            this.handManager = handManager;
            this.dragController = dragController;
            HandIndex = handIndex;
            Card = card;
            PlayState = playState;
            Refresh();
        }

        public void Refresh()
        {
            if (Card == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            SetText(nameText, Card.Name);
            SetText(goldCostText, Card.GoldCost.ToString());
            SetText(apCostText, Card.IsCharacter ? Card.APCost.ToString() : string.Empty);
            SetText(descriptionText, Card.IsCharacter ? Card.CharacterData.description : Card.FoodData.description);
            SetText(blockReasonText, PlayState.CanPlay ? string.Empty : PlayState.BlockReason.ToString());

            if (artwork != null)
            {
                artwork.sprite = Card.IsCharacter ? Card.CharacterData.artwork : Card.FoodData.artwork;
                artwork.enabled = artwork.sprite != null;
            }

            if (background != null)
            {
                background.color = PlayState.CanPlay ? playableColor : blockedColor;
                SetSpriteIfAvailable(background, visualStyle != null ? visualStyle.GetBackground(Card.Category) : null);
            }

            SetSpriteIfAvailable(rarityFrame, visualStyle != null ? visualStyle.GetRarityFrame(Card.Rarity) : null);
            SetSpriteIfAvailable(cardTypeFrame, visualStyle != null ? visualStyle.GetTypeFrame(Card.Category) : null);
            SetSpriteIfAvailable(goldCostBadge, visualStyle != null ? visualStyle.goldCostBadge : null);
            SetSpriteIfAvailable(apCostBadge, visualStyle != null ? visualStyle.apCostBadge : null);
            SetOverlay(disabledOverlay, visualStyle != null ? visualStyle.disabledOverlay : null, !PlayState.CanPlay);
            SetOverlay(playableGlow, visualStyle != null ? visualStyle.playableGlow : null, PlayState.CanPlay);
            SetOverlay(selectedOverlay, visualStyle != null ? visualStyle.selectedOverlay : null, false);

            if (goldCostText != null)
            {
                goldCostText.color = PlayState.CanAffordGold ? affordableCostColor : blockedCostColor;
            }

            if (apCostText != null)
            {
                apCostText.color = PlayState.CanAffordAP ? affordableCostColor : blockedCostColor;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = PlayState.CanPlay ? 1f : 0.55f;
                canvasGroup.blocksRaycasts = true;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Card != null && Card.IsFood && PlayState.CanPlay && handManager != null)
            {
                handManager.TryBuyFoodCard(HandIndex);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            draggingCharacter = false;
            if (Card == null || !Card.IsCharacter || dragController == null || !PlayState.CanPlay)
            {
                return;
            }

            draggingCharacter = dragController.BeginCharacterDrag(HandIndex);
            if (!draggingCharacter)
            {
                return;
            }

            if (rectTransform != null)
            {
                originalAnchoredPosition = rectTransform.anchoredPosition;
            }

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!draggingCharacter || rectTransform == null)
            {
                return;
            }

            rectTransform.anchoredPosition += eventData.delta;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!draggingCharacter)
            {
                return;
            }

            if (dragController != null)
            {
                dragController.EndCharacterDrag(eventData.position);
            }

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = originalAnchoredPosition;
            }

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
            }

            draggingCharacter = false;
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static void SetSpriteIfAvailable(Image image, Sprite sprite)
        {
            if (image == null || sprite == null)
            {
                return;
            }

            image.sprite = sprite;
            image.enabled = true;
        }

        private static void SetOverlay(Image image, Sprite sprite, bool visible)
        {
            if (image == null)
            {
                return;
            }

            if (sprite != null)
            {
                image.sprite = sprite;
            }

            image.enabled = visible && image.sprite != null;
        }
    }
}
