using UnityEngine;

public class KeyTile3D : MonoBehaviour
{
    public char     letter;
    public Color    neonColor;
    public Color    tileBase;
    public Material baseMat;

    private bool  isHighlighted = false;
    private bool  isCorrect     = false;
    private float pulseTimer    = 0f;

    private Vector3 originalPos;
    private float   pressDepth   = 0.08f;
    private float   currentPress = 0f;
    private float   pressSpeed   = 12f;

    private Renderer[] neonRenderers;
    private float baseEmission = 2.2f;

    void Start()
    {
        originalPos = transform.localPosition;
        var list = new System.Collections.Generic.List<Renderer>();
        foreach (Transform child in transform)
        {
            if (child.name == "Neon")
            {
                var r = child.GetComponent<Renderer>();
                if (r != null) list.Add(r);
            }
        }
        neonRenderers = list.ToArray();
    }

    void Update()
    {
        float targetPress = isHighlighted ? pressDepth : 0f;
        currentPress = Mathf.Lerp(currentPress, targetPress, Time.deltaTime * pressSpeed);
        transform.localPosition = originalPos + new Vector3(0, -currentPress, 0);

        if (isHighlighted && !isCorrect)
        {
            pulseTimer += Time.deltaTime * 3.5f;
            float pulse = 0.75f + Mathf.Sin(pulseTimer) * 0.25f;
            if (baseMat)
                baseMat.color = Color.Lerp(tileBase, new Color(0.20f, 0.28f, 0.48f), pulse);
            SetNeonEmission(baseEmission + 2.5f * pulse);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        KeyboardMap.Instance?.OnPlayerEnterTile(this);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        KeyboardMap.Instance?.OnPlayerExitTile(this);
    }

    public void SetHighlight(bool on)
    {
        isHighlighted = on; pulseTimer = 0f;
        if (!on && !isCorrect)
        {
            if (baseMat) baseMat.color = tileBase;
            SetNeonEmission(baseEmission);
        }
    }

    public void SetCorrect()
    {
        isCorrect = true; isHighlighted = false;
        Color green = new Color(0.15f, 1f, 0.35f);
        if (baseMat) baseMat.color = new Color(0.06f, 0.32f, 0.10f);
        foreach (var r in neonRenderers)
        {
            if (r == null) continue;
            r.material.color = green;
            r.material.SetColor("_EmissionColor", green * 2.5f);
        }
        var label = transform.Find("Label");
        if (label != null) { var tm = label.GetComponent<TextMesh>(); if (tm) tm.color = green; }
    }

    public void ResetState()
    {
        isCorrect = false; isHighlighted = false;
        if (baseMat) baseMat.color = tileBase;
        foreach (var r in neonRenderers)
        {
            if (r == null) continue;
            r.material.color = neonColor;
            r.material.SetColor("_EmissionColor", neonColor * baseEmission);
        }
        var label = transform.Find("Label");
        if (label != null) { var tm = label.GetComponent<TextMesh>(); if (tm) tm.color = neonColor; }
    }

    void SetNeonEmission(float intensity)
    {
        if (neonRenderers == null) return;
        foreach (var r in neonRenderers)
        {
            if (r == null) continue;
            r.material.SetColor("_EmissionColor", neonColor * intensity);
        }
    }
}
