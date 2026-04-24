using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

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
    [SerializeField] private CameraJoystick cameraJoystick;
    [SerializeField] private Transform playerPosition;
    [SerializeField] private float joystickOffset;
    Vector3 control = Vector3.zero;
    public float smoothTime;
    private float smoothSave;
    private Transform currentTarget;
    
    private Camera cam;
    private float camsize;
    private float targetSize;
    private float sizeVelocity;
    private void Start() {
        cam = GetComponent<Camera>();
        camsize = cam.orthographicSize;
    }
    private void LateUpdate()
    {
        Vector3 cameraTarget;        

        if (currentTarget == playerPosition)
        {
            cameraTarget = playerPosition.position;
            Vector2 input = new Vector2(cameraJoystick.Horizontal, cameraJoystick.Vertical);
            float inputMagnitude = input.magnitude;

            if (Mathf.Abs(cameraJoystick.Horizontal) >= 0.3f)
                cameraTarget.x += cameraJoystick.Horizontal * joystickOffset;
            if (Mathf.Abs(cameraJoystick.Vertical) >= 0.3f)
                cameraTarget.y += cameraJoystick.Vertical * joystickOffset;

            targetSize = camsize + inputMagnitude * 1f; // Joystick hareketine bağlı olarak kamera boyutunu artır
        }
        else
        {
            cameraTarget = currentTarget.position;
        }

        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetSize, ref sizeVelocity, smoothTime);

            cameraTarget.z = -10f;

        transform.position = Vector3.SmoothDamp(transform.position, cameraTarget, ref control, smoothTime);
    }

    public void StartCinematicFocus(Transform newTarget, float newSmooth, float duration)
    {
        StartCoroutine(WaitForFocus(duration));
        if (newTarget != null)
            currentTarget = newTarget;
        smoothTime = newSmooth;
        UIManager.instance.LockJoysticks(); // Tutorial panelini kapat
    }

    private IEnumerator WaitForFocus(float duration)
    {
        yield return new WaitForSeconds(duration);
        EndCinematicFocus();
    }
    public void EndCinematicFocus()
    {
        currentTarget = playerPosition;
        smoothTime = smoothSave;
        UIManager.instance.UnlockJoysticks(); // Tutorial panelini kapat
    }
}
