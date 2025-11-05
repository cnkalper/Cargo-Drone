using UnityEngine;

// SORUMLULUK: ROKET İVMELEMESİ ve ROTASYON KONTROLÜ
public class DroneController : MonoBehaviour
{
    [Header("Roket Kontrol Ayarları")]
    [Tooltip("Roketin ileri itme kuvveti.")]
    public float thrustForce = 50f; 
    
    [Tooltip("Drone'un dönme hızı.")]
    public float rotationSpeed = 150f;
    
    // Frenleme artık manueldir (kod ile aktif frenleme kaldırıldı). 
    // Drone, itki kesilince süzülmeye devam eder.

    [Header("Referanslar")]
    private Rigidbody rb;
    private float rotationInput;
    private bool isThrusting; 
    private Health droneHealth; 

    // --- TEMEL UNITY FONKSİYONLARI ---

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        droneHealth = GetComponent<Health>(); 
        
        if (rb == null || droneHealth == null)
        {
            Debug.LogError("HATA: Gerekli bileşenler (Rigidbody VEYA Health) Drone_Player üzerinde bulunamadı!");
        }
    }

    void Update()
    {
        ProcessInput();
    }

    void FixedUpdate()
    {
        // Fizik motoru içinde sadece döndürme ve itme kuvveti uygulanır.
        ApplyRotation();
        ApplyThrust();
        // ApplyBraking() KESİNLİKLE KALDIRILDI. Drone serbest süzülür.
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (droneHealth == null) return;

        float impactVelocity = collision.relativeVelocity.magnitude;
        
        if (impactVelocity > droneHealth.criticalDamageVelocity)
        {
            droneHealth.TakeDamage(20); 
        }
        // Hafif çarpışma geri bildirimi (ses/görsel için) burada ele alınır.
    }

    // --- ÖZEL FONKSİYONLAR ---

    private void ProcessInput()
    {
        // 1. İvme Kontrolü (Space ile İleri Git)
        isThrusting = Input.GetKey(KeyCode.Space);

        // 2. Rotasyon Kontrolü (A/D ile Dön)
        rotationInput = Input.GetAxisRaw("Horizontal"); // A (-1) veya D (1)

        // Yükseklik/Alçalma kontrolü artık yok.
    }

    private void ApplyRotation()
    {
        if (rotationInput != 0)
        {
            // Angular Velocity yerine Quaternion kullanmak, arcade dönüş için daha akıcıdır.
            // Rotasyon, Y ekseninde (dünya Z ekseni derinlik olduğu için) yapılır.
            float rotationAmount = rotationInput * rotationSpeed * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0f, 0f, -rotationAmount); // -Y ekseni etrafında döndür
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
    }

    private void ApplyThrust()
    {
        if (isThrusting)
        {
            // İtkiyi, drone'un baktığı yöne (Transform.up) uygula.
            // 2D/2.5D düzlemde ileri yön genellikle Y (up) eksenidir.
            Vector3 thrustDirection = transform.up; 
            
            // Eğer oyun XZ düzlemindeyse, burası 'transform.forward' olmalıdır.
            // Bizim oyunumuz XY düzleminde (2.5D) olduğu için up kullanıyoruz.
            rb.AddForce(thrustDirection * thrustForce, ForceMode.Acceleration);
        }
    }
    
    // ApplyBraking metodu artık yoktur.
}