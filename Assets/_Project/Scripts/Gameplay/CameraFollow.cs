using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Cible à suivre")]
    public Transform player;

    [Header("Paramètres de vue")]
    public Vector3 offset = new Vector3(0f, 10f, -8f);
    public float rotationSmoothness = 5f;

    [Header("Limites de rotation")]
    public float maxYawAngle = 45f;
    public float maxPitchAngle = 20f;

    [Header("Zoom")]
    public float zoomSpeed = 5f;
    public float minZoom = 5f;
    public float maxZoom = 15f;

    private float currentZoom;
    private float targetZoom;
    private Vector3 initialPosition;
    private Quaternion baseRotation;

    void Start()
    {
        if (player == null)
        {
            Debug.LogWarning("⚠️ Aucun player assigné à la caméra !");
            enabled = false;
            return;
        }

        currentZoom = offset.magnitude;
        targetZoom = currentZoom;

        initialPosition = transform.position;
        baseRotation = transform.rotation;
    }

    void LateUpdate()
    {
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomSpeed);
        Camera cam = GetComponent<Camera>();
        if (cam)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60f - (currentZoom - minZoom) * 2f, Time.deltaTime * zoomSpeed);   
        }

        Vector3 dirToPlayer = player.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(dirToPlayer, Vector3.up);

        Quaternion smoothRotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothness);
        Quaternion limitedRotation = LimitRotation(smoothRotation, baseRotation, maxYawAngle, maxPitchAngle);

        transform.rotation = limitedRotation;
    }

    private Quaternion LimitRotation(Quaternion current, Quaternion reference, float maxYaw, float maxPitch)
    {
        Vector3 currentEuler = current.eulerAngles;
        Vector3 baseEuler = reference.eulerAngles;

        float yawDelta = Mathf.DeltaAngle(baseEuler.y, currentEuler.y);
        float pitchDelta = Mathf.DeltaAngle(baseEuler.x, currentEuler.x);

        yawDelta = Mathf.Clamp(yawDelta, -maxYaw, maxYaw);
        pitchDelta = Mathf.Clamp(pitchDelta, -maxPitch, maxPitch);

        return Quaternion.Euler(baseEuler.x + pitchDelta, baseEuler.y + yawDelta, 0f);
    }

    public void ZoomIn(float amount = 2f)
    {
        targetZoom = Mathf.Max(minZoom, targetZoom - amount);
    }

    public void ZoomOut(float amount = 2f)
    {
        targetZoom = Mathf.Min(maxZoom, targetZoom + amount);
    }
}
