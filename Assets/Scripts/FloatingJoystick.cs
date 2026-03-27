using UnityEngine;
using UnityEngine.EventSystems;

public class FloatingJoystick : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public float handleRange = 80f;

    public float Horizontal { get; private set; }
    public float Vertical   { get; private set; }

    private RectTransform bgRect;
    private RectTransform handleRect;
    private CanvasGroup   bgGroup;
    private Canvas        canvas;
    private Vector2       anchorPos;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();

        // Tìm JoystickBG theo tên trong Canvas
        var bg = canvas.transform.Find("JoystickBG");
        if (bg != null)
        {
            bgRect  = bg.GetComponent<RectTransform>();
            bgGroup = bg.GetComponent<CanvasGroup>();
            var hd  = bg.Find("JoystickHandle");
            if (hd != null) handleRect = hd.GetComponent<RectTransform>();
        }

        if (bgRect    == null) Debug.LogError("❌ Không tìm thấy JoystickBG!");
        if (handleRect == null) Debug.LogError("❌ Không tìm thấy JoystickHandle!");

        Hide();
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (bgRect == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform, e.position, GetCam(), out anchorPos);
        bgRect.anchoredPosition     = anchorPos;
        handleRect.anchoredPosition = Vector2.zero;
        Show();
        OnDrag(e);
    }

    public void OnDrag(PointerEventData e)
    {
        if (bgRect == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform, e.position, GetCam(), out Vector2 pos);
        Vector2 clamped = Vector2.ClampMagnitude(pos - anchorPos, handleRange);
        handleRect.anchoredPosition = clamped;
        Horizontal = clamped.x / handleRange;
        Vertical   = clamped.y / handleRange;
    }

    public void OnPointerUp(PointerEventData e)
    {
        Horizontal = Vertical = 0f;
        if (handleRect != null) handleRect.anchoredPosition = Vector2.zero;
        Hide();
    }

    void Show() { if (bgGroup != null) bgGroup.alpha = 1f; }
    void Hide() { if (bgGroup != null) bgGroup.alpha = 0f; }
    Camera GetCam() => canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
}
