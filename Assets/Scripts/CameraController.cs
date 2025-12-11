using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private AttackJoystick attackJoystick;
    [SerializeField] private Transform playerPosition;
    [SerializeField] private float joystickOffset;
    Vector3 control = Vector3.zero;
    public float smoothTime;
    private float smoothSave;
    private Transform currentTarget;

    private void Awake()
    {
        currentTarget = playerPosition;
        smoothSave = smoothTime;
    }
    private void LateUpdate()
    {
        Vector3 cameraTarget;

        if (currentTarget == playerPosition)
        {
            cameraTarget = playerPosition.position;

            if (Mathf.Abs(attackJoystick.Horizontal) >= 0.3f)
                cameraTarget.x += attackJoystick.Horizontal * joystickOffset;
            if (Mathf.Abs(attackJoystick.Vertical) >= 0.3f)
                cameraTarget.y += attackJoystick.Vertical * joystickOffset;
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
