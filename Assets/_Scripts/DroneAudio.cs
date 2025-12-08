using UnityEngine;

public class DroneAudio : MonoBehaviour
{
    [Header("Settings")]
    public AudioSource motorSource;
    public Rigidbody droneRb;

    [Header("Pitch Tuning")]
    public float minPitch = 1.0f; // Rolanti sesi
    public float maxPitch = 2.0f; // Tam gaz sesi
    public float pitchSpeed = 5f; // Sesin ne kadar yumuþak deðiþeceði

    void Start()
    {
        if (motorSource == null) motorSource = GetComponent<AudioSource>();
        if (droneRb == null) droneRb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (motorSource == null || droneRb == null) return;

        // Drone'un hýzý ne kadar?
        float currentSpeed = droneRb.linearVelocity.magnitude;

        // Hýzý 0 ile 10 arasýnda bir orana çevir (Maks hýzý 10 varsayýyoruz)
        float speedRatio = Mathf.Clamp01(currentSpeed / 10f);

        // Hedef Pitch'i hesapla (Hýz arttýkça ses incelir)
        float targetPitch = Mathf.Lerp(minPitch, maxPitch, speedRatio);

        // Sesi yumuþakça deðiþtir
        motorSource.pitch = Mathf.Lerp(motorSource.pitch, targetPitch, Time.deltaTime * pitchSpeed);
    }
}