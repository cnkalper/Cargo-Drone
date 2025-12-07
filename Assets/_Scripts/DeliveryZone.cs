using UnityEngine;

public class DeliveryZone : MonoBehaviour
{
    [Header("Settings")]
    public string cargoTag = "Cargo";
    public GameObject deliveryEffect;

    [Header("Target Material")]
    public Material successMaterial;

    private bool levelFinished = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(cargoTag) && !levelFinished)
        {
            CompleteDelivery(other.gameObject);
        }
    }

    private void CompleteDelivery(GameObject cargo)
    {
        levelFinished = true;
        Debug.Log("DELIVERY SUCCESSFUL!");

        // 1. KANCADAN AYIR (DROP)
        // Sahnedeki kancayý bul ve "Býrak" de.
        CargoGrabber droneHook = FindFirstObjectByType<CargoGrabber>();
        if (droneHook != null)
        {
            droneHook.DropCargo();
        }

        // 2. TEKRAR TUTULMASINI ENGELLE (KATMAN DEÐÝÞTÝRME)
        // Kargonun layerýný "Default" yapýyoruz.
        // Böylece CargoGrabber (sadece Cargo_Layer aradýðý için) bunu artýk göremez.
        cargo.layer = LayerMask.NameToLayer("Default");

        // NOT: Kargo artýk fiziksel kalacak (Rigidbody'sine dokunmadýk).
        // Yere düþer, yuvarlanýr ama tekrar alýnamaz.

        // 3. GÖRSEL VE EFEKTLER
        if (successMaterial != null)
        {
            Renderer r = cargo.GetComponent<Renderer>();
            if (r != null) r.material = successMaterial;
        }

        if (deliveryEffect != null)
        {
            Instantiate(deliveryEffect, transform.position, Quaternion.identity);
        }

        // 4. OYUNU KAZAN (GECÝKMELÝ)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LevelComplete();
        }
    }
}