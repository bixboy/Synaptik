using System;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    [SerializeField]
    private Rigidbody _rb;
    [SerializeField]
    private float _speed = 1.0f;

    private void FixedUpdate()
    {
        Vector2 movement = InputsDetection.Instance.MoveVector;
        _rb.AddForce(new Vector3(movement.x, 0, movement.y) * (_speed * Time.fixedDeltaTime));
    }
}