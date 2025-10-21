using UnityEngine;

public class DartboardCelebration : MonoBehaviour
{
    [Header("Reveal on dart hit")]
    [Tooltip("Existing beer object in the scene (set inactive at start if you want it to 'appear').")]
    public GameObject beerObject;

    [Header("Limits (optional)")]
    [Tooltip("If true, only the first successful hit will reveal the beer.")]
    public bool oneShot = false;

    [Tooltip("Minimum seconds between reveals (0 = no cooldown).")]
    public float cooldown = 0f;

    private float nextAllowedTime = 0f;
    private bool firedOnce = false;

    /// <summary>
    /// Hook this up to the dart's onStuck event: Celebrate(hitPoint, hitNormal, boardTransform)
    /// </summary>
    public void Celebrate(Vector3 hitPoint, Vector3 hitNormal, Transform board)
    {
        if (Time.time < nextAllowedTime) return;
        if (oneShot && firedOnce) return;

        // reveal beer if it's currently hidden
        if (beerObject != null && !beerObject.activeSelf)
        {
            // if your beer has BeerExplodeOnHit with HasExploded, skip re-activating exploded items
            var explode = beerObject.GetComponent<BeerExplodeOnHit>();
            if (explode == null || !explode.HasExploded)
                beerObject.SetActive(true);
        }

        firedOnce = true;
        nextAllowedTime = Time.time + Mathf.Max(0f, cooldown);
    }
}

