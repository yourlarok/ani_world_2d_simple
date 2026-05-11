using UnityEngine;

namespace AniWorld.Board
{
    public static class ScreenCoordinateUtility
    {
        public static bool TryScreenToWorldOnBoardPlane(Camera camera, Vector2 screenPosition, out Vector3 worldPosition)
        {
            worldPosition = Vector3.zero;
            if (camera == null || !IsScreenPositionInsideCamera(camera, screenPosition))
            {
                return false;
            }

            float distanceToBoardPlane = Mathf.Abs(camera.transform.position.z);
            Vector3 screen = new Vector3(screenPosition.x, screenPosition.y, distanceToBoardPlane);
            worldPosition = camera.ScreenToWorldPoint(screen);
            worldPosition.z = 0f;
            return IsFinite(worldPosition);
        }

        public static bool IsScreenPositionInsideCamera(Camera camera, Vector2 screenPosition)
        {
            if (camera == null || !camera.isActiveAndEnabled)
            {
                return false;
            }

            Rect pixelRect = camera.pixelRect;
            return screenPosition.x >= pixelRect.xMin &&
                screenPosition.x <= pixelRect.xMax &&
                screenPosition.y >= pixelRect.yMin &&
                screenPosition.y <= pixelRect.yMax;
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
