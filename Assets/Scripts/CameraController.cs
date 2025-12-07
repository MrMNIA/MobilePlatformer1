using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private AttackJoystick attackJoystick;
    [SerializeField] private Transform playerPosition;
    [SerializeField] private float attackDistance;
    Vector3 control = Vector3.zero;
    public float smooothTime;
    private Transform currentTarget;

    private void Awake()
    {
        currentTarget = playerPosition;
    }
    private void LateUpdate()
    {
        Vector3 cameraTarget;

        if (currentTarget == playerPosition)
        {
            cameraTarget = playerPosition.position;

            if (Mathf.Abs(attackJoystick.Horizontal) >= 0.3f)
                cameraTarget.x += attackJoystick.Horizontal * attackDistance;
            if (Mathf.Abs(attackJoystick.Vertical) >= 0.3f)
                cameraTarget.y += attackJoystick.Vertical * attackDistance;
        }
        else
        {
            cameraTarget = currentTarget.position;
        }

            cameraTarget.z = -10f;

        transform.position = Vector3.SmoothDamp(transform.position, cameraTarget, ref control, smooothTime);
    }

    public void StartCinematicFocus(Transform newTarget)
    {
        if (newTarget != null)
            currentTarget = newTarget;
    }

    public void EndCinematicFocus()
    {
        currentTarget = playerPosition;
    }
}
