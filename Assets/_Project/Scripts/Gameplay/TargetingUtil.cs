using UnityEngine;

namespace Synaptik.Game
{
    public static class TargetingUtil
    {
        private const float DistancePenalty = 0.35f;
        private static readonly Collider[] _overlapBuffer = new Collider[16];

        public static Alien FindAlienInFront(Transform origin, float radius, float maxAngleDeg, int layerMask)
        {
            if (origin == null)
            {
                return null;
            }

            int count = Physics.OverlapSphereNonAlloc(origin.position, radius, _overlapBuffer, layerMask);
            if (count <= 0)
            {
                return null;
            }

            Vector3 originPos = origin.position;
            Vector3 forward = origin.forward;
            float bestScore = float.MinValue;
            Alien bestAlien = null;

            for (int i = 0; i < count; i++)
            {
                var collider = _overlapBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                if (!collider.TryGetComponent(out Alien alien))
                {
                    alien = collider.GetComponentInParent<Alien>();
                    if (alien == null)
                    {
                        continue;
                    }
                }

                Vector3 toAlien = alien.transform.position - originPos;
                float distance = toAlien.magnitude;
                if (distance <= 0.0001f)
                {
                    distance = 0.0001f;
                }

                Vector3 dir = toAlien / distance;
                float angle = Vector3.Angle(forward, dir);
                if (angle > maxAngleDeg)
                {
                    continue;
                }

                float score = Vector3.Dot(forward, dir) - DistancePenalty * (distance / radius);
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