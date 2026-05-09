using System;
using System.Collections.Generic;
using AniWorld.Board;
using AniWorld.Cards.Data;
using UnityEngine;

namespace AniWorld.Tokens
{
    public class TokenSpawner : MonoBehaviour
    {
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private Transform unitsRoot;
        [SerializeField] private GameToken defaultTokenPrefab;
        [SerializeField] private TeamType defaultTeam = TeamType.Player;
        [SerializeField] private DeploymentZone deploymentZone;

        private readonly List<GameToken> activeTokens = new List<GameToken>();

        public event Action<GameToken> TokenSpawned;
        public event Action<GameToken> TokenRemoved;

        public IReadOnlyList<GameToken> ActiveTokens => activeTokens;
        public DeploymentZone DeploymentZone => deploymentZone;

        private void Awake()
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }
        }

        public bool IsInDeployZone(Vector2Int position)
        {
            return deploymentZone == null || deploymentZone.Contains(position);
        }

        public bool CanSpawn(CharacterCardData card, Vector2Int position)
        {
            return card != null &&
                boardManager != null &&
                boardManager.CanPlaceUnit(position) &&
                IsInDeployZone(position);
        }

        public GameToken Spawn(CharacterCardData card, Vector2Int position)
        {
            return Spawn(card, defaultTeam, position);
        }

        public GameToken Spawn(CharacterCardData card, TeamType team, Vector2Int position)
        {
            if (!CanSpawn(card, position))
            {
                return null;
            }

            GameToken prefab = ResolvePrefab(card);
            GameToken token;
            if (prefab != null)
            {
                token = Instantiate(prefab, unitsRoot);
            }
            else
            {
                GameObject tokenObject = new GameObject(card.cardName);
                if (unitsRoot != null)
                {
                    tokenObject.transform.SetParent(unitsRoot);
                }

                token = tokenObject.AddComponent<GameToken>();
            }

            token.Initialize(card, team, position);
            token.transform.position = boardManager.GetCellWorldCenter(position);

            if (!boardManager.SetOccupiedUnit(position, token))
            {
                Destroy(token.gameObject);
                return null;
            }

            activeTokens.Add(token);
            TokenSpawned?.Invoke(token);
            return token;
        }

        public void Remove(GameToken token)
        {
            if (token == null)
            {
                return;
            }

            if (activeTokens.Remove(token))
            {
                boardManager.ClearOccupiedUnit(token.BoardPosition);
                TokenRemoved?.Invoke(token);
            }

            Destroy(token.gameObject);
        }

        public GameToken GetTokenAt(Vector2Int position)
        {
            if (boardManager == null || !boardManager.TryGetCell(position, out BoardCell cell))
            {
                return null;
            }

            return cell.OccupiedUnit as GameToken;
        }

        private GameToken ResolvePrefab(CharacterCardData card)
        {
            if (card != null && card.tokenPrefab != null)
            {
                GameToken cardPrefab = card.tokenPrefab.GetComponent<GameToken>();
                if (cardPrefab != null)
                {
                    return cardPrefab;
                }
            }

            return defaultTokenPrefab;
        }
    }
}
