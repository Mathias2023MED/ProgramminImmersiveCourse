using UnityEngine;

public class DartboardCelebration : MonoBehaviour
{
    [Header("Objects to reveal")]
    public GameObject beerObject;      // your existing beer (set inactive at start)
    public GameObject posterObject;    // your existing poster (set inactive at start)

    [Header("Limits (optional)")]
    public bool oneShot = false;       // only first hit reveals
    public float cooldown = 0f;        // seconds between reveals

    float nextAllowedTime;
    bool fired;

    // Hook this to the dart's onStuck event in Inspector
    public void Celebrate(Vector3 hitPoint, Vector3 hitNormal, Transform board)
    {
        if (Time.time < nextAllowedTime) return;
        if (oneShot && fired) return;

        if (posterObject && !posterObject.activeSelf)
            posterObject.SetActive(true);

        if (beerObject && !beerObject.activeSelf)
            beerObject.SetActive(true);

        fired = true;
        nextAllowedTime = Time.time + Mathf.Max(0f, cooldown);
    }
}
