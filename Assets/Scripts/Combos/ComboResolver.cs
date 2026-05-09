using AniWorld.Board;
using AniWorld.Cards.Data;
using AniWorld.Cards.Runtime;
using AniWorld.Tokens;
using UnityEngine;

namespace AniWorld.Combos
{
    public class ComboResolver : MonoBehaviour
    {
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private APManager apManager;
        [SerializeField] private TokenSpawner tokenSpawner;

        private void Awake()
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            if (apManager == null)
            {
                apManager = FindObjectOfType<APManager>();
            }

            if (tokenSpawner == null)
            {
                tokenSpawner = FindObjectOfType<TokenSpawner>();
            }
        }

        public bool CanResolve(ComboCastRequest request)
        {
            if (request.Caster == null || request.Combo == null || apManager == null || !apManager.CanSpend(request.Combo.apCost))
            {
                return false;
            }

            if (!request.Combo.RequiresTarget)
            {
                return true;
            }

            return IsValidTarget(request);
        }

        public bool TryResolve(ComboCastRequest request)
        {
            if (!CanResolve(request))
            {
                return false;
            }

            if (!apManager.Spend(request.Combo.apCost))
            {
                return false;
            }

            ExecuteEffect(request);
            return true;
        }

        private bool IsValidTarget(ComboCastRequest request)
        {
            switch (request.Combo.targetType)
            {
                case ComboTargetType.FriendlyUnit:
                    return request.TargetToken != null && request.TargetToken.Team == request.Caster.Team;
                case ComboTargetType.EnemyUnit:
                    return request.TargetToken != null && request.TargetToken.Team != request.Caster.Team;
                case ComboTargetType.AnyUnit:
                    return request.TargetToken != null;
                case ComboTargetType.EmptyCell:
                    return boardManager != null && boardManager.CanPlaceUnit(request.TargetPosition);
                case ComboTargetType.AnyCell:
                case ComboTargetType.Area:
                case ComboTargetType.Terrain:
                    return boardManager != null && boardManager.HasCell(request.TargetPosition);
                default:
                    return true;
            }
        }

        private void ExecuteEffect(ComboCastRequest request)
        {
            switch (request.Combo.effectType)
            {
                case ComboEffectType.DamageSingle:
                    ApplyDamage(request.TargetToken, request.Combo.damage);
                    break;
                case ComboEffectType.DamageAOE:
                    ApplyAreaDamage(request);
                    break;
                case ComboEffectType.Heal:
                    ApplyHeal(request);
                    break;
                case ComboEffectType.BuffAll:
                    ApplyBuffAll(request);
                    break;
                case ComboEffectType.DebuffEnemy:
                    ApplyDamage(request.TargetToken, request.Combo.damage);
                    break;
                case ComboEffectType.Summon:
                    ApplySummon(request);
                    break;
                case ComboEffectType.TerrainChange:
                    ApplyTerrainChange(request);
                    break;
                case ComboEffectType.Special:
                    Debug.Log($"Special combo '{request.Combo.comboName}' needs a custom executor.");
                    break;
            }
        }

        private static void ApplyDamage(GameToken target, int amount)
        {
            if (target != null)
            {
                target.TakeDamage(amount);
            }
        }

        private void ApplyAreaDamage(ComboCastRequest request)
        {
            ForEachCellInRadius(request.TargetPosition, request.Combo.aoeRadius, position =>
            {
                GameToken target = tokenSpawner != null ? tokenSpawner.GetTokenAt(position) : null;
                if (target != null && target.Team != request.Caster.Team)
                {
                    target.TakeDamage(request.Combo.damage);
                }
            });
        }

        private void ApplyHeal(ComboCastRequest request)
        {
            GameToken target = request.TargetToken != null ? request.TargetToken : request.Caster;
            target.Heal(request.Combo.healAmount);
        }

        private void ApplyBuffAll(ComboCastRequest request)
        {
            if (tokenSpawner == null)
            {
                request.Caster.ApplyBuff(
                    request.Combo.buffPATK,
                    request.Combo.buffMATK,
                    request.Combo.buffPDEF,
                    request.Combo.buffMDEF,
                    request.Combo.buffMOV,
                    request.Combo.buffRage);
                return;
            }

            foreach (GameToken token in tokenSpawner.ActiveTokens)
            {
                if (token != null && token.Team == request.Caster.Team)
                {
                    token.ApplyBuff(
                        request.Combo.buffPATK,
                        request.Combo.buffMATK,
                        request.Combo.buffPDEF,
                        request.Combo.buffMDEF,
                        request.Combo.buffMOV,
                        request.Combo.buffRage);
                }
            }
        }

        private void ApplySummon(ComboCastRequest request)
        {
            if (tokenSpawner != null && request.Combo.summonCharacter != null)
            {
                tokenSpawner.Spawn(request.Combo.summonCharacter, request.Caster.Team, request.TargetPosition);
            }
        }

        private void ApplyTerrainChange(ComboCastRequest request)
        {
            ForEachCellInRadius(request.TargetPosition, request.Combo.aoeRadius, position =>
            {
                if (boardManager != null && boardManager.HasCell(position))
                {
                    boardManager.SetTerrain(position, request.Combo.terrainChange);
                }
            });
        }

        private void ForEachCellInRadius(Vector2Int center, int radius, System.Action<Vector2Int> action)
        {
            if (action == null)
            {
                return;
            }

            int clampedRadius = Mathf.Max(0, radius);
            for (int x = -clampedRadius; x <= clampedRadius; x++)
            {
                for (int y = -clampedRadius; y <= clampedRadius; y++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) > clampedRadius)
                    {
                        continue;
                    }

                    action(new Vector2Int(center.x + x, center.y + y));
                }
            }
        }
    }
}
