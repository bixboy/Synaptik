using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;

    [Header("Movement (plan XZ)")]
    [SerializeField, Min(0f)] private float _maxSpeed = 5f;       // m/s (vitesse de croisière)
    [SerializeField, Min(0f)] private float _acceleration = 20f;  // m/s² quand input ≠ 0
    [SerializeField, Min(0f)] private float _deceleration = 25f;  // m/s² quand input = 0

    [Header("Camera-relative ?")]
    [SerializeField] private bool _cameraRelative = true;
    [SerializeField, ShowIf("_cameraRelative")] private Camera _camera; // si null et cameraRelative=true, prendra Camera.main

    private void Reset()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Awake()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody>();
        if (_cameraRelative && _camera == null) _camera = Camera.main;

        // Optionnel : évite que le rigidbody “tombe” sur les côtés.
        _rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void FixedUpdate()
    {
        // 1) Récupère l'input 2D (déjà normalisé côté InputsDetection)
        Vector2 input = InputsDetection.Instance ? InputsDetection.Instance.MoveVector : Vector2.zero;

        // 2) Direction voulue sur XZ
        Vector3 dir;
        if (_cameraRelative && _camera != null)
        {
            Vector3 fwd = _camera.transform.forward; fwd.y = 0f; fwd.Normalize();
            Vector3 right = _camera.transform.right; right.y = 0f; right.Normalize();
            dir = right * input.x + fwd * input.y;   // x → droite, y → avant
        }
        else
        {
            dir = new Vector3(input.x, 0f, input.y); // world-relative
        }

        // 3) Vitesse cible horizontale (on garde la gravité sur Y)
        Vector3 v = _rb.linearVelocity;
        Vector3 vHoriz = new Vector3(v.x, 0f, v.z);
        Vector3 targetHoriz = dir * _maxSpeed;

        // 4) Accélérer vers la cible (ou freiner si pas d'input)
        float accel = (dir.sqrMagnitude > 0.0001f) ? _acceleration : _deceleration;

        // Delta-v max autorisé sur ce pas
        float maxDeltaV = accel * Time.fixedDeltaTime;

        // Delta-v horizontal voulu
        Vector3 deltaV = targetHoriz - vHoriz;
        if (deltaV.sqrMagnitude > maxDeltaV * maxDeltaV)
            deltaV = deltaV.normalized * maxDeltaV;

        // 5) Applique une *VelocityChange* (indépendant de la masse, pas de double dt)
        _rb.AddForce(deltaV, ForceMode.VelocityChange);
        // (On n'affecte que l'horizontal ⇒ Y conserve la gravité)
    }
}
