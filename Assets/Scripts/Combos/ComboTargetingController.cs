using System.Collections.Generic;
using AniWorld.Board;
using AniWorld.Cards.Data;
using AniWorld.Cards.Runtime;
using AniWorld.Tokens;
using UnityEngine;

namespace AniWorld.Combos
{
    public class ComboTargetingController : MonoBehaviour
    {
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private BoardHighlighter highlighter;
        [SerializeField] private ComboResolver resolver;
        [SerializeField] private TokenSpawner tokenSpawner;

        private GameToken activeCaster;
        private FoodComboData activeCombo;

        public BoardInteractionMode Mode { get; private set; } = BoardInteractionMode.Inspect;

        private void Awake()
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            if (highlighter == null)
            {
                highlighter = FindObjectOfType<BoardHighlighter>();
            }

            if (resolver == null)
            {
                resolver = FindObjectOfType<ComboResolver>();
            }

            if (tokenSpawner == null)
            {
                tokenSpawner = FindObjectOfType<TokenSpawner>();
            }
        }

        public bool BeginCombo(GameToken caster)
        {
            if (caster == null)
            {
                return false;
            }

            FoodComboData combo = caster.FindReadyCombo();
            if (combo == null)
            {
                return false;
            }

            if (!combo.RequiresTarget)
            {
                return caster.TryFastEat(resolver, caster.BoardPosition, caster);
            }

            activeCaster = caster;
            activeCombo = combo;
            Mode = BoardInteractionMode.SelectingComboTarget;
            ShowTargetHighlights();
            return true;
        }

        public bool ConfirmTarget(Vector2Int targetPosition)
        {
            if (Mode != BoardInteractionMode.SelectingComboTarget || activeCaster == null || activeCombo == null)
            {
                Cancel();
                return false;
            }

            GameToken targetToken = tokenSpawner != null ? tokenSpawner.GetTokenAt(targetPosition) : null;
            bool resolved = activeCaster.TryFastEat(resolver, targetPosition, targetToken);
            Cancel();
            return resolved;
        }

        public void Cancel()
        {
            activeCaster = null;
            activeCombo = null;
            Mode = BoardInteractionMode.Inspect;
            highlighter?.ClearHighlights();
        }

        private void ShowTargetHighlights()
        {
            if (boardManager == null || highlighter == null || activeCaster == null || activeCombo == null)
            {
                return;
            }

            List<Vector2Int> positions = new List<Vector2Int>();
            foreach (BoardCell cell in boardManager.Cells)
            {
                if (cell == null)
                {
                    continue;
                }

                int distance = Mathf.Abs(cell.Position.x - activeCaster.BoardPosition.x) +
                    Mathf.Abs(cell.Position.y - activeCaster.BoardPosition.y);
                if (activeCombo.range > 0 && distance > activeCombo.range)
                {
                    continue;
                }

                GameToken token = tokenSpawner != null ? tokenSpawner.GetTokenAt(cell.Position) : null;
                if (IsValidHighlightedTarget(cell.Position, token))
                {
                    positions.Add(cell.Position);
                }
            }

            highlighter.ShowAttackRange(positions);
        }

        private bool IsValidHighlightedTarget(Vector2Int position, GameToken targetToken)
        {
            switch (activeCombo.targetType)
            {
                case ComboTargetType.FriendlyUnit:
                    return targetToken != null && targetToken.Team == activeCaster.Team;
                case ComboTargetType.EnemyUnit:
                    return targetToken != null && targetToken.Team != activeCaster.Team;
                case ComboTargetType.AnyUnit:
                    return targetToken != null;
                case ComboTargetType.EmptyCell:
                    return boardManager.CanPlaceUnit(position);
                case ComboTargetType.AnyCell:
                case ComboTargetType.Area:
                case ComboTargetType.Terrain:
                    return boardManager.HasCell(position);
                default:
                    return true;
            }
        }
    }
}
