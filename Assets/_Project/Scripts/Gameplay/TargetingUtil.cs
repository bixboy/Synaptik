using Synaptik.Gameplay.Alien;
using Synaptik.Interfaces;
using UnityEngine;

namespace Synaptik.Gameplay
{
    public static class TargetingUtil
    {
        private const float DistancePenalty = 0.35f;
        private static readonly Collider[] OverlapBuffer = new Collider[16];

        public static Alien FindAlienInFront(Transform origin, float radius, float maxAngleDeg, int layerMask)
        {
            if (origin == null)
            {
                return null;
            }

            var camera = Camera.main;
            if (camera == null)
            {
                Debug.LogWarning("TargetingUtil: aucune caméra principale trouvée !");
                return null;
            }

            var originPos = camera.transform.position;
            var forward = camera.transform.forward;

            var count = Physics.OverlapSphereNonAlloc(origin.position, radius, OverlapBuffer, layerMask);
            if (count <= 0)
            {
                return null;
            }

            var bestScore = float.MinValue;
            Alien bestAlien = null;

            for (var i = 0; i < count; i++)
            {
                var collider = OverlapBuffer[i];
                if (collider == null || !collider.gameObject.activeInHierarchy)
                {
                    continue;
                }

                var alien = collider.GetComponent<Alien>() ?? collider.GetComponentInParent<Alien>();
                if (alien == null || !alien.gameObject.activeInHierarchy)
                {
                    continue;
                }

                var toAlien = alien.transform.position - originPos;
                toAlien.y = 0f;

                var flatForward = forward;
                flatForward.y = 0f;

                if (toAlien.sqrMagnitude < 0.0001f)
                {
                    continue;
                }

                toAlien.Normalize();
                flatForward.Normalize();

                var angle = Vector3.Angle(flatForward, toAlien);
                if (angle > maxAngleDeg)
                {
                    continue;
                }

                var distance = Vector3.Distance(originPos, alien.transform.position);
                var score = Vector3.Dot(flatForward, toAlien) - DistancePenalty * (distance / radius);

                if (score <= bestScore)
                {
                    continue;
                }

                bestScore = score;
                bestAlien = alien;
            }

            return bestAlien;
        }

        public static IInteraction FindInteractionInFront(Transform origin, float radius, float maxAngleDeg, int layerMask)
        {
            if (origin == null)
            {
                return null;
            }

            var camera = Camera.main;
            if (camera == null)
            {
                Debug.LogWarning("TargetingUtil: aucune caméra principale trouvée !");
                return null;
            }

            var originPos = camera.transform.position;
            var forward = camera.transform.forward;

            var count = Physics.OverlapSphereNonAlloc(origin.position, radius, OverlapBuffer, layerMask, QueryTriggerInteraction.Ignore);
            if (count <= 0)
            {
                return null;
            }

            var bestScore = float.MinValue;
            IInteraction best = null;

            var flatForward = forward;
            flatForward.y = 0f;
            flatForward.Normalize();

            for (var i = 0; i < count; i++)
            {
                var collider = OverlapBuffer[i];
                if (collider == null || !collider.gameObject.activeInHierarchy)
                {
                    continue;
                }

                var interactable = collider.GetComponent<IInteraction>() ?? collider.GetComponentInParent<IInteraction>();
                if (interactable == null)
                {
                    continue;
                }

                if (interactable is not Component component || !component.gameObject.activeInHierarchy)
                {
                    continue;
                }

                var toTarget = component.transform.position - originPos;
                toTarget.y = 0f;

                var sqrMagnitude = toTarget.sqrMagnitude;
                if (sqrMagnitude < 0.0001f)
                {
                    continue;
                }

                toTarget /= Mathf.Sqrt(sqrMagnitude);

                var angle = Vector3.Angle(flatForward, toTarget);
                if (angle > maxAngleDeg)
                {
                    continue;
                }

                var distance = Mathf.Sqrt(sqrMagnitude);
                var score = Vector3.Dot(flatForward, toTarget) - DistancePenalty * (distance / radius);

                if (score <= bestScore)
                {
                    continue;
                }

                bestScore = score;
                best = interactable;
            }

            return best;
        }
    }
}
