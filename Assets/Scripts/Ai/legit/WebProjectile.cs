using UnityEngine;
using System.Collections;

public class WebProjectile : MonoBehaviour
{
    public float speed = 8f;
    public float fadeDuration = 0.4f;

    private Transform target;
    private Renderer rend;
    private bool arriving = false;

    public void Launch(Transform from, Transform to)
    {
        transform.position = from.position;
        target = to;
        rend = GetComponent<Renderer>();
        StartCoroutine(Travel());
    }

    IEnumerator Travel()
    {
        while (Vector3.Distance(transform.position, target.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target.position,
                speed * Time.deltaTime
            );
            transform.LookAt(target.position);
            yield return null;
        }

        yield return StartCoroutine(FadeAndDestroy());
    }

    IEnumerator FadeAndDestroy()
    {
        // Make sure material is transparent-capable
        Color c = rend.material.color;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = 1f - (t / fadeDuration);
            rend.material.color = c;
            yield return null;
        }
        Destroy(gameObject);
    }
}
