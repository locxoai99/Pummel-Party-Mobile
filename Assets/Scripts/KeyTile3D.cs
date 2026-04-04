using UnityEngine;

public class KeyTile3D : MonoBehaviour
{
    public char letter;
    public Color neonColor;

    public Material baseMat;
    public Color tileBase;

    private Renderer[] neonRenderers;

    private bool isHighlighted = false;
    private bool isCorrect = false;

    private float baseEmission = 1.2f;

    private Vector3 originalScale;
    private Vector3 originalPos;
    private float swingTimer = 0f;
    private bool isSwinging = false;
    private float swingDuration = 1.5f;
    private float swingAngle = 12f;
    void Update()
    {
        if (isSwinging)
        {
            swingTimer += Time.deltaTime;
            float t = swingTimer / swingDuration;
            float angle = swingAngle * Mathf.Sin(t * Mathf.PI * 4f) * (1f - t);
            transform.localRotation = Quaternion.Euler(0, 0, angle);
            if (t >= 1f)
            {
                isSwinging = false;
                transform.localRotation = Quaternion.identity;
            }
        }
    }

    public void StartSwing()
    {
        isSwinging = true;
        swingTimer = 0f;
    }
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        KeyboardMap.Instance?.OnPlayerEnterTile(this);
        StartSwing();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        KeyboardMap.Instance?.OnPlayerExitTile(this);
    }
    void Awake()
    {
        originalScale = transform.localScale;
        originalPos = transform.localPosition;

        // Lấy toàn bộ renderer có tên "Neon"
        var rends = GetComponentsInChildren<Renderer>();
        var list = new System.Collections.Generic.List<Renderer>();

        foreach (var r in rends)
        {
            if (r.gameObject.name.Contains("Neon"))
                list.Add(r);
        }

        neonRenderers = list.ToArray();
    }

    void SetNeonEmission(float intensity)
    {
        foreach (var r in neonRenderers)
        {
            if (r == null) continue;

            var mat = r.material;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", neonColor * intensity);
        }
    }

    // =============================
    // 🔵 HIGHLIGHT (đứng lên ô)
    // =============================
    public void SetHighlight(bool on)
    {
        isHighlighted = on;

        if (on && !isCorrect)
        {
            // sáng nhẹ body
            if (baseMat)
            {
                baseMat.color = new Color(
                    Mathf.Min(tileBase.r + 0.08f, 1f),
                    Mathf.Min(tileBase.g + 0.08f, 1f),
                    Mathf.Min(tileBase.b + 0.08f, 1f)
                );
            }

            // tăng glow
            SetNeonEmission(baseEmission * 1.8f);

            // scale + nổi lên
            transform.localScale = originalScale * 1.08f;
            transform.localPosition = originalPos + new Vector3(0f, 0.025f, 0f);

            // chữ sáng hơn
            var label = transform.Find("Label");
            if (label != null)
            {
                var tm = label.GetComponent<TextMesh>();
                if (tm) tm.color = neonColor * 2f;
            }
        }
        else if (!on && !isCorrect)
        {
            if (baseMat) baseMat.color = tileBase;

            SetNeonEmission(baseEmission);

            transform.localScale = originalScale;
            transform.localPosition = originalPos;

            var label = transform.Find("Label");
            if (label != null)
            {
                var tm = label.GetComponent<TextMesh>();
                if (tm) tm.color = neonColor;
            }
        }
    }

    // =============================
    // 🟢 ĐÚNG
    // =============================
    public void SetCorrect()
    {
        isCorrect = true;
        isHighlighted = false;

        Color green = new Color(0.15f, 1f, 0.35f);

        if (baseMat)
            baseMat.color = new Color(0.06f, 0.32f, 0.10f);

        foreach (var r in neonRenderers)
        {
            if (r == null) continue;

            var mat = r.material;
            mat.color = green;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", green * 4f);
        }

        var label = transform.Find("Label");
        if (label != null)
        {
            var tm = label.GetComponent<TextMesh>();
            if (tm) tm.color = green * 2f;
        }

        transform.localScale = originalScale * 1.12f;
        transform.localPosition = originalPos + new Vector3(0f, 0.04f, 0f);
    }

    // =============================
    // 🔄 RESET
    // =============================
    public void ResetState()
    {
        isCorrect = false;
        isHighlighted = false;

        if (baseMat) baseMat.color = tileBase;

        foreach (var r in neonRenderers)
        {
            if (r == null) continue;

            var mat = r.material;
            mat.color = neonColor;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", neonColor * baseEmission);
        }

        var label = transform.Find("Label");
        if (label != null)
        {
            var tm = label.GetComponent<TextMesh>();
            if (tm) tm.color = neonColor;
        }

        transform.localScale = originalScale;
        transform.localPosition = originalPos;
    }
}