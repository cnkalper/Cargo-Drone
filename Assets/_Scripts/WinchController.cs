using UnityEngine;

public class WinchController : MonoBehaviour
{
    [Header("References")]
    public Transform hookTransform;
    public Transform winchOrigin;
    public LineRenderer cableVisual;

    [Header("Cable Length")]
    public float currentLength = 2f;
    public float minLength = 0.8f;
    public float maxLength = 10f;
    public float winchSpeed = 2f;

    [Header("Physics Settings")]
    public float springForce = 200f;
    public float damperForce = 10f;

    [Header("Stabilizer")]
    public float maxSwingAngle = 60f;
    public float stabilityGain = 2f;
    public float stabilityDrag = 5f;

    [Header("Rope Collision (Snapping)")]
    public LayerMask obstacleLayer; // Select 'Obstacle' layer here
    public Color normalColor = Color.black;
    public Color warningColor = Color.red;

    [Tooltip("How long can the rope touch a wall before breaking?")]
    public float breakTime = 2f;
    private float contactTimer = 0f;

    private SpringJoint joint;
    private Rigidbody hookRb;
    private float defaultDrag;

    // To drop cargo when rope breaks
    private CargoGrabber hookGrabber;

    public AudioSource winchAudio; // Loop ayarlý bir AudioSource

    void Start()
    {
        if (hookTransform == null || winchOrigin == null) return;

        hookRb = hookTransform.GetComponent<Rigidbody>();
        defaultDrag = hookRb.linearDamping;

        // Get the CargoGrabber to force drop if rope breaks
        hookGrabber = hookTransform.GetComponent<CargoGrabber>();

        // Joint Setup
        SetupJoint();

        // Default visual setup
        if (cableVisual != null)
        {
            cableVisual.startColor = normalColor;
            cableVisual.endColor = normalColor;
        }

        Physics.IgnoreCollision(GetComponent<Collider>(), hookTransform.GetComponent<Collider>());
    }

    void Update()
    {
        // If rope is broken (joint is null), stop everything
        if (joint == null)
        {
            cableVisual.enabled = false;
            return;
        }

        HandleWinchInput();
        UpdateCableVisual();
        CheckRopeCollision(); // NEW: Check if hitting walls

        joint.maxDistance = currentLength;
    }

    void FixedUpdate()
    {
        if (joint != null) ApplySwingStabilization();
    }

    private void SetupJoint()
    {
        joint = gameObject.AddComponent<SpringJoint>();
        joint.connectedBody = hookRb;
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = winchOrigin.localPosition;
        joint.connectedAnchor = Vector3.zero;
        joint.spring = springForce;
        joint.damper = damperForce;
        joint.maxDistance = currentLength;
        joint.minDistance = 0;
    }

    private void HandleWinchInput()
    {
        float input = 0;
        bool isMoving = false; // Hareket var mý?

        if (Input.GetKey(KeyCode.Q)) { input = -1; isMoving = true; }
        if (Input.GetKey(KeyCode.E)) { input = 1; isMoving = true; }

        // SES MANTIÐI
        if (winchAudio != null)
        {
            if (isMoving)
            {
                // Hareket var ve ses çalmýyorsa -> ÇAL
                if (!winchAudio.isPlaying) winchAudio.Play();
                // Pitch ile oynayarak hýzý hissettirebilirsin
                winchAudio.pitch = 1.0f + (winchSpeed * 0.1f);
            }
            else
            {
                // Hareket durduysa -> SUS
                winchAudio.Stop();
            }
        }

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

    // --- NEW: ROPE COLLISION LOGIC ---
    private void CheckRopeCollision()
    {
        // Draw an invisible line from Winch to Hook to see if it hits an 'Obstacle'
        bool isHit = Physics.Linecast(winchOrigin.position, hookTransform.position, obstacleLayer);

        if (isHit)
        {
            // OBSTACLE DETECTED!
            contactTimer += Time.deltaTime; // Increase timer

            // Change color to RED to warn player
            cableVisual.startColor = warningColor;
            cableVisual.endColor = warningColor;

            // If timer exceeds limit -> BREAK THE ROPE
            if (contactTimer > breakTime)
            {
                SnapRope();
            }
        }
        else
        {
            // SAFE
            contactTimer = 0f; // Reset timer
            cableVisual.startColor = normalColor;
            cableVisual.endColor = normalColor;
        }
    }

    private void SnapRope()
    {
        Debug.Log("ROPE SNAPPED! Cable touched an obstacle for too long.");

        // 1. Destroy the joint
        Destroy(joint);
        joint = null;

        // 2. Hide the line
        cableVisual.enabled = false;

        // 3. Drop the cargo (if carrying any)
        if (hookGrabber != null && hookGrabber.isCarrying)
        {
            // We force the hook to drop whatever it holds
            // (You might need to make DropCargo public in CargoGrabber script)
            hookGrabber.SendMessage("DropCargo", SendMessageOptions.DontRequireReceiver);
        }

        // 4. Trigger Game Over (Optional)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }

    private void ApplySwingStabilization()
    {
        if (hookRb == null) return;

        Vector3 directionToHook = (hookTransform.position - winchOrigin.position).normalized;
        float angle = Vector3.Angle(Vector3.down, directionToHook);

        if (angle > maxSwingAngle)
        {
            float angleDifference = angle - maxSwingAngle;
            Vector3 correctionDir = (Vector3.down - directionToHook).normalized;

            hookRb.AddForce(correctionDir * angleDifference * stabilityGain, ForceMode.Acceleration);
            hookRb.linearDamping = Mathf.Lerp(hookRb.linearDamping, stabilityDrag, Time.fixedDeltaTime * 5f);
        }
        else
        {
            hookRb.linearDamping = Mathf.Lerp(hookRb.linearDamping, defaultDrag, Time.fixedDeltaTime * 5f);
        }
    }
}