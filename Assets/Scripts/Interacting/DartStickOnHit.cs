using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(Rigidbody))]
public class DartStickOnHit : MonoBehaviour
{
    // ---- Public config ----
    [Header("Stick Conditions")]
    [Tooltip("Only stick to these layers (e.g., Dartboard).")]
    public LayerMask stickLayers;

    [Tooltip("Minimum speed (m/s) required on impact to stick.")]
    public float minSpeed = 0.2f;

    [Tooltip("Max angle (deg) between the dart tip direction and the board normal at impact.")]
    public float maxAngleFromNormal = 85f;

    [Header("Placement")]
    [Tooltip("How far to embed the tip into the board (meters).")]
    public float embedDepth = 0.02f;

    [Tooltip("Parent the dart to the target after sticking.")]
    public bool parentToTarget = true;

    [Tooltip("If your model's tip points opposite of +Z, add a 180� flip around Y on stick.")]
    public bool flipForwardBy180 = false;

    [Header("XR / Interactable (optional)")]
    [Tooltip("If present, will be disabled while stuck and force-released on impact.")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    // ---- Events ----
    [System.Serializable]
    public class StickEvent : UnityEvent<Vector3, Vector3, Transform> { } // (hitPoint, hitNormal, target)
    [Header("Events")]
    public StickEvent onStuck;

    // ---- Internals ----
    Rigidbody rb;
    bool stuck;

    void Reset()
    {
        rb = GetComponent<Rigidbody>();
        if (!grab) grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!grab) grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // Sensible defaults if missing
        if (rb) rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void OnCollisionEnter(Collision col)
    {
        if (stuck) return;

        // Must be a valid layer
        if ((stickLayers.value & (1 << col.gameObject.layer)) == 0) return;

        // Gather impact data
        ContactPoint contact = col.GetContact(0);
        Vector3 hitPoint = contact.point;
        Vector3 hitNormal = contact.normal;
        float speed = rb ? rb.linearVelocity.magnitude : 0f;

        // Check gates: speed + approach angle
        if (speed < minSpeed) return;

        // We assume the dart's TIP points along +Z (transform.forward).
        // Angle between dart tip direction and board normal should be small-ish.
        float tipVsNormalAngle = Vector3.Angle(-transform.forward, hitNormal);
        if (tipVsNormalAngle > maxAngleFromNormal) return;

        // All good � stick it
        Stick(hitPoint, hitNormal, col.transform);
    }

    void Stick(Vector3 hitPoint, Vector3 hitNormal, Transform target)
    {
        stuck = true;

        // If being held, force a clean release
        if (grab != null && grab.isSelected)
        {
            foreach (var interactor in grab.interactorsSelecting)
            {
                grab.interactionManager?.SelectExit(interactor, grab);
            }
        }

        // Stop physics
        if (rb)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }

        // Orient dart so its tip goes into the board
        Quaternion rot = Quaternion.LookRotation(-hitNormal, Vector3.up);
        if (flipForwardBy180) rot *= Quaternion.Euler(0f, 180f, 0f); // use if your model faces -Z
        transform.rotation = rot;

        // Position so the tip is embedded slightly
        transform.position = hitPoint - transform.forward * embedDepth;

        // Optional: parent to the hit object so it moves with the board
        if (parentToTarget && target != null)
            transform.SetParent(target, true);

        // Make it non-grabbable while stuck (optional)
        if (grab) grab.enabled = false;

        // >>> FIRE EVENT HERE (let others react: confetti, poster/beer reveal, etc.)
        onStuck?.Invoke(hitPoint, hitNormal, target);
    }

    // Optional: call this to pull the dart back out
    public void Unstick()
    {
        if (!stuck) return;
        stuck = false;

        transform.SetParent(null, true);

        if (grab) grab.enabled = true;

        if (rb)
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }
}
