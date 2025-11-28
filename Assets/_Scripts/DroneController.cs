using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    [Header("Uçuş Güç Ayarları")]
    public float verticalThrustForce = 40f;
    public float horizontalStrafeForce = 40f;

    [Header("Fizik ve Limitler")]
    public float maxVelocity = 10f;
    public float droneDrag = 2f;

    [Header("Görsel Efektler (Tilt)")]
    [Tooltip("Dönmesi gereken 3D Model objesi.")]
    public Transform droneModelTransform;

    [Tooltip("Maksimum kaç derece yatabilir?")]
    public float maxTiltAngle = 30f;

    [Tooltip("Yatma işleminin yumuşaklık hızı.")]
    public float tiltSpeed = 5f;

    // Referanslar
    private Rigidbody rb;
    private float verticalInput;
    private float horizontalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.linearDamping = droneDrag;
        // Fizik motoru dönmeyi engeller, biz modeli kodla döndüreceğiz.
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

        // Eğer model atanmadıysa, hata vermesin diye uyaralım
        if (droneModelTransform == null)
        {
            Debug.LogWarning("DİKKAT: 'Drone Model Transform' slotu boş! Görsel eğilme çalışmaz.");
        }
    }

    void Update()
    {
        ProcessInput();

        // Görsel güncellemeleri Update içinde yaparız (Daha akıcı görünür)
        HandleVisualTilt();
    }

    void FixedUpdate()
    {
        ApplyForces();
        LimitVelocity();
    }

    // --- ÖZEL FONKSİYONLAR ---

    private void ProcessInput()
    {
        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput = Input.GetAxisRaw("Horizontal");
    }

    private void ApplyForces()
    {
        if (verticalInput != 0)
        {
            rb.AddForce(Vector3.up * verticalInput * verticalThrustForce, ForceMode.Acceleration);
        }

        if (horizontalInput != 0)
        {
            rb.AddForce(Vector3.right * horizontalInput * horizontalStrafeForce, ForceMode.Acceleration);
        }
    }

    private void LimitVelocity()
    {
        if (rb.linearVelocity.magnitude > maxVelocity)
        {
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxVelocity);
        }
    }

    private void HandleVisualTilt()
    {
        if (droneModelTransform == null) return;

        // Hedef açıyı hesapla:
        // Hız (Velocity) bazlı yatma, daha gerçekçi görünür.
        // Hız ne kadar yüksekse o kadar çok yatar (maxTiltAngle'a kadar).
        // Eksi (-) ile çarpıyoruz çünkü sağa (+X) giderken Z ekseninde eksi yöne dönmesi gerekir.
        float targetZAngle = -rb.linearVelocity.x * 2f; // 2f çarpanı hassasiyeti artırır

        // Açıyı maksimum değerle sınırla (Kelebek gibi takla atmasın)
        targetZAngle = Mathf.Clamp(targetZAngle, -maxTiltAngle, maxTiltAngle);

        // Mevcut açıdan hedef açıya yumuşak geçiş (Interpolation)
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetZAngle);

        droneModelTransform.localRotation = Quaternion.Slerp(
            droneModelTransform.localRotation,
            targetRotation,
            Time.deltaTime * tiltSpeed
        );
    }
}