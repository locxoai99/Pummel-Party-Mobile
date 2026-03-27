using UnityEngine;

/// <summary>
/// Gắn lên các cube có Rigidbody.
/// Khi Player đụng vào → cube bị hất văng, Player KHÔNG bị reset velocity.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class KnockbackObject : MonoBehaviour
{
    public float knockbackForce = 12f;
    public float upwardForce    = 5f;

    private Rigidbody rb;
    private Renderer  rend;
    private Color     origColor;
    private float     flashTimer;

    void Awake()
    {
        rb   = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        if (rend != null) origColor = rend.material.color;

        rb.mass        = 1f;
        rb.drag        = 0.8f;
        rb.angularDrag = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Update()
    {
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f && rend != null)
                rend.material.color = origColor;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (!col.gameObject.CompareTag("Player")) return;

        // Tính hướng từ player → cube (chỉ ngang)
        Vector3 dir = transform.position - col.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) dir = Vector3.forward;
        dir.Normalize();

        // Chỉ reset velocity của CUBE, không động đến Player
        rb.velocity = Vector3.zero;
        rb.AddForce(dir * knockbackForce + Vector3.up * upwardForce, ForceMode.Impulse);

        // Flash đỏ
        if (rend != null)
        {
            rend.material.color = Color.red;
            flashTimer = 0.25f;
        }
    }
}
