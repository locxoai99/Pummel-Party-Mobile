using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// PlayerWordSpeller — Từng chữ cái trên đầu là TextMesh RIÊNG BIỆT
/// → đổi màu từng chữ khi select: xanh lá = đã gõ, vàng = tiếp theo, trắng mờ = chưa
/// </summary>
public class PlayerWordSpeller : MonoBehaviour
{
    [Header("Player Info")]
    public string playerName = "Player 1";

    [Header("Punch")]
    public float punchRadius   = 2.5f;
    public float punchForce    = 14f;
    public float punchCooldown = 0.8f;
    public KeyCode selectKey   = KeyCode.E;

    [Header("Head Display")]
    public float headOffset    = 3.2f;
    public float letterSpacing = 0.45f;  // khoảng cách giữa mỗi chữ

    private string targetWord = "";
    private int    progress   = 0;
    private bool   stunned    = false;
    private float  stunTimer  = 0f;
    private float  punchTimer = 0f;
    private Rigidbody rb;

    // Head display
    private GameObject headDisplayGO;
    private List<TextMesh> letterMeshes = new List<TextMesh>();
    private TextMesh stunnedText;

    // Colors
    static readonly Color COL_DONE    = new Color(0.25f, 1f, 0.45f);    // xanh lá — đã gõ đúng
    static readonly Color COL_NEXT    = new Color(1f, 0.92f, 0.22f);    // vàng — chữ tiếp theo cần gõ
    static readonly Color COL_PENDING = new Color(0.55f, 0.58f, 0.68f); // xám nhạt — chưa tới
    static readonly Color COL_STUNNED = new Color(1f, 0.28f, 0.28f);    // đỏ

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        CreateHeadContainer();
    }

    void CreateHeadContainer()
    {
        headDisplayGO = new GameObject("HeadWordDisplay");
        headDisplayGO.transform.SetParent(transform);
        headDisplayGO.transform.localPosition = new Vector3(0, headOffset, 0);

        // Stunned text (ẩn mặc định)
        var sGO = new GameObject("StunnedText");
        sGO.transform.SetParent(headDisplayGO.transform);
        sGO.transform.localPosition = Vector3.zero;
        sGO.transform.localScale = Vector3.one * 0.25f;
        stunnedText = sGO.AddComponent<TextMesh>();
        stunnedText.text = "STUNNED!"; stunnedText.fontSize = 52;
        stunnedText.fontStyle = FontStyle.Bold; stunnedText.color = COL_STUNNED;
        stunnedText.anchor = TextAnchor.MiddleCenter;
        stunnedText.alignment = TextAlignment.Center;
        stunnedText.characterSize = 0.18f;
        sGO.SetActive(false);
    }

    void LateUpdate()
    {
        // Chữ nằm ngang theo map, nhìn từ trên xuống
        if (headDisplayGO != null)
            headDisplayGO.transform.rotation = Quaternion.Euler(68f, 0f, 0f);
    }

    void Update()
    {
        if (stunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f) { stunned = false; RefreshUI(); }
        }
        punchTimer -= Time.deltaTime;
        if (Input.GetKeyDown(selectKey)) OnSelectButtonPressed();
        if (Input.GetKeyDown(KeyCode.F) && punchTimer <= 0f) DoPunch();
    }

    // ── Tạo từng chữ cái riêng biệt trên đầu ──────────────────────────
    void BuildLetterMeshes()
    {
        // Xóa chữ cũ
        foreach (var tm in letterMeshes)
        {
            if (tm != null) Destroy(tm.gameObject);
        }
        letterMeshes.Clear();

        if (string.IsNullOrEmpty(targetWord)) return;

        int len = targetWord.Length;
        float totalWidth = (len - 1) * letterSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < len; i++)
        {
            var go = new GameObject("L_" + i);
            go.transform.SetParent(headDisplayGO.transform);
            go.transform.localPosition = new Vector3(startX + i * letterSpacing, 0, 0);
            go.transform.localScale = Vector3.one * 0.25f;

            var tm = go.AddComponent<TextMesh>();
            tm.text = targetWord[i].ToString();
            tm.fontSize = 52;
            tm.fontStyle = FontStyle.Bold;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.characterSize = 0.18f;
            tm.color = COL_PENDING;

            letterMeshes.Add(tm);
        }
    }

    // ── Cập nhật màu từng chữ ───────────────────────────────────────────
    void UpdateLetterColors()
    {
        if (stunned)
        {
            // Ẩn tất cả chữ, hiện "STUNNED!"
            foreach (var tm in letterMeshes)
                if (tm != null) tm.gameObject.SetActive(false);
            if (stunnedText != null) stunnedText.gameObject.SetActive(true);
            return;
        }

        // Hiện chữ, ẩn stunned
        if (stunnedText != null) stunnedText.gameObject.SetActive(false);

        for (int i = 0; i < letterMeshes.Count; i++)
        {
            var tm = letterMeshes[i];
            if (tm == null) continue;
            tm.gameObject.SetActive(true);

            if (i < progress)
            {
                // Đã gõ đúng → xanh lá
                tm.color = COL_DONE;
                // Scale nhỏ hơn một chút (đã hoàn thành)
                tm.transform.localScale = Vector3.one * 0.22f;
            }
            else if (i == progress)
            {
                // Chữ tiếp theo → vàng, to hơn
                tm.color = COL_NEXT;
                tm.transform.localScale = Vector3.one * 0.30f;
            }
            else
            {
                // Chưa tới → xám nhạt
                tm.color = COL_PENDING;
                tm.transform.localScale = Vector3.one * 0.25f;
            }
        }
    }

    // ── Game logic ──────────────────────────────────────────────────────
    public void StartNewWord(string word)
    {
        targetWord = word; progress = 0; stunned = false;
        KeyboardMap.Instance?.ResetAllTiles();
        BuildLetterMeshes();
        RefreshUI();
    }

    public void OnSelectButtonPressed()
    {
        if (stunned) return;
        if (WordManager.Instance == null || !WordManager.Instance.roundActive) return;
        char? current = KeyboardMap.Instance?.GetCurrentLetter();
        if (current == null) return;
        TryLetter(current.Value);
    }

    public bool TryLetter(char c)
    {
        if (stunned || string.IsNullOrEmpty(targetWord)) return false;
        if (progress >= targetWord.Length) return false;

        char need = targetWord[progress];
        if (char.ToUpper(c) == need)
        {
            KeyboardMap.Instance?.SetTileCorrect(need);
            progress++;
            RefreshUI();
            if (progress >= targetWord.Length)
                StartCoroutine(Complete());
            return true;
        }
        else
        {
            // Sai → flash chữ tiếp theo đỏ rồi trở lại vàng
            if (progress < letterMeshes.Count && letterMeshes[progress] != null)
                StartCoroutine(FlashWrong(letterMeshes[progress]));
            WordManager.Instance?.Announce("WRONG! Need: " + need, 0.8f);
            return false;
        }
    }

    IEnumerator FlashWrong(TextMesh tm)
    {
        Color orig = tm.color;
        tm.color = COL_STUNNED;
        yield return new WaitForSeconds(0.3f);
        tm.color = orig;
    }

    IEnumerator Complete()
    {
        // Tất cả chữ chuyển xanh lá sáng
        foreach (var tm in letterMeshes)
        {
            if (tm != null) { tm.color = new Color(0.3f, 1f, 0.5f); tm.transform.localScale = Vector3.one * 0.28f; }
        }
        WordManager.Instance?.OnWordCompleted(this);
        yield return null;
    }

    void RefreshUI()
    {
        UpdateLetterColors();
        if (WordWarsHUD.Instance != null)
            WordWarsHUD.Instance.UpdateProgress(this, targetWord, progress, stunned);
    }

    // ── Punch ───────────────────────────────────────────────────────────
    public void GetPunched(Vector3 fromPos)
    {
        if (stunned) return;
        int lose = Mathf.Min(progress, Random.Range(1, 3));
        progress = Mathf.Max(0, progress - lose);
        stunned = true; stunTimer = 1.5f;
        RefreshUI();
        if (rb != null)
        {
            Vector3 dir = (transform.position - fromPos).normalized;
            dir.y = 0.4f; rb.velocity = Vector3.zero;
            rb.AddForce(dir * punchForce, ForceMode.Impulse);
        }
        WordManager.Instance?.Announce("POW! " + playerName + " -" + lose + "!", 1.2f);
    }

    public void DoPunch()
    {
        if (punchTimer > 0f) return;
        punchTimer = punchCooldown;
        foreach (var s in FindObjectsOfType<PlayerWordSpeller>())
        {
            if (s == this) continue;
            if (Vector3.Distance(transform.position, s.transform.position) <= punchRadius)
                s.GetPunched(transform.position);
        }
    }

    public void OnPunchButtonPressed() => DoPunch();
    public string TargetWord => targetWord;
    public int    Progress   => progress;
    public bool   IsStunned  => stunned;
}