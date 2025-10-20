using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class BeerExplodeOnHit : MonoBehaviour
{
    [Header("Explode when hitting these layers (e.g., Poster)")]
    public LayerMask explodeLayers;

    [Header("Impact gating")]
    public float minImpactSpeed = 2.0f;      // m/s required to explode
    public float armDelayAfterRelease = 0.05f;// small delay after release

    [Header("Effects (optional)")]
    public ParticleSystem popParticles;      // assign a simple burst
    public AudioSource popAudio;             // optional sound
    public float cleanupDelay = 2f;          // optional delayed cleanup/respawn

    Rigidbody rb;
    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    bool isHeld;
    bool armed;
    float armAtTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (grab)
        {
            grab.selectEntered.AddListener(_ => { isHeld = true; armed = false; });
            grab.selectExited.AddListener(_ =>
            {
                isHeld = false;
                armAtTime = Time.time + armDelayAfterRelease;
            });
        }
    }

    void Update()
    {
        // Arm once we've been released and the small delay has passed
        if (!isHeld && !armed && Time.time >= armAtTime)
            armed = true;
    }

    void OnCollisionEnter(Collision col)
    {
        if (!armed) return;
        if ((explodeLayers.value & (1 << col.gameObject.layer)) == 0) return;

        // Use relative speed for robustness
        float speed = col.relativeVelocity.magnitude;
        if (speed < minImpactSpeed) return;

        Explode(col);
    }

    void Explode(Collision col)
    {
        armed = false;

        // Force release if still somehow selected
        if (grab && grab.isSelected)
            foreach (var interactor in grab.interactorsSelecting)
                grab.interactionManager?.SelectExit(interactor, grab);

        // Play FX
        if (popParticles)
        {
            var cp = col.GetContact(0);
            popParticles.transform.SetPositionAndRotation(cp.point, Quaternion.LookRotation(cp.normal));
            popParticles.Play();
        }
        if (popAudio) popAudio.Play();

        // Hide visuals + stop further collisions
        foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = false;
        foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = false;

        // Freeze physics
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // (Optional) disable grabbing now that it's "broken"
        if (grab) grab.enabled = false;

        if (cleanupDelay > 0f)
            Invoke(nameof(AfterExplodeCleanup), cleanupDelay);
    }

    void AfterExplodeCleanup()
    {
        // Choose one: destroy, pool, or reset visuals.
        // Destroy(gameObject);

        // Example: reset to be ready again (if you�re not destroying)
        foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = true;
        foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = true;
        if (grab) grab.enabled = true;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        armed = false; isHeld = false;
        // You can also reposition the beer to a rack/spawn point here.
        gameObject.SetActive(true);
    }
}
