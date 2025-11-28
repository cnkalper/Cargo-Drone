using UnityEngine;

public class WinchController : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform hookTransform;
    public Transform winchOrigin;
    public LineRenderer cableVisual;

    [Header("Kablo Uzunlu�u")]
    public float currentLength = 2f;
    public float minLength = 0.8f;
    public float maxLength = 10f;
    public float winchSpeed = 2f;

    [Header("Fizik Ayarlar�")]
    public float springForce = 200f;
    public float damperForce = 10f;

    [Header("Savrulma Engelleyici (Yumu�ak)")]
    [Tooltip("Halat�n dikeyden yapabilece�i maksimum a�� (Derece).")]
    public float maxSwingAngle = 60f;

    [Tooltip("D�zeltme kuvveti �arpan� (Daha d���k de�er = Daha yumu�ak).")]
    public float stabilityGain = 2f; // �nceki gibi sabit g�� de�il, �arpan kullanaca��z

    [Tooltip("S�n�r a��ld���nda uygulanacak ekstra s�rt�nme (H�z� �ld�r�r).")]
    public float stabilityDrag = 5f;

    private SpringJoint joint;
    private Rigidbody hookRb;
    private float defaultDrag; // Kancan�n orijinal s�rt�nmesini hat�rlamak i�in

    void Start()
    {
        if (hookTransform == null || winchOrigin == null) return;

        hookRb = hookTransform.GetComponent<Rigidbody>();
        defaultDrag = hookRb.linearDamping; // Orijinal ayar� kaydet

        // Joint Kurulumu
        joint = gameObject.AddComponent<SpringJoint>();
        joint.connectedBody = hookRb;
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = winchOrigin.localPosition;
        joint.connectedAnchor = Vector3.zero;
        joint.spring = springForce;
        joint.damper = damperForce;
        joint.maxDistance = currentLength;
        joint.minDistance = 0;

        Physics.IgnoreCollision(GetComponent<Collider>(), hookTransform.GetComponent<Collider>());
    }

    void Update()
    {
        HandleWinchInput();
        UpdateCableVisual();

        if (joint != null)
        {
            joint.maxDistance = currentLength;
        }
    }

    void FixedUpdate()
    {
        ApplySwingStabilization();
    }

    private void HandleWinchInput()
    {
        float input = 0;
        if (Input.GetKey(KeyCode.Q)) input = -1;
        if (Input.GetKey(KeyCode.E)) input = 1;

        currentLength += input * winchSpeed * Time.deltaTime;
        currentLength = Mathf.Clamp(currentLength, minLength, maxLength);
    }

    private void UpdateCableVisual()
    {
        if (cableVisual != null)
        {
            cableVisual.SetPosition(0, winchOrigin.position);
            cableVisual.SetPosition(1, hookTransform.position);
        }
    }

    // --- G�NCELLENM�� YUMU�AK STAB�L�ZE ---
    private void ApplySwingStabilization()
    {
        if (hookRb == null) return;

        Vector3 directionToHook = (hookTransform.position - winchOrigin.position).normalized;
        float angle = Vector3.Angle(Vector3.down, directionToHook);

        // E�er a�� limiti a��ld�ysa
        if (angle > maxSwingAngle)
        {
            // 1. Ne kadar a�t�k? (�rn: 65 derece ise fark 5 derecedir)
            float angleDifference = angle - maxSwingAngle;

            // 2. Yumu�ak Kuvvet: Fark ne kadar b�y�kse o kadar it.
            //    Fark az ise (1 derece), �ok hafif it.
            Vector3 correctionDir = (Vector3.down - directionToHook).normalized;

            // "Stability Gain" ile �arp�yoruz. �rn: 5 * 2 = 10 kuvvet uygula.
            hookRb.AddForce(correctionDir * angleDifference * stabilityGain, ForceMode.Acceleration);

            // 3. H�z �ld�r�c�: S�n�rdayken s�rt�nmeyi art�r ki sekmesin.
            //    Yumu�ak ge�i� (Lerp) ile s�rt�nmeyi art�r�yoruz.
            hookRb.linearDamping = Mathf.Lerp(hookRb.linearDamping, stabilityDrag, Time.fixedDeltaTime * 5f);
        }
        else
        {
            // S�n�r�n i�indeysek s�rt�nmeyi yava��a normale d�nd�r
            hookRb.linearDamping = Mathf.Lerp(hookRb.linearDamping, defaultDrag, Time.fixedDeltaTime * 5f);
        }
    }
}