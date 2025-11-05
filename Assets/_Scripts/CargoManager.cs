using UnityEngine;

// SORUMLULUK: Kargo alma/bırakma, FixedJoint bağlantısını kurma ve kısıtlamaları yönetme.
public class CargoManager : MonoBehaviour
{
    [Header("Kargo Yönetimi Ayarları")]
    [Tooltip("Kargonun bağlanacağı boş GameObject.")]
    public Transform cargoHoldPoint; 
    
    [Tooltip("Kritik darbe hızı. Bu hızın üzerindeki çarpışmalarda kargo hasar alıp düşer.")]
    public float criticalImpactVelocity = 6f; 
    
    [Header("Kargo Dayanıklılığı (Ayarlar)")]
    [Tooltip("Yeni kargoların başlangıç dayanıklılığını ayarlamak için kullanılır.")]
    public int defaultMaxCargoDurability = 3; 
    
    [Header("Durum")]
    private FixedJoint currentCargoJoint;
    private Rigidbody carriedCargoRb; 
    
    // --- TEMEL UNITY FONKSİYONLARI ---
    
    void Update()
    {
        // Kargo Alma/Bırakma Tuşu (E Tuşu)
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentCargoJoint == null)
            {
                TryPickupCargo();
            }
            else
            {
                DropCargo();
            }
        }
    }
    
    // Harici (CargoHealthReporter) tarafından çağrılır.
    public void ReportCargoImpactForConnectedCargo(int currentDurability, int maxDurability)
    {
        // Bu metod, UI veya diğer sistemlere bilgi göndermek için kullanılır.
        Debug.Log($"CargoManager Bilgilendirildi: Bağlı kargonun kalan dayanıklılığı: {currentDurability} / {maxDurability}");
    }
    
    // --- ÖZEL KARGO FONKSİYONLARI ---
    
    private void TryPickupCargo()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, 2f); 
        foreach (var col in nearbyColliders)
        {
            if (col.CompareTag("Cargo") && col.GetComponent<Rigidbody>() != null)
            {
                carriedCargoRb = col.GetComponent<Rigidbody>();
                
                CargoHealthReporter reporter = carriedCargoRb.GetComponent<CargoHealthReporter>();
                if (reporter == null)
                {
                    Debug.LogError("Kargo nesnesinde CargoHealthReporter script'i eksik!");
                    return;
                }
                
                // 1. FİZİKSEL PATLAMAYI ENGELLEMEK İÇİN SIFIRLAMA
                // Bağlantıdan hemen önce hızları sıfırla.
                GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                carriedCargoRb.linearVelocity = Vector3.zero;
                carriedCargoRb.angularVelocity = Vector3.zero;
                
                // 2. KISITLAMALAR (CONSTRAINTS) UYGULAMASI - ÇÖZÜM
                // Kargo, drone'a bağlandığında Drone ile aynı rotasyon kısıtlamalarına sahip olmalı.
                carriedCargoRb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;

                // 3. MÜKEMMEL KONUMLAMA
                if (cargoHoldPoint != null)
                {
                    carriedCargoRb.transform.position = cargoHoldPoint.position;
                    carriedCargoRb.transform.rotation = cargoHoldPoint.rotation;
                }

                // 4. BAĞLANTIYI KURMA
                currentCargoJoint = gameObject.AddComponent<FixedJoint>();
                currentCargoJoint.connectedBody = carriedCargoRb;
                
                // FixedJoint yumuşatma parametreleri (patlamayı engellemek için)
                currentCargoJoint.breakForce = Mathf.Infinity; 
                currentCargoJoint.breakTorque = Mathf.Infinity;
                currentCargoJoint.enablePreprocessing = false; 
                
                Debug.Log($"Kargo alındı. Bağlantı kuruldu.");
                break;
            }
        }
    }

    private void DropCargo()
    {
        if (currentCargoJoint != null)
        {
            // KISITLAMALARI KALDIRMA (Kargo serbest kalmalı)
            // Sadece Z pozisyon kısıtlaması kalır, rotasyon serbest kalır.
            carriedCargoRb.constraints = RigidbodyConstraints.FreezePositionZ; 
            
            // Momentum aktarımı (Bırakılan kargo, drone'un hızını devralır)
            carriedCargoRb.linearVelocity = GetComponent<Rigidbody>().linearVelocity;
            carriedCargoRb.angularVelocity = GetComponent<Rigidbody>().angularVelocity;
            
            Destroy(currentCargoJoint);
            currentCargoJoint = null;
            carriedCargoRb = null;

            Debug.Log("Kargo bırakıldı.");
        }
    }
}