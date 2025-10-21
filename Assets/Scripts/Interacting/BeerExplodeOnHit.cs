using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class BeerExplodeOnHit : MonoBehaviour
{
    [Header("Valid hit layers (e.g., Poster, or Poster + Dartboard)")]
    public LayerMask explodeLayers;

    [Header("Arming & impact")]
    public float minImpactSpeed = 2.0f;
    public float armDelayAfterRelease = 0.08f;

    [Header("Fire effect (existing object in scene)")]
    public GameObject fireObject;         // drag disabled "Fire" from Hierarchy
    public float fireSurfaceOffset = 0.02f;

    [Header("Audio (optional)")]
    public AudioClip popClip;             // assign a clip if you want a one-shot
    public float popVolume = 1f;

    [Header("XR Grab (optional)")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;       // auto-found if left empty
    public Transform beerGrabPoint;       // drag your BeerGrabPoint if you have one

    public bool HasExploded { get; private set; }

    Rigidbody rb;
    bool isHeld;
    bool armed;
    float armAtTime;

    void Reset()
    {
        rb = GetComponent<Rigidbody>();
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!grab) grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (grab)
        {
            if (beerGrabPoint) grab.attachTransform = beerGrabPoint;

            grab.selectEntered.AddListener(_ =>
            {
                isHeld = true;
                armed = false;
            });

            grab.selectExited.AddListener(_ =>
            {
                isHeld = false;
                armAtTime = Time.time + armDelayAfterRelease;
            });
        }

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        if (!isHeld && !armed && Time.time >= armAtTime)
            armed = true;
    }

    void OnCollisionEnter(Collision col)
    {
        if (!armed || HasExploded) return;
        if ((explodeLayers.value & (1 << col.gameObject.layer)) == 0) return;

        if (col.relativeVelocity.magnitude < minImpactSpeed) return;

        Explode(col);
    }

    void Explode(Collision col)
    {
        HasExploded = true;
        armed = false;

        // Play one-shot audio that survives even if we disable the beer
        if (popClip)
            AudioSource.PlayClipAtPoint(popClip, transform.position, popVolume);

        // Place + start FIRE
        if (fireObject)
        {
            var cp = col.GetContact(0);
            Vector3 pos = cp.point + cp.normal * fireSurfaceOffset;
            Quaternion rot = Quaternion.LookRotation(cp.normal, Vector3.up);

            fireObject.transform.SetPositionAndRotation(pos, rot);

            if (!fireObject.activeSelf)
                fireObject.SetActive(true);

            // Force the animation to start from frame 0 even if it was played before
            var anim = fireObject.GetComponent<Animator>();
            if (anim)
            {
                anim.Rebind();
                anim.Update(0f);
                anim.Play(0, -1, 0f); // default layer, from start
            }
        }

        // Safety: release if still selected
        if (grab && grab.isSelected)
            foreach (var interactor in grab.interactorsSelecting)
                grab.interactionManager?.SelectExit(interactor, grab);

        // STOP rendering & collisions (don�t destroy)
        foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = false;
        foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = false;

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (grab) grab.enabled = false;

        // If you want the whole object gone from the hierarchy, uncomment:
        // gameObject.SetActive(false);
    }
}
