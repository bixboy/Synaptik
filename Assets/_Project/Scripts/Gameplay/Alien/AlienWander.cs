using System.Collections;
using UnityEngine;

namespace Synaptik.Gameplay.Alien
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class AlienWander : MonoBehaviour
    {
        [Header("Cible à surveiller")]
        [SerializeField]
        private Transform player;

        [SerializeField]
        private float detectionRadius = 6f;

        [Header("Paramètres de déplacement")]
        [SerializeField]
        private bool canMove = true;

        [SerializeField]
        private float moveRadius = 5f;

        [SerializeField]
        private float moveSpeed = 2f;

        [SerializeField]
        private float waitTimeMin = 1f;

        [SerializeField]
        private float waitTimeMax = 3f;

        [Header("Paramètres de rotation")]
        [SerializeField]
        private float rotationSpeed = 5f;

        private Vector3 origin;
        private Vector3 target;
        private CharacterController controller;
        private float verticalVelocity;
        private const float Gravity = -9.81f;

        private void Start()
        {
            controller = GetComponent<CharacterController>();
            origin = transform.position;

            if (canMove)
            {
                StartCoroutine(WanderRoutine());
            }
        }

        private IEnumerator WanderRoutine()
        {
            while (true)
            {
                yield return CheckPlayerProximity();

                target = GetRandomPointAround(origin, moveRadius);

                while (!HasReachedTarget())
                {
                    if (IsPlayerClose())
                    {
                        break;
                    }

                    MoveTowardsTarget();
                    yield return null;
                }

                if (!IsPlayerClose())
                {
                    var waitTime = Random.Range(waitTimeMin, waitTimeMax);
                    yield return new WaitForSeconds(waitTime);
                }
            }
        }

        private IEnumerator CheckPlayerProximity()
        {
            if (player == null)
            {
                yield break;
            }

            while (IsPlayerClose())
            {
                LookAtPlayer();
                yield return null;
            }
        }

        private bool IsPlayerClose()
        {
            if (player == null)
            {
                return false;
            }

            var current = new Vector3(transform.position.x, 0f, transform.position.z);
            var targetPos = new Vector3(player.position.x, 0f, player.position.z);
            return Vector3.Distance(current, targetPos) <= detectionRadius;
        }

        private void LookAtPlayer()
        {
            if (player == null)
            {
                return;
            }

            var direction = (player.position - transform.position).normalized;
            direction.y = 0f;

            if (direction == Vector3.zero)
            {
                return;
            }

            var targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        private void MoveTowardsTarget()
        {
            var direction = (target - transform.position).normalized;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                var targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }

            verticalVelocity = controller.isGrounded ? -1f : verticalVelocity + Gravity * Time.deltaTime;
            var velocity = direction * moveSpeed + Vector3.up * verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }

        private Vector3 GetRandomPointAround(Vector3 center, float radius)
        {
            var randomCircle = Random.insideUnitCircle * radius;
            var point = new Vector3(center.x + randomCircle.x, center.y + 10f, center.z + randomCircle.y);

            if (Physics.Raycast(point, Vector3.down, out var hit, 20f))
            {
                point.y = hit.point.y;
            }
            else
            {
                point.y = center.y;
            }

            return point;
        }

        private bool HasReachedTarget()
        {
            var flatPos = new Vector3(transform.position.x, 0f, transform.position.z);
            var flatTarget = new Vector3(target.x, 0f, target.z);
            return Vector3.Distance(flatPos, flatTarget) < 0.3f;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(Application.isPlaying ? origin : transform.position, moveRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}
