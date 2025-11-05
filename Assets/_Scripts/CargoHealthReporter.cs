using UnityEngine;

// SORUMLULUK: Kargo nesnesinin kendisine gelen çarpışmaları algılayıp, 
// bağlı olduğu Drone üzerindeki CargoManager'a hasar bilgisini rapor eder.
public class CargoHealthReporter : MonoBehaviour
{
    [Header("Kargo Dayanıklılık Ayarları")]
    // Kargonun ilk dayanıklılığı, Inspector'dan ayarlanır.
    public int maxCargoDurability = 3; 
    private int currentCargoDurability;
    [Tooltip("Kritik darbe hızı. Bu hızın üzerindeki çarpışmalarda kargo hasar alıp düşer.")]
    public float criticalImpactVelocity = 6f; 

    void Start()
    {
        // Kargo başlangıçta tam canla başlar.
        currentCargoDurability = maxCargoDurability;
        Debug.Log($"Kargo hazırlandı. Başlangıç Dayanıklılığı: {currentCargoDurability}");
    }

    void OnCollisionEnter(Collision collision)
    {
        // Yüksek hızda çarpışma olmadıkça performans için raporlama yapmayalım.
        float impactVelocity = collision.relativeVelocity.magnitude;
        if (impactVelocity < criticalImpactVelocity) return; // Sadece kritik çarpışmaları işle
        
        // Canı zaten 0 veya altındaysa tekrar hasar uygulama.
        if (currentCargoDurability <= 0) return;

        Debug.Log(">>> KARGO ÇARPIŞMASI TESPİT EDİLDİ: " + collision.gameObject.name, this);
        
        // 1. Hasar Uygula (Bağlı olup olmaması fark etmez - Yeni Kural)
        currentCargoDurability--;
        Debug.LogWarning($"KRİTİK KARGO HASARI! Kalan Dayanıklılık: {currentCargoDurability} / {maxCargoDurability}");

        if (currentCargoDurability <= 0)
        {
            Die();
            return;
        }
        
        // 2. Eğer bağlıysak, Manager'a da haber verelim (Log/UI mekaniği için)
        GameObject droneObject = GameObject.FindWithTag("Player"); 
        
        if (droneObject != null)
        {
            CargoManager manager = droneObject.GetComponent<CargoManager>();
            FixedJoint droneJoint = droneObject.GetComponent<FixedJoint>();

            // Drone'da CargoManager var mı VE bu kargoyu taşıyor mu?
            if (manager != null && droneJoint != null && droneJoint.connectedBody == GetComponent<Rigidbody>())
            {
                // Manager'ı sadece bilgi amaçlı çağırıyoruz. Gerçek hasar bu script'te uygulandı.
                Debug.Log("RAPOR BAŞARILI: Manager bulundu ve hasar iletiliyor.");
                // Manager'a güncel durumu bildir.
                manager.ReportCargoImpactForConnectedCargo(currentCargoDurability, maxCargoDurability); 
            }
        }
    }

    private void Die()
    {
        Debug.LogError("KARGO TAMAMEN KIRILDI! Görev Başarısız/Yüksek Ceza!");
        // İSTEK: Dayanıklılık bitince kargo yok olacak.
        Destroy(gameObject);
    }
}