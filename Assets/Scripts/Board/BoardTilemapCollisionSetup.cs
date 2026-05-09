using UnityEngine;
using UnityEngine.Tilemaps;

namespace AniWorld.Board
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Tilemap))]
    public class BoardTilemapCollisionSetup : MonoBehaviour
    {
        [SerializeField] private bool configureOnAwake = true;
        [SerializeField] private bool useCompositeCollider = true;

        private void Reset()
        {
            Configure();
        }

        private void Awake()
        {
            if (configureOnAwake)
            {
                Configure();
            }
        }

        public void Configure()
        {
            TilemapCollider2D tilemapCollider = GetComponent<TilemapCollider2D>();
            if (tilemapCollider == null)
            {
                tilemapCollider = gameObject.AddComponent<TilemapCollider2D>();
            }

            tilemapCollider.usedByComposite = useCompositeCollider;

            if (!useCompositeCollider)
            {
                return;
            }

            Rigidbody2D rigidbody2d = GetComponent<Rigidbody2D>();
            if (rigidbody2d == null)
            {
                rigidbody2d = gameObject.AddComponent<Rigidbody2D>();
            }

            rigidbody2d.bodyType = RigidbodyType2D.Static;

            if (GetComponent<CompositeCollider2D>() == null)
            {
                gameObject.AddComponent<CompositeCollider2D>();
            }
        }
    }
}
