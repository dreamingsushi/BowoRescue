using UnityEngine;

public class AOEIndicator : MonoBehaviour
{
    public float growDuration = 1.5f;
    public float maxScale = 3f;
    public float delayBeforeDestroy = 0.5f;

    private float timer = 0f;
    private Vector3 initialScale;

    void Start()
    {
        initialScale = Vector3.zero;
        transform.localScale = initialScale;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer < growDuration)
        {
            float t = timer / growDuration;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * maxScale, t);
        }
        else
        {
            // Optional: destroy after visual ends
            Destroy(gameObject, delayBeforeDestroy);
        }
    }
}
