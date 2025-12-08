using UnityEngine;

public class DurabilitySystem : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Damage Settings")]
    public float impactThreshold = 5f;
    public float damageMultiplier = 2f;

    [Header("Effects")]
    public GameObject explosionEffect;

    [Header("Drone Specific")]
    public bool isPlayer = false;
    [Tooltip("Drone patlayýnca kancayý da yok etmek için buraya sürükle.")]
    public GameObject connectedHook; // YENÝ: Kanca Referansý

    [Header("Audio Settings")]
    public AudioSource collisionAudioSource; // Sesi çalacak kaynak
    public AudioClip[] crashSounds; // Çarpma sesleri (Birden fazla olabilir)

    void Start()
    {
        currentHealth = maxHealth;
    }

    void OnCollisionEnter(Collision collision)
    {
        float impactForce = collision.relativeVelocity.magnitude;

        // Eðer vuruþ þiddeti, hasar eþiðinden büyükse ses çal
        if (impactForce > impactThreshold)
        {
            PlayCrashSound(impactForce); // YENÝ: Ses çalma fonksiyonu

            float damageAmount = (impactForce - impactThreshold) * damageMultiplier;
            TakeDamage(damageAmount);
        }
    }

    private void PlayCrashSound(float force)
    {
        if (collisionAudioSource != null && crashSounds.Length > 0)
        {
            // Rastgele bir çarpma sesi seç
            AudioClip clip = crashSounds[Random.Range(0, crashSounds.Length)];

            // Sesi vuruþ þiddetine göre sesli veya sessiz çal (Volume)
            // Force 20 ise volume 1 olur, 10 ise 0.5 olur.
            float volume = Mathf.Clamp01(force / 20f);

            collisionAudioSource.PlayOneShot(clip, volume);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount:F1} damage! Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // 1. Patlama Efekti (Herkes için)
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, transform.rotation);
        }

        if (isPlayer)
        {
            // Drone öldüyse: Kontrolleri kapat, ekraný aç
            DisableDrone();
        }
        else
        {
            // --- BURASI DEÐÝÞTÝ ---
            // Kargo veya baþka bir þey öldüyse:

            // 1. Önce Oyun Bitti ekranýný çaðýr
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }

            // 2. Sonra objeyi yok et
            Destroy(gameObject);
        }
    }

    private void DisableDrone()
    {
        Debug.Log("PLAYER DESTROYED!");

        // 1. KONTROLLERÝ KAPAT
        DroneController controller = GetComponent<DroneController>();
        if (controller != null) controller.enabled = false;

        // 2. HALAT SÝSTEMÝNÝ KAPAT (Hata vermesin diye önce bunu kapatýyoruz)
        WinchController winch = GetComponent<WinchController>();
        if (winch != null)
        {
            // Halatýn görselini de kapat
            if (winch.cableVisual != null) winch.cableVisual.enabled = false;
            winch.enabled = false;
        }

        // 3. KANCAYI YOK ET (YENÝ KISIM)
        if (connectedHook != null)
        {
            // Kancayý tutan bir joint varsa önce onu koparalým ki fizik motoru saçmalamasýn
            Joint[] joints = GetComponents<Joint>();
            foreach (Joint j in joints) Destroy(j);

            // Kancada patlama efekti de olsun istersen:
            if (explosionEffect != null) Instantiate(explosionEffect, connectedHook.transform.position, Quaternion.identity);

            Destroy(connectedHook);
        }

        // 4. FÝZÝÐÝ DURDUR
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 5. GÖRSELLERÝ SAKLA
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in allRenderers)
        {
            r.enabled = false;
        }

        // GameManager'a seslen: "Oyun bitti ekranýný getir!"
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }
}