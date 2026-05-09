using System.Collections.Generic;
using AniWorld.Cards.Runtime;
using UnityEngine;

namespace AniWorld.Cards.UI
{
    public class HandView : MonoBehaviour
    {
        [SerializeField] private HandManager handManager;
        [SerializeField] private CardPlayValidator validator;
        [SerializeField] private CardDragController dragController;
        [SerializeField] private CardView cardViewPrefab;
        [SerializeField] private Transform cardContainer;

        private readonly List<CardView> cardViews = new List<CardView>();

        private void Awake()
        {
            if (handManager == null)
            {
                handManager = FindObjectOfType<HandManager>();
            }

            if (validator == null)
            {
                validator = FindObjectOfType<CardPlayValidator>();
            }

            if (dragController == null)
            {
                dragController = FindObjectOfType<CardDragController>();
            }

            if (cardContainer == null)
            {
                cardContainer = transform;
            }
        }

        private void OnEnable()
        {
            if (handManager != null)
            {
                handManager.HandChanged += Refresh;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (handManager != null)
            {
                handManager.HandChanged -= Refresh;
            }
        }

        public void Refresh()
        {
            if (handManager == null || cardViewPrefab == null)
            {
                return;
            }

            EnsureViewCount(handManager.HandCount);

            for (int i = 0; i < cardViews.Count; i++)
            {
                if (i >= handManager.HandCount)
                {
                    cardViews[i].gameObject.SetActive(false);
                    continue;
                }

                GameCard card = handManager.GetCard(i);
                CardPlayState state = validator != null
                    ? validator.EvaluateCard(card)
                    : CardPlayState.Blocked(CardPlayBlockReason.MissingSystem, false, false, false);

                cardViews[i].Bind(handManager, dragController, i, card, state);
            }
        }

        private void EnsureViewCount(int count)
        {
            while (cardViews.Count < count)
            {
                CardView view = Instantiate(cardViewPrefab, cardContainer);
                cardViews.Add(view);
            }
        }
    }
}
