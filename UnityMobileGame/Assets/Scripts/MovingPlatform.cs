using UnityEngine;

/// <summary>
/// Gắn lên Platform để nó di chuyển qua lại giữa 2 điểm.
/// Player đứng trên sẽ được carry theo.
/// </summary>
public class MovingPlatform : MonoBehaviour
{
    [Header("Movement")]
    public Vector3 moveOffset   = new Vector3(6f, 0f, 0f); // di chuyển bao xa
    public float   speed        = 2.5f;
    public bool    pingPong     = true;

    private Vector3 startPos;
    private Vector3 endPos;
    private float   t = 0f;
    private int     dir = 1;

    void Start()
    {
        startPos = transform.position;
        endPos   = startPos + moveOffset;
    }

    void FixedUpdate()
    {
        t += Time.fixedDeltaTime * speed * dir;

        if (pingPong)
        {
            if (t >= 1f) { t = 1f; dir = -1; }
            if (t <= 0f) { t = 0f; dir =  1; }
        }
        else
        {
            t = Mathf.Repeat(t, 1f);
        }

        Vector3 newPos = Vector3.Lerp(startPos, endPos, EaseInOut(t));
        Vector3 delta  = newPos - transform.position;
        transform.position = newPos;

        // Carry player nếu đứng trên platform
        foreach (var hit in Physics.OverlapBox(
            transform.position + Vector3.up * 0.6f,
            new Vector3(transform.localScale.x * 0.48f, 0.1f, transform.localScale.z * 0.48f)))
        {
            if (hit.CompareTag("Player"))
                hit.transform.position += delta;
        }
    }

    float EaseInOut(float x) => x * x * (3f - 2f * x);
}
