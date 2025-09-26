using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class FlashlightController : MonoBehaviour
{
    [Header("Fener Kurulumu")]
    [Tooltip("F tuşuyla üretilecek fener prefabı.")]
    [SerializeField] GameObject flashlightPrefab;

    [Tooltip("Fenerin varsayılan olarak bağlanacağı soket (isteğe bağlı).")]
    [SerializeField] Transform flashlightSocket;

    [Tooltip("Soket atanmadığında karakter pozisyonuna uygulanacak ofset.")]
    [SerializeField] Vector2 spawnOffset = new(0.25f, 0.45f);

    [Header("Davranış")]
    [Tooltip("Açıksa fener fare konumuna taşınır, kapalıysa pivotta kalarak yalnızca döner.")]
    [SerializeField] bool followCursorPosition = false;

    [Tooltip("Takip modunda fenerin oyuncudan uzaklaşabileceği azami mesafe (dünya birimi).")]
    [SerializeField] float maxDistance = 6f;

    [Tooltip("Fener örneğinin Z ekseninde tutulacağı sabit değer.")]
    [SerializeField] float zDepth = 0f;

    [Tooltip("Fener yalnızca karakter aktif kontrol altındayken açılabilsin mi?")]
    [SerializeField] bool onlyWhenActiveControlled = true;

    [Tooltip("Time.timeScale ≤ 0 olduğunda güncelleme döngüsü çalıştırılmaz.")]
    [SerializeField] bool respectPauseState = true;

    [Tooltip("Aktif karakter bilgisini sağlayan PartyController/InputRouter benzeri bileşen (isteğe bağlı).")]
    [SerializeField] MonoBehaviour activeControlProvider;

    InputAdapter input;
    Camera cachedCamera;
    Mouse cachedMouse;
    Transform flashlightInstance;
    IActiveCharacterGate controlGate;
    bool warnedInvalidProvider;

    public interface IActiveCharacterGate
    {
        bool IsCharacterActive(GameObject character);
    }

    void Awake()
    {
        input = GetComponent<InputAdapter>();
        cachedCamera = Camera.main;
        cachedMouse = Mouse.current;
        CacheControlGate();
    }

    void OnEnable()
    {
        if (cachedCamera == null)
        {
            cachedCamera = Camera.main;
        }

        if (cachedMouse == null)
        {
            cachedMouse = Mouse.current;
        }
    }

    void OnValidate() => CacheControlGate();

    void OnDisable() => ShutdownInstance();

    void OnDestroy() => ShutdownInstance();

    void Update()
    {
        if (respectPauseState && Time.timeScale <= 0f)
        {
            return;
        }

        if (input == null)
        {
            return;
        }

        bool hasControl = !onlyWhenActiveControlled || HasControlAuthority();

        if (input.FlashlightTogglePressed && hasControl)
        {
            Toggle();
        }

        if (!hasControl && flashlightInstance != null)
        {
            ShutdownInstance();
            return;
        }

        if (flashlightInstance != null)
        {
            UpdateFlashlightTransform();
        }
    }

    void Toggle()
    {
        if (flashlightInstance == null)
        {
            SpawnInstance();
        }
        else
        {
            ShutdownInstance();
        }
    }

    void SpawnInstance()
    {
        if (flashlightPrefab == null)
        {
            Debug.LogWarning("Flashlight prefabı atanmamış.", this);
            return;
        }

        Transform parent = (!followCursorPosition && flashlightSocket != null) ? flashlightSocket : null;
        GameObject created = parent ? Instantiate(flashlightPrefab, parent) : Instantiate(flashlightPrefab);
        flashlightInstance = created.transform;

        Vector3 spawnPosition = GetPivotPosition();
        flashlightInstance.position = new Vector3(spawnPosition.x, spawnPosition.y, zDepth);
        UpdateFlashlightRotation(GetMouseWorldPosition(spawnPosition));
    }

    void UpdateFlashlightTransform()
    {
        Vector3 pivotPosition = GetPivotPosition();
        Vector3 mouseWorld = GetMouseWorldPosition(pivotPosition);

        if (followCursorPosition)
        {
            Vector2 ownerPosition = transform.position;
            Vector2 desired = new(mouseWorld.x, mouseWorld.y);
            Vector2 clamped = ClampToCircle(desired, ownerPosition, maxDistance);
            flashlightInstance.position = new Vector3(clamped.x, clamped.y, zDepth);
        }
        else
        {
            flashlightInstance.position = new Vector3(pivotPosition.x, pivotPosition.y, zDepth);
        }

        UpdateFlashlightRotation(mouseWorld);
    }

    void UpdateFlashlightRotation(Vector3 targetWorld)
    {
        Vector3 origin = flashlightInstance != null ? flashlightInstance.position : GetPivotPosition();
        Vector2 direction = new(targetWorld.x - origin.x, targetWorld.y - origin.y);

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        flashlightInstance.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void ShutdownInstance()
    {
        if (flashlightInstance == null)
        {
            return;
        }

        Destroy(flashlightInstance.gameObject);
        flashlightInstance = null;
    }

    bool HasControlAuthority()
    {
        if (controlGate != null)
        {
            return controlGate.IsCharacterActive(gameObject);
        }

        return true;
    }

    void CacheControlGate()
    {
        controlGate = null;

        if (activeControlProvider == null)
        {
            warnedInvalidProvider = false;
            return;
        }

        controlGate = activeControlProvider as IActiveCharacterGate;

        if (controlGate == null)
        {
            if (!warnedInvalidProvider)
            {
                Debug.LogWarning($"{activeControlProvider.name} bileşeni IActiveCharacterGate arayüzünü uygulamıyor.", this);
                warnedInvalidProvider = true;
            }
        }
        else
        {
            warnedInvalidProvider = false;
        }
    }

    Vector3 GetPivotPosition()
    {
        if (flashlightSocket != null)
        {
            Vector3 socketPosition = flashlightSocket.position;
            socketPosition.z = zDepth;
            return socketPosition;
        }

        Vector3 ownerPosition = transform.position;
        ownerPosition.x += spawnOffset.x;
        ownerPosition.y += spawnOffset.y;
        ownerPosition.z = zDepth;
        return ownerPosition;
    }

    Vector3 GetMouseWorldPosition(Vector3 fallback)
    {
        Camera cam = ResolveCamera();
        Mouse mouse = ResolveMouse();

        if (cam == null || mouse == null)
        {
            return fallback;
        }

        Vector2 mousePos = mouse.position.ReadValue();
        float planeDistance = Mathf.Abs(cam.transform.position.z - zDepth);
        Vector3 screenPoint = new(mousePos.x, mousePos.y, planeDistance);
        Vector3 world = cam.ScreenToWorldPoint(screenPoint);
        world.z = zDepth;
        return world;
    }

    Camera ResolveCamera()
    {
        if (cachedCamera == null)
        {
            cachedCamera = Camera.main;
        }

        return cachedCamera;
    }

    Mouse ResolveMouse()
    {
        if (cachedMouse == null || !cachedMouse.added)
        {
            cachedMouse = Mouse.current;
        }

        return cachedMouse;
    }

    static Vector2 ClampToCircle(Vector2 point, Vector2 center, float radius)
    {
        if (radius <= 0f)
        {
            return center;
        }

        Vector2 offset = point - center;
        float sqrRadius = radius * radius;

        if (offset.sqrMagnitude > sqrRadius)
        {
            offset = offset.normalized * radius;
            return center + offset;
        }

        return point;
    }
}
