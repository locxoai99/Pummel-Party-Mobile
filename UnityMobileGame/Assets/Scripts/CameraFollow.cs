using UnityEngine;

/// <summary>
/// CameraFollow — Camera CỐ ĐỊNH, không follow player
/// Chỉnh vị trí/góc trong Inspector hoặc WordWarsSetup
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3   offset    = new Vector3(0f, 16f, -14f);
    public float     smoothTime = 0.15f;

    // Giữ field để WordWarsSetup không lỗi compile
    // Nhưng không follow — camera đứng yên
}