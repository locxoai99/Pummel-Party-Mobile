using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// WordWarsHUD — Pummel Party style
/// Chữ cần gõ đã di chuyển lên đầu nhân vật (3D) → HUD chỉ còn:
/// - Announcer giữa, Score góc phải, Name góc trái
/// - Joystick + buttons mobile
/// </summary>
public class WordWarsHUD : MonoBehaviour
{
    public static WordWarsHUD Instance { get; private set; }

    private Text   announcerText;
    private Text   scoreText;
    private Text   playerNameText;
    private GameObject announcerPanel;

    static readonly Color NEON_BLUE   = new Color(0.20f, 0.65f, 1f);
    static readonly Color NEON_GREEN  = new Color(0.25f, 1f, 0.50f);
    static readonly Color NEON_RED    = new Color(1f, 0.30f, 0.30f);
    static readonly Color NEON_TEAL   = new Color(0.18f, 0.82f, 0.75f);
    static readonly Color DARK_BG     = new Color(0.05f, 0.07f, 0.12f, 0.88f);

    void Awake() { Instance = this; }
    void Start() { BuildHUD(); }

    void BuildHUD()
    {
        // ── Announcer ───────────────────────────────────────────────────
        {
            announcerPanel = MakePanel("AnnPanel",
                new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.76f), DARK_BG);

            // Neon border bottom
            MakePanel("AnnBorder", new Vector2(0.1f, 0.595f), new Vector2(0.9f, 0.603f), NEON_BLUE);

            var go = new GameObject("AnnText");
            go.transform.SetParent(announcerPanel.transform, false);
            announcerText = go.AddComponent<Text>();
            announcerText.fontSize = 50; announcerText.fontStyle = FontStyle.Bold;
            announcerText.color = Color.white; announcerText.alignment = TextAnchor.MiddleCenter;
            announcerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            FillRect(go);

            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.8f);
            outline.effectDistance = new Vector2(2, -2);

