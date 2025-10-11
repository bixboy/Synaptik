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
        float freezeDistance = detectionRadius;
        float minFleeTime = 0.4f;
        float timer = 0f;

        /* if (alien?.Animator)
            alien.Animator.SetBool("IsAfraidIdle", false); */

        while (ShouldFlee())
        {
            float dist = Vector3.Distance(transform.position, player.position);

            // --- 1️⃣ joueur trop proche : l'alien est tétanisé ---
            if (dist <= freezeDistance)
            {
                LookAtPlayer();

               /* if (alien?.Animator)
                    alien.Animator.SetBool("IsAfraidIdle", false); */

                Vector3 retreatDir = -transform.forward * (0.2f * Time.deltaTime);
                controller.Move(retreatDir);

                yield return null;
                continue;
            }

            // --- 2️⃣ joueur à distance moyenne : l'alien fuit ---
            if (dist < stopDistance || timer < minFleeTime)
            {
                timer += Time.deltaTime;

                /* if (alien?.Animator)
                    alien.Animator.SetBool("IsAfraidIdle", false); */

                Vector3 fleeDir = (transform.position - player.position).normalized;
                Vector2 rand = Random.insideUnitCircle * 0.3f;
                fleeDir += new Vector3(rand.x, 0f, rand.y);
                fleeDir.Normalize();

                if (!IsGroundAhead(fleeDir))
                    fleeDir = FindAlternativeDirection(fleeDir);

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
    
    private bool IsGroundAhead(Vector3 dir)
    {
        Vector3 origin = transform.position + dir * 0.8f + Vector3.up * 0.2f;
        return Physics.Raycast(origin, Vector3.down, out _, 2f);
    }
    
    private Vector3 FindAlternativeDirection(Vector3 baseDir)
    {
        for (int i = 0; i < 8; i++)
        {
            float angle = Random.Range(-60f, 60f);
            Vector3 newDir = Quaternion.Euler(0f, angle, 0f) * baseDir;
            
            if (IsGroundAhead(newDir))
                return newDir.normalized;
        }

        return -baseDir.normalized;
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
