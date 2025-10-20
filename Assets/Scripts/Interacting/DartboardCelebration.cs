using UnityEngine;

public class DartboardCelebration : MonoBehaviour
{
    [Header("Reveal on dart hit")]
    public GameObject beerObject;      // your existing beer (set inactive at start)

    [Header("Limits (optional)")]
    public bool oneShot = false;
    public float cooldown = 0f;

    float nextAllowedTime;
    bool fired;

    // Hook this to DartStickOnHit.onStuck(point, normal, board)
    public void Celebrate(Vector3 hitPoint, Vector3 hitNormal, Transform board)
    {
        if (Time.time < nextAllowedTime) return;
        if (oneShot && fired) return;

        if (beerObject && !beerObject.activeSelf)
            beerObject.SetActive(true);

        fired = true;
        nextAllowedTime = Time.time + Mathf.Max(0f, cooldown);
    }
}
