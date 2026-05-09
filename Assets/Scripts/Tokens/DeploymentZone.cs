using System.Collections.Generic;
using UnityEngine;

namespace AniWorld.Tokens
{
    public class DeploymentZone : MonoBehaviour
    {
        [SerializeField] private bool useRect = true;
        [SerializeField] private Vector2Int rectOrigin = Vector2Int.zero;
        [SerializeField] private int width = 3;
        [SerializeField] private int height = 3;
        [SerializeField] private List<Vector2Int> explicitCells = new List<Vector2Int>();

        public bool Contains(Vector2Int position)
        {
            if (useRect)
            {
                return position.x >= rectOrigin.x &&
                    position.y >= rectOrigin.y &&
                    position.x < rectOrigin.x + width &&
                    position.y < rectOrigin.y + height;
            }

            return explicitCells.Contains(position);
        }
    }
}
