using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class AlienWander : MonoBehaviour
{
    [Header("Paramètres de déplacement")]
    public bool canMove = true;
    public float moveRadius = 5f;
    public float moveSpeed = 2f;
    public float waitTimeMin = 1f;
    public float waitTimeMax = 3f;

    [Header("Paramètres de rotation")]
    public float rotationSpeed = 5f;

    private Vector3 _origin;
    private Vector3 _target;
    private CharacterController _controller;
    private bool _isMoving;
    
    private float _verticalVelocity = 0f;
    private float gravity = -9.81f;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _origin = transform.position;
        
        if (canMove)
            StartCoroutine(WanderRoutine());
    }

    IEnumerator WanderRoutine()
    {
        while (true)
        {
            _target = GetRandomPointAround(_origin, moveRadius);
            _isMoving = true;

            while (!HasReachedTarget())
            {
                MoveTowardsTarget();
                yield return null;
            }

            _isMoving = false;

            float waitTime = Random.Range(waitTimeMin, waitTimeMax);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = (_target - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }

        if (_controller.isGrounded)
            _verticalVelocity = -1f;
        else
            _verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = direction * moveSpeed + Vector3.up * _verticalVelocity;
        _controller.Move(velocity * Time.deltaTime);
    }

    private Vector3 GetRandomPointAround(Vector3 center, float radius)
    {
        Vector2 randomCircle = Random.insideUnitCircle * radius;
        Vector3 point = new Vector3(center.x + randomCircle.x, center.y + 10f, center.z + randomCircle.y);

        if (Physics.Raycast(point, Vector3.down, out RaycastHit hit, 20f))
            point.y = hit.point.y;
        else
            point.y = center.y;

        return point;
    }
    
    private bool HasReachedTarget()
    {
        Vector3 flatPos = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 flatTarget = new Vector3(_target.x, 0f, _target.z);
        return Vector3.Distance(flatPos, flatTarget) < 0.3f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Application.isPlaying ? _origin : transform.position, moveRadius);
    }
}
