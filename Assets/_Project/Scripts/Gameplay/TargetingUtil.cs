using UnityEngine;

namespace Synaptik.Game
{
    public static class TargetingUtil
    {
        private const float DistancePenalty = 0.35f;
        private static readonly Collider[] _overlapBuffer = new Collider[16];

        public static Alien FindAlienInFront(Transform origin, float radius, float maxAngleDeg, int layerMask)
        {
            if (!origin)
                return null;

            Camera cam = Camera.main;
            if (!cam)
            {
                Debug.LogWarning("TargetingUtil: aucune caméra principale trouvée !");
                return null;
            }

            Vector3 originPos = cam.transform.position;
            Vector3 forward = cam.transform.forward;

            int count = Physics.OverlapSphereNonAlloc(origin.position, radius, _overlapBuffer, layerMask);
            if (count <= 0)
                return null;

            float bestScore = float.MinValue;
            Alien bestAlien = null;

            for (int i = 0; i < count; i++)
            {
                Collider collider = _overlapBuffer[i];
                if (!collider || !collider.gameObject.activeInHierarchy)
                    continue;

                Alien alien = collider.GetComponent<Alien>() ?? collider.GetComponentInParent<Alien>();
                if (!alien || !alien.gameObject.activeInHierarchy)
                    continue;

                Vector3 toAlien = alien.transform.position - originPos;

                toAlien.y = 0;
                Vector3 flatForward = forward;
                flatForward.y = 0;

                if (toAlien.sqrMagnitude < 0.0001f)
                    continue;

                toAlien.Normalize();
                flatForward.Normalize();

                float angle = Vector3.Angle(flatForward, toAlien);
                if (angle > maxAngleDeg)
                    continue;

                float distance = Vector3.Distance(originPos, alien.transform.position);
                float score = Vector3.Dot(flatForward, toAlien) - DistancePenalty * (distance / radius);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestAlien = alien;
                }
            }

            return bestAlien;
        }
    }
}
