using UnityEngine;

public class CargoGrabber : MonoBehaviour
{
    [Header("Magnet Settings")]
    public float grabRadius = 0.8f;
    public LayerMask cargoLayer;
    public Transform grabPoint; // 'AttachPoint' buraya gelecek

    [Header("Fine Tuning")]
    [Tooltip("Otomatik hesaplamanýn üzerine eklenecek mini pay (0.05 idealdir).")]
    public float skinWidth = 0.05f;

    [Header("State")]
    public bool isCarrying = false;
    private GameObject currentCargo;
    private HingeJoint grabJoint;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
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
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, grabRadius, cargoLayer);

        foreach (Collider col in hitColliders)
        {
            if (col.gameObject == gameObject) continue;
            if (col.transform.root == transform.root) continue;

            AttachCargo(col.gameObject);
            return;
        }
    }

    private void AttachCargo(GameObject cargo)
    {
        if (grabPoint == null) return;

        currentCargo = cargo;
        isCarrying = true;

        // --- 1. ÖNCE KARGOYU SAKÝNLEÞTÝR (Dönme sorununu çözer) ---
        Rigidbody cargoRb = cargo.GetComponent<Rigidbody>();
        cargoRb.linearVelocity = Vector3.zero;
        cargoRb.angularVelocity = Vector3.zero;

        // Kargonun açýsýný düzeltiyoruz ki köþesi kancaya girmesin
        cargo.transform.rotation = Quaternion.identity;

        // --- 2. OTOMATÝK BOYUT HESAPLAMA (Her boyuta uyar) ---
        Collider cargoCol = cargo.GetComponent<Collider>();

        // 'extents.y' bize objenin merkezinden en tepesine olan mesafeyi verir.
        float objectHalfHeight = cargoCol.bounds.extents.y;

        // Kargo Pozisyonu = Kanca - (Yarým Boy + Ufak Bir Pay)
        Vector3 targetPosition = grabPoint.position - (Vector3.up * (objectHalfHeight + skinWidth));

        cargo.transform.position = targetPosition;

        // --- 3. BAÐLANTIYI KUR (HINGE JOINT) ---
        grabJoint = gameObject.AddComponent<HingeJoint>();
        grabJoint.connectedBody = cargoRb;

        grabJoint.autoConfigureConnectedAnchor = false;
        grabJoint.anchor = Vector3.zero; // Kancanýn ucu (AttachPoint)

        // Kargo üzerindeki baðlantý noktasý: Tam tepesi (Local Space)
        // Kargo düzeltildiði için (Rotation identity), tepesi her zaman (0, Height, 0)'dýr.
        // Ancak daha garanti olmasý için relative hesaplýyoruz:
        Vector3 localConnectionPoint = cargo.transform.InverseTransformPoint(grabPoint.position);
        grabJoint.connectedAnchor = localConnectionPoint;

        grabJoint.axis = Vector3.forward; // Z ekseninde sallan

        // Çarpýþmayý kapat
        Physics.IgnoreCollision(GetComponent<Collider>(), cargoCol, true);

        Debug.Log($"CargoGrabber: Grabbed {cargo.name} (Height: {objectHalfHeight})");
    }

    private void DropCargo()
    {
        if (currentCargo != null)
        {
            Collider cargoCol = currentCargo.GetComponent<Collider>();
            Physics.IgnoreCollision(GetComponent<Collider>(), cargoCol, false);

            Rigidbody cargoRb = currentCargo.GetComponent<Rigidbody>();
            if (cargoRb)
            {
                cargoRb.WakeUp();
            }
        }

        if (grabJoint != null)
        {
            Destroy(grabJoint);
        }

        currentCargo = null;
        isCarrying = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, grabRadius);
    }
}