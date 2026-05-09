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
    
    [SerializeField] private CameraJoystick cameraJoystick;
    [SerializeField] private Transform playerPosition;
    [SerializeField] private float joystickOffset;
    Vector3 control = Vector3.zero;
    public float smoothTime;

    [Header ("Camera Size")]
    public float defaultCamSize = 4.5f;
    public float CamsizeAdjust = 0f;
    public float zoomedCamSize = 1f;

    public float shakeMagnitude = 0.03f;
    private Vector3 shakeOffset = Vector3.zero;
    private bool shakeEnabled = false;

    private float smoothSave;
    private Transform currentTarget;
    
    private Camera cam;
    private float targetSize;
    private float sizeVelocity;
    private void Start() {
        cam = GetComponent<Camera>();
    }
    private void LateUpdate()
    {
        Vector3 cameraTarget;
        // Temel hedef boyutu (Varsayılan + Dışarıdan gelen ekstra düzeltme)
        targetSize = defaultCamSize + CamsizeAdjust;

        if (currentTarget == playerPosition)
        {
            cameraTarget = playerPosition.position;

            Vector2 input = new Vector2(cameraJoystick.Horizontal, cameraJoystick.Vertical);
            float inputMagnitude = input.magnitude; 

            if (inputMagnitude >= 0.3f)
            {
                cameraTarget.x += input.x * joystickOffset;
                cameraTarget.y += input.y * joystickOffset;
            }
            targetSize += inputMagnitude * zoomedCamSize;
        }
        else
        {
            // Sinematik odaklanma varsa hedef objenin pozisyonunu al
            cameraTarget = currentTarget.position;
        }

        // --- UYGULAMA BÖLÜMÜ ---

        // Boyutu yumuşak bir şekilde değiştir
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetSize, ref sizeVelocity, smoothTime);

        // Z eksenini 2D kamera için sabitle
        cameraTarget.z = -10f;

        // Pozisyonu yumuşak bir şekilde takip et
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, cameraTarget, ref control, smoothTime);

        // Sarsıntı varsa (shakeOffset) üzerine ekle, yoksa Vector3.zero eklenmiş olur
        transform.position = smoothedPosition + shakeOffset;
    }

    public void StartCinematicFocus(Transform target, float smooth, float duration, bool returnToPlayer = true)
    {
        currentTarget = target;
        smoothTime = smooth;

        // Eğer otomatik dönsün istiyorsak Coroutine başlasın
        if (returnToPlayer)
        {
            StartCoroutine(WaitForFocus(duration));
        }
    }

    public void AdjustCamSize(float value)
    {
        CamsizeAdjust = value;
    }

    public void ResetCamSize()
    {
        CamsizeAdjust = 0f;
    }

    public void ShakeCamera(float duration)
    {
        StartCoroutine(CameraShake(duration));
    }
    public IEnumerator CameraShake(float duration)
    {
        cameraJoystick.ChangeAbleToInteract();
        Vector3 originalPos = Vector3.zero; // Sarsıntı öncesi sapma miktarı
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Rastgele bir yönde sarsıntı miktarı belirle
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            // Kameranın mevcut pozisyonuna bu rastgele değeri ekle
            // Not: LateUpdate içindeki SmoothDamp ile çakışmaması için 
            // sarsıntıyı transform.localPosition üzerinden manipüle ediyoruz.
            transform.position += new Vector3(x, y, 0);

            elapsed += Time.deltaTime;

            // Bir sonraki kareye kadar bekle
            yield return null;
        }

        // Sarsıntı bittiğinde kamera zaten LateUpdate içindeki 
        // SmoothDamp sayesinde hedefine (oyuncuya) yumuşakça dönecektir.
        cameraJoystick.ChangeAbleToInteract();

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