            announcerPanel.SetActive(false);
            if (WordManager.Instance != null)
                WordManager.Instance.announcerText = announcerText;
        }

        // ── Score (góc phải trên) ───────────────────────────────────────
        {
            var go = new GameObject("ScoreCircle");
            go.transform.SetParent(transform, false);
            go.AddComponent<Image>().color = NEON_TEAL;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(1, 1); rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-14, -14); rt.sizeDelta = new Vector2(80, 80);

            var inner = new GameObject("Inner");
            inner.transform.SetParent(go.transform, false);
            inner.AddComponent<Image>().color = DARK_BG;
            var iRT = inner.GetComponent<RectTransform>();
            iRT.anchorMin = Vector2.zero; iRT.anchorMax = Vector2.one;
            iRT.offsetMin = new Vector2(4, 4); iRT.offsetMax = new Vector2(-4, -4);

            scoreText = MakeText(inner.transform, "0", 40, NEON_TEAL);

            var lbl = new GameObject("Lbl");
            lbl.transform.SetParent(go.transform, false);
            var lt = lbl.AddComponent<Text>();
            lt.text = "SCORE"; lt.fontSize = 13; lt.fontStyle = FontStyle.Bold;
            lt.color = new Color(0.5f, 0.6f, 0.7f); lt.alignment = TextAnchor.UpperCenter;
            lt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var lRT = lbl.GetComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0, 0); lRT.anchorMax = new Vector2(1, 0);
            lRT.pivot = new Vector2(0.5f, 1); lRT.anchoredPosition = new Vector2(0, -2);
            lRT.sizeDelta = new Vector2(80, 18);
        }

        // ── Player name (góc trái trên) ─────────────────────────────────
        {
            var go = new GameObject("NamePanel");
            go.transform.SetParent(transform, false);
            go.AddComponent<Image>().color = DARK_BG;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(14, -14); rt.sizeDelta = new Vector2(190, 44);

            // Neon accent left
            var acc = new GameObject("Acc"); acc.transform.SetParent(go.transform, false);
            acc.AddComponent<Image>().color = NEON_BLUE;
            var aRT = acc.GetComponent<RectTransform>();
            aRT.anchorMin = new Vector2(0, 0); aRT.anchorMax = new Vector2(0, 1);
            aRT.pivot = new Vector2(0, 0.5f); aRT.anchoredPosition = Vector2.zero;
            aRT.sizeDelta = new Vector2(4, 0);

            var t = new GameObject("Txt"); t.transform.SetParent(go.transform, false);
            playerNameText = t.AddComponent<Text>();
            playerNameText.text = "Player 1"; playerNameText.fontSize = 24;
            playerNameText.fontStyle = FontStyle.Bold;
            playerNameText.color = new Color(0.85f, 0.9f, 1f);
            playerNameText.alignment = TextAnchor.MiddleCenter;
            playerNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var tRT = t.GetComponent<RectTransform>();
            tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
            tRT.offsetMin = new Vector2(12, 0); tRT.offsetMax = Vector2.zero;
        }

        // ── Hint ────────────────────────────────────────────────────────
        {
            var go = new GameObject("Hint"); go.transform.SetParent(transform, false);
            var t = go.AddComponent<Text>();
            t.text = "E = SELECT    F = PUNCH    SPACE = JUMP";
            t.fontSize = 16; t.color = new Color(0.45f, 0.5f, 0.6f, 0.6f);
            t.alignment = TextAnchor.MiddleCenter;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.15f, 0); rt.anchorMax = new Vector2(0.85f, 0);
            rt.pivot = new Vector2(0.5f, 0); rt.anchoredPosition = new Vector2(0, 6);
            rt.sizeDelta = new Vector2(0, 26);
        }

        BuildJoystick();
        BuildActionButtons();
    }

    // ── Helpers ──────────────────────────────────────────────────────────
    GameObject MakePanel(string name, Vector2 aMin, Vector2 aMax, Color color)
    {
        var go = new GameObject(name); go.transform.SetParent(transform, false);
        go.AddComponent<Image>().color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }

    Text MakeText(Transform parent, string text, int size, Color color)
    {
        var go = new GameObject("Txt"); go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.text = text; t.fontSize = size; t.fontStyle = FontStyle.Bold;
        t.color = color; t.alignment = TextAnchor.MiddleCenter;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        FillRect(go); return t;
    }

    void FillRect(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    void BuildJoystick()
    {
        var hint = new GameObject("JoyHint"); hint.transform.SetParent(transform, false);
        hint.AddComponent<Image>().color = new Color(0.4f, 0.5f, 0.6f, 0.12f);
        var hr = hint.GetComponent<RectTransform>();
        hr.anchorMin = hr.anchorMax = Vector2.zero; hr.pivot = Vector2.zero;
        hr.anchoredPosition = new Vector2(28, 28); hr.sizeDelta = new Vector2(145, 145);

        var bgGO = new GameObject("JoystickBG"); bgGO.transform.SetParent(transform, false);
        bgGO.AddComponent<Image>().color = new Color(0.3f, 0.4f, 0.5f, 0.4f);
        var bgCG = bgGO.AddComponent<CanvasGroup>(); bgCG.alpha = 0; bgCG.blocksRaycasts = false;
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = bgRT.anchorMax = Vector2.zero; bgRT.pivot = new Vector2(0.5f, 0.5f);
        bgRT.sizeDelta = new Vector2(145, 145); bgRT.anchoredPosition = new Vector2(100, 100);

        var hdGO = new GameObject("JoystickHandle"); hdGO.transform.SetParent(bgGO.transform, false);
        hdGO.AddComponent<Image>().color = new Color(0.6f, 0.7f, 0.8f, 0.8f);
        hdGO.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);

        var ta = new GameObject("JoystickTouchArea"); ta.transform.SetParent(transform, false);
        ta.AddComponent<Image>().color = Color.clear;
        ta.GetComponent<Image>().raycastTarget = true;
        var taRT = ta.GetComponent<RectTransform>();
        taRT.anchorMin = Vector2.zero; taRT.anchorMax = new Vector2(0.38f, 0.38f);
        taRT.offsetMin = taRT.offsetMax = Vector2.zero;

        var js = ta.AddComponent<FloatingJoystick>(); js.handleRange = 68f;
        var pc = FindObjectOfType<PlayerController>();
        if (pc) pc.joystick = js;
    }

    void BuildActionButtons()
    {
        var pws = FindObjectOfType<PlayerWordSpeller>();
        var pc  = FindObjectOfType<PlayerController>();

        MakeBtn("SelectBtn", new Vector2(-28, 28), new Vector2(148, 148), NEON_GREEN, "SELECT", 24,
            pws != null ? new UnityEngine.Events.UnityAction(pws.OnSelectButtonPressed) : null);
        MakeBtn("PunchBtn", new Vector2(-190, 28), new Vector2(108, 108), NEON_RED, "PUNCH", 20,
            pws != null ? new UnityEngine.Events.UnityAction(pws.OnPunchButtonPressed) : null);
        MakeBtn("JumpBtn", new Vector2(-190, 150), new Vector2(100, 100), NEON_BLUE, "JUMP", 20,
            pc != null ? new UnityEngine.Events.UnityAction(pc.OnJumpButtonPressed) : null);
    }

    void MakeBtn(string name, Vector2 pos, Vector2 size, Color color, string label, int fontSize,
        UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(name); go.transform.SetParent(transform, false);
        go.AddComponent<Image>().color = new Color(color.r, color.g, color.b, 0.22f);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1, 0); rt.pivot = new Vector2(1, 0);
        rt.anchoredPosition = pos; rt.sizeDelta = size;

        var inner = new GameObject("In"); inner.transform.SetParent(go.transform, false);
        inner.AddComponent<Image>().color = new Color(color.r*0.65f, color.g*0.65f, color.b*0.65f, 0.88f);
        var iRT = inner.GetComponent<RectTransform>();
        iRT.anchorMin = Vector2.zero; iRT.anchorMax = Vector2.one;
        iRT.offsetMin = new Vector2(4, 4); iRT.offsetMax = new Vector2(-4, -4);

        var btn = go.AddComponent<Button>();
        if (onClick != null) btn.onClick.AddListener(onClick);

        var lbl = new GameObject("Lbl"); lbl.transform.SetParent(inner.transform, false);
        var t = lbl.AddComponent<Text>();
        t.text = label; t.fontSize = fontSize; t.fontStyle = FontStyle.Bold;
        t.color = Color.white; t.alignment = TextAnchor.MiddleCenter;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        FillRect(lbl);
        var ol = lbl.AddComponent<Outline>(); ol.effectColor = new Color(0,0,0,0.6f);
        ol.effectDistance = new Vector2(1.5f, -1.5f);
    }

    // ── Public APIs ─────────────────────────────────────────────────────
    public void UpdateProgress(PlayerWordSpeller s, string w, int p, bool stunned)
    {
        if (playerNameText) playerNameText.text = s.playerName;
    }

    public void UpdateScore(int score)
    {
        if (scoreText) scoreText.text = score.ToString();
    }

    public void ShowAnnouncer(string msg, float dur)
    {
        if (!announcerText) return;
        StopCoroutine("HideAnn");
        announcerText.text = msg;
        announcerPanel.SetActive(true);
        StartCoroutine(HideAnn(dur));
    }

    IEnumerator HideAnn(float t)
    {
        yield return new WaitForSeconds(t);
        if (announcerPanel) announcerPanel.SetActive(false);
    }
}
