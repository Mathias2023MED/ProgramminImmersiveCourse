using UnityEngine;


public class RevealPosterOnBeerPickup : MonoBehaviour
{
    [Header("Poster to reveal or spawn")]
    public GameObject posterObject;          // existing poster in scene (start disabled)
    public GameObject posterPrefab;          // optional: spawn this if posterObject is null
    public Transform boardTransform;         // the dartboard transform (for placement if spawning)

    [Header("Placement if spawning a poster")]
    public float distanceFromBoard = 0.08f;  // meters in front of board
    public Vector3 eulerOffset;              // fine rotation tweak
    public bool parentToBoard = true;

    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    bool done;

    void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grab == null)
            Debug.LogWarning("RevealPosterOnBeerPickup needs XRGrabInteractable on the same GameObject.");
        else
            grab.selectEntered.AddListener(_ => OnPickedUp());
    }

    void OnPickedUp()
    {
        if (done) return;
        done = true;

        // Option A: enable existing poster
        if (posterObject != null)
        {
            posterObject.SetActive(true);
            return;
        }

        // Option B: spawn a poster prefab in front of the board
        if (posterPrefab != null && boardTransform != null)
        {
            Vector3 pos = boardTransform.position + boardTransform.forward * distanceFromBoard;
            Quaternion rot = Quaternion.LookRotation(boardTransform.forward, Vector3.up) *
                             Quaternion.Euler(eulerOffset);

            var poster = Instantiate(posterPrefab, pos, rot);
            if (parentToBoard) poster.transform.SetParent(boardTransform, true);
        }
        else
        {
            Debug.LogWarning("No posterObject assigned and/or posterPrefab+boardTransform missing.");
        }
    }
}
