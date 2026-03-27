using UnityEngine;

/// <summary>
/// Đặt một plane trigger dưới map.
/// Khi Player rơi xuống qua đây → respawn về điểm bắt đầu.
/// </summary>
public class RespawnZone : MonoBehaviour
{
    [Header("Respawn Point")]
    public Vector3 respawnPoint = new Vector3(0f, 2f, 0f);

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Reset vị trí
        other.transform.position = respawnPoint;

        // Reset velocity
        var rb = other.GetComponent<Rigidbody>();
        if (rb != null) rb.velocity = Vector3.zero;

        Debug.Log("Respawn!");
    }
}
