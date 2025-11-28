using UnityEngine;

public class CargoGrabber : MonoBehaviour
{
    [Header("Mýknatýs Ayarlarý")]
    public float grabRadius = 1.5f;
    public LayerMask cargoLayer;
    public Transform grabPoint;

    [Header("Durum")]
    public bool isCarrying = false;
    private GameObject currentCargo;
    private FixedJoint grabJoint;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("SPACE tuþuna basýldý. Ýþlem baþlýyor..."); // KONTROL 1

            if (isCarrying)
            {
                DropCargo();
            }
            else
            {
                TryGrabCargo();
            }
        }
    }

    private void TryGrabCargo()
    {
        // Alaný tara
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, grabRadius, cargoLayer);

        // KONTROL 2: Etrafta kaç tane 'Cargo' katmanlý obje buldu?
        Debug.Log("Tarama yapýldý. Bulunan kargo sayýsý: " + hitColliders.Length);

        if (hitColliders.Length > 0)
        {
            GameObject cargoCandidate = hitColliders[0].gameObject;
            Debug.Log("Bulunan Kargo: " + cargoCandidate.name); // KONTROL 3
            AttachCargo(cargoCandidate);
        }
        else
        {
            Debug.LogWarning("HATA: Kanca menzilinde 'Cargo' katmanýnda obje yok! Layer ayarýný kontrol et.");
        }
    }

    private void AttachCargo(GameObject cargo)
    {
        if (grabPoint == null)
        {
            Debug.LogError("HATA: 'Grab Point' (Tutma Noktasý) scriptte atanmamýþ!");
            return;
        }

        currentCargo = cargo;
        isCarrying = true;

        cargo.transform.position = grabPoint.position;
        cargo.transform.rotation = grabPoint.rotation;

        grabJoint = gameObject.AddComponent<FixedJoint>();
        grabJoint.connectedBody = cargo.GetComponent<Rigidbody>();
        grabJoint.breakForce = Mathf.Infinity;

        Debug.Log("BAÞARILI: Kargo baðlandý!");
    }

    private void DropCargo()
    {
        if (grabJoint != null)
        {
            Destroy(grabJoint);
        }

        if (currentCargo != null)
        {
            // Kutuyu biraz uyandýr, havada donup kalmasýn
            Rigidbody cargoRb = currentCargo.GetComponent<Rigidbody>();
            if (cargoRb) cargoRb.WakeUp();
        }

        currentCargo = null;
        isCarrying = false;
        Debug.Log("Kargo býrakýldý.");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, grabRadius);
    }
}