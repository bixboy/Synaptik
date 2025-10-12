using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class AlienWander : MonoBehaviour
{
    [Header("Cible à surveiller")]
    [SerializeField]
    private Transform player;

    [SerializeField]
    private Alien alien;

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
    

    [Header("Paramètres de Peur")]
    [SerializeField]
    private float fearfulSpeedMultiplier = 1.5f;

    [SerializeField]
    [Min(0f)]
    private float fearfulDetectionRadiusMultiplier = 0.6f;
    
    [SerializeField]
    private float fearfulPlayerDistance = 6f;
    

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
        if (!alien)
        {
            alien = GetComponent<Alien>();
        }
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
                if (ShouldFlee())
                {
                    yield return FleeRoutine();
                    break;
                }

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
        if (!player)
            yield break;

        while (IsPlayerClose())
        {
            if (ShouldFlee())
            {
                yield break;
            }

            LookAtPlayer();
            yield return null;
        }
    }

    private bool IsPlayerClose()
    {
        if (!player)
            return false;

        var current = new Vector3(transform.position.x, 0f, transform.position.z);
        var targetPos = new Vector3(player.position.x, 0f, player.position.z);
        return Vector3.Distance(current, targetPos) <= GetCurrentDetectionRadius();
    }

    private void LookAtPlayer()
    {
        if (!player)
            return;

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

        MoveInDirection(direction, moveSpeed);
    }

    private IEnumerator FleeRoutine()
    {
        if (!player)
            yield break;

        float stopDistance = fearfulPlayerDistance;
        float freezeDistance = GetCurrentDetectionRadius();
        float minFleeTime = 0.4f;
        float timer = 0f;
        float stuckTimer = 0f;

        Vector3 lastPos = transform.position;
        Vector3 fleeDir = (transform.position - player.position).normalized;

        while (ShouldFlee())
        {
            float dist = Vector3.Distance(transform.position, player.position);

            // 1️⃣ Trop proche : tétanisé
            if (dist <= freezeDistance)
            {
                LookAtPlayer();
                
                /* if (alien?.Animator)
                    alien.Animator.SetBool("IsAfraidIdle", false); */
                
                controller.Move(-transform.forward * 0.05f * Time.deltaTime);
                
                yield return null;
                continue;
            }

            // 2️⃣ Fuite active
            if (dist < stopDistance || timer < minFleeTime)
            {
                timer += Time.deltaTime;
                
                /* if (alien?.Animator)
                    alien.Animator.SetBool("IsAfraidIdle", false); */

                Vector3 desiredDir = (transform.position - player.position).normalized;
                desiredDir.y = 0f;

                Vector2 rand = Random.insideUnitCircle * 0.2f;
                desiredDir += new Vector3(rand.x, 0f, rand.y);
                desiredDir.Normalize();

                fleeDir = ComputeAvoidanceDirection(desiredDir);

                if (Vector3.Distance(transform.position, lastPos) < 0.05f)
                {
                    stuckTimer += Time.deltaTime;
                    if (stuckTimer > 1f)
                    {
                        fleeDir = FindEscapeDirection();
                        stuckTimer = 0f;
                    }
                }
                else
                {
                    stuckTimer = 0f;
                }

                lastPos = transform.position;

                MoveInDirection(fleeDir, moveSpeed * fearfulSpeedMultiplier);
            }
            // --- 3️⃣ joueur trop loin : l'alien s'arrête et observe ---
            else
            {
                LookAtPlayer();
                
                /* if (alien?.Animator)
                    alien.Animator.SetBool("IsAfraidIdle", true); */
            }

            yield return null;
        }
        
        /* if (alien?.Animator)
            alien.Animator.SetBool("IsAfraidIdle", false); */
    }
   
   
    // ===== Fearful Helper ===== //
    
    private Vector3 ComputeAvoidanceDirection(Vector3 desiredDir)
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 finalDir = desiredDir;

        bool hitCenter = Physics.Raycast(origin, desiredDir, out RaycastHit hitC, 1f);
        bool hitLeft = Physics.Raycast(origin, Quaternion.Euler(0, -30f, 0) * desiredDir, out RaycastHit hitL, 1f);
        bool hitRight = Physics.Raycast(origin, Quaternion.Euler(0, 30f, 0) * desiredDir, out RaycastHit hitR, 1f);

        if (hitCenter)
        {
            Vector3 slide = Vector3.ProjectOnPlane(desiredDir, hitC.normal).normalized;
            finalDir = slide;
        }
        else if (hitLeft && !hitRight)
        {
            finalDir = Quaternion.Euler(0, 35f, 0) * desiredDir;
        }
        else if (hitRight && !hitLeft)
        {
            finalDir = Quaternion.Euler(0, -35f, 0) * desiredDir;
        }

        if (!IsGroundAhead(finalDir))
        {
            finalDir = Quaternion.Euler(0, 90f, 0) * desiredDir;
        }

        return finalDir.normalized;
    }

    private Vector3 FindEscapeDirection()
    {
        for (int i = 0; i < 12; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere;
            randomDir.y = 0f;
            randomDir.Normalize();

            if (!IsObstacleAhead(randomDir) && IsGroundAhead(randomDir))
                return randomDir;
        }

        return -transform.forward;
    }

    private bool IsObstacleAhead(Vector3 dir)
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        return Physics.Raycast(origin, dir, 0.8f);
    }

    private bool IsGroundAhead(Vector3 dir)
    {
        Vector3 origin = transform.position + dir * 0.7f + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, 2f);
    }

    private void MoveInDirection(Vector3 direction, float speed)
    {
        if (direction != Vector3.zero)
        {
            var targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
        else
        {
            return;
        }

        verticalVelocity = controller.isGrounded ? -1f : verticalVelocity + Gravity * Time.deltaTime;
        var velocity = direction * speed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }
    
    // ===== Movement Helper ===== //
    
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

    private bool ShouldFlee()
    {
        return canMove && player && alien && alien.Emotion == Emotion.Fearful;
    }
    
    private float GetCurrentDetectionRadius()
    {
        if (ShouldFlee())
        {
            return Mathf.Max(0f, detectionRadius * fearfulDetectionRadiusMultiplier);
        }

        return Mathf.Max(0f, detectionRadius);
    }
    
    
    private void OnDrawGizmosSelected()
    {
        if (!canMove)
            return;
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Application.isPlaying ? origin : transform.position, moveRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, GetCurrentDetectionRadius());

        if (ShouldFlee())
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, fearfulPlayerDistance);   
        }
    }
}
