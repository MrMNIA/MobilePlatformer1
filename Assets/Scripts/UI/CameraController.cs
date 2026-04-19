using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        currentTarget = playerPosition;
        smoothSave = smoothTime;
    }
    
    [SerializeField] private Transform defaultTarget;
    [SerializeField] private CameraJoystick camera;
    [SerializeField] private Transform playerPosition;
    [SerializeField] private float joystickOffset;
    Vector3 control = Vector3.zero;
    public float smoothTime;
    private float smoothSave;
    private Transform currentTarget;
    private void LateUpdate()
    {
        Vector3 cameraTarget;

        if (currentTarget == playerPosition)
        {
            cameraTarget = playerPosition.position;

            if (Mathf.Abs(camera.Horizontal) >= 0.3f)
                cameraTarget.x += camera.Horizontal * joystickOffset;
            if (Mathf.Abs(camera.Vertical) >= 0.3f)
                cameraTarget.y += camera.Vertical * joystickOffset;
        }
        else
        {
            cameraTarget = currentTarget.position;
        }

            cameraTarget.z = -10f;

        transform.position = Vector3.SmoothDamp(transform.position, cameraTarget, ref control, smoothTime);
    }

    public void StartCinematicFocus(Transform newTarget, float newSmooth)
    {
        if (newTarget != null)
            currentTarget = newTarget;
        smoothTime = newSmooth;
    }

    public void EndCinematicFocus()
    {
        currentTarget = playerPosition;
        smoothTime = smoothSave;
    }
}
