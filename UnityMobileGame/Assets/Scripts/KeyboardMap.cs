using UnityEngine;
using System.Collections.Generic;

public class KeyboardMap : MonoBehaviour
{
    public static KeyboardMap Instance { get; private set; }

    [Header("Tile Settings")]
    public float tileSize    = 1.8f;   // nhỏ hơn (was 2.8)
    public float tileHeight  = 0.25f;  // thấp hơn (was 0.35)
    public float tileSpacing = 0.6f;   // khoảng cách hẹp hơn (was 1.4)

    private static readonly Color COL_BODY = new Color(0.08f, 0.10f, 0.16f);
    private static readonly Color COL_TOP  = new Color(0.11f, 0.14f, 0.22f);

    private Dictionary<char, KeyTile3D> tiles = new Dictionary<char, KeyTile3D>();
    private KeyTile3D currentTile = null;

    private static readonly string[] rows = { "ABCDEF", "GHIJKL", "MNOPQR", "STUVWX", "YZ" };

    private static readonly Color[] neonColors = {
        new Color(0.35f,1f,0.50f), new Color(1f,0.50f,0.18f),
        new Color(0.45f,0.75f,1f), new Color(0.85f,0.40f,1f),
        new Color(1f,0.85f,0.15f), new Color(0.15f,0.90f,1f),
        new Color(1f,0.35f,0.55f), new Color(0.65f,1f,0.25f),
        new Color(1f,0.58f,0.08f), new Color(0.45f,0.78f,1f),
        new Color(0.90f,0.25f,1f), new Color(0.25f,1f,0.78f),
        new Color(1f,0.90f,0.25f), new Color(0.55f,1f,0.55f),
        new Color(1f,0.42f,0.42f), new Color(0.42f,0.62f,1f),
        new Color(1f,0.72f,0.35f), new Color(0.72f,1f,0.42f),
        new Color(0.35f,0.88f,1f), new Color(1f,0.50f,0.72f),
        new Color(0.62f,0.42f,1f), new Color(0.95f,1f,0.35f),
        new Color(0.35f,1f,0.68f), new Color(1f,0.68f,0.22f),
        new Color(0.50f,0.88f,1f), new Color(1f,0.42f,0.78f),
    };

    void Awake() { Instance = this; }
    void Start()  { BuildKeyboard(); }

    public void BuildKeyboard()
    {
        foreach (Transform child in transform) Destroy(child.gameObject);
        tiles.Clear();

        float step   = tileSize + tileSpacing;
        float totalD = rows.Length * step - tileSpacing;

        for (int r = 0; r < rows.Length; r++)
        {
            string row   = rows[r];
            float rowW   = row.Length * step - tileSpacing;
            float startX = -rowW / 2f + tileSize / 2f;
            float z      = r * step - totalD / 2f + tileSize / 2f;

            for (int c = 0; c < row.Length; c++)
            {
                char  letter = row[c];
                float x      = startX + c * step;
                Color neon   = neonColors[(letter - 'A') % neonColors.Length];
                tiles[letter] = CreateTile(letter, new Vector3(x, 0f, z), neon);
            }
        }
    }

    KeyTile3D CreateTile(char letter, Vector3 localPos, Color neonColor)
    {
        float s = tileSize;
        float h = tileHeight;

        var go = new GameObject("Key_" + letter);
        go.transform.SetParent(transform);
        go.transform.localPosition = localPos;

        // 1. BODY
        var body = MakeCube(go.transform, "Body",
            new Vector3(0, h / 2f, 0), new Vector3(s, h, s), COL_BODY);

        // 2. TOP
        float topH = 0.04f, inset = 0.04f;
        MakeCube(go.transform, "Top",
            new Vector3(0, h + topH / 2f, 0),
            new Vector3(s - inset, topH, s - inset), COL_TOP);

        // 3. VIỀN NEON 4 mặt bên
        float ew = 0.06f, eh = h * 0.65f, ey = h / 2f, reach = s / 2f;
        MakeNeon(go.transform, new Vector3(0, ey,  reach), new Vector3(s*0.7f, eh, ew), neonColor);
        MakeNeon(go.transform, new Vector3(0, ey, -reach), new Vector3(s*0.7f, eh, ew), neonColor);
        MakeNeon(go.transform, new Vector3(-reach, ey, 0), new Vector3(ew, eh, s*0.7f), neonColor);
        MakeNeon(go.transform, new Vector3( reach, ey, 0), new Vector3(ew, eh, s*0.7f), neonColor);

        // 4. VIỀN NEON mặt trên
        float bt = 0.05f, by = h + topH + 0.002f;
        float bHalf = s / 2f - bt / 2f - inset / 2f;
        MakeNeon(go.transform, new Vector3(0, by,  bHalf), new Vector3(s*0.68f, bt*0.25f, bt), neonColor);
        MakeNeon(go.transform, new Vector3(0, by, -bHalf), new Vector3(s*0.68f, bt*0.25f, bt), neonColor);
        MakeNeon(go.transform, new Vector3(-bHalf, by, 0), new Vector3(bt, bt*0.25f, s*0.68f), neonColor);
        MakeNeon(go.transform, new Vector3( bHalf, by, 0), new Vector3(bt, bt*0.25f, s*0.68f), neonColor);

        // 5. CHỮ CÁI
        var lbl = new GameObject("Label");
        lbl.transform.SetParent(go.transform);
        lbl.transform.localPosition = new Vector3(0, h + topH + 0.01f, 0);
        lbl.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        lbl.transform.localScale    = Vector3.one * 0.28f; // nhỏ hơn (was 0.40)
        var tm = lbl.AddComponent<TextMesh>();
        tm.text = letter.ToString(); tm.fontSize = 56; tm.fontStyle = FontStyle.Bold;
        tm.color = neonColor; tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;

        // 6. COLLIDER
        var col = go.AddComponent<BoxCollider>();
        col.isTrigger = false;
        col.center = new Vector3(0, h / 2f, 0);
        col.size   = new Vector3(s * 0.88f, h, s * 0.88f);

        // 7. SLOPE 4 cạnh (nhỏ hơn)
        float slopeW = 0.25f;
        MakeSlope(go.transform, new Vector3(0, 0, -s/2f), new Vector3(s, h, slopeW), -15f, true);
        MakeSlope(go.transform, new Vector3(0, 0,  s/2f), new Vector3(s, h, slopeW),  15f, true);
        MakeSlope(go.transform, new Vector3(-s/2f, 0, 0), new Vector3(slopeW, h, s), -15f, false);
        MakeSlope(go.transform, new Vector3( s/2f, 0, 0), new Vector3(slopeW, h, s),  15f, false);

        // 8. TRIGGER detect player
        var trigGO = new GameObject("Trigger");
        trigGO.transform.SetParent(go.transform);
        trigGO.transform.localPosition = new Vector3(0, h + 0.4f, 0);
        var trig = trigGO.AddComponent<BoxCollider>();
        trig.isTrigger = true;
        trig.size = new Vector3(s * 0.8f, 0.8f, s * 0.8f);

        // 9. Component
        var kt = go.AddComponent<KeyTile3D>();
        kt.letter = letter; kt.neonColor = neonColor;
        kt.baseMat = body.GetComponent<Renderer>().material;
        kt.tileBase = COL_BODY;

        return kt;
    }

    GameObject MakeCube(Transform parent, string name, Vector3 lp, Vector3 ls, Color col)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name; g.transform.SetParent(parent);
        g.transform.localPosition = lp; g.transform.localScale = ls;
        Destroy(g.GetComponent<Collider>());
        g.GetComponent<Renderer>().material = new Material(Shader.Find("Standard")) { color = col };
        return g;
    }

    void MakeNeon(Transform parent, Vector3 lp, Vector3 ls, Color color)
    {
        var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bar.name = "Neon"; bar.transform.SetParent(parent);
        bar.transform.localPosition = lp; bar.transform.localScale = ls;
        Destroy(bar.GetComponent<Collider>());
        var mat = new Material(Shader.Find("Standard")) { color = color };
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * 2.2f);
        bar.GetComponent<Renderer>().material = mat;
    }

    void MakeSlope(Transform parent, Vector3 pos, Vector3 size, float angle, bool zAxis)
    {
        var slope = new GameObject("Slope"); slope.transform.SetParent(parent);
        slope.transform.localPosition = pos;
        slope.transform.localRotation = zAxis
            ? Quaternion.Euler(angle, 0, 0)
            : Quaternion.Euler(0, 0, angle);
        var col = slope.AddComponent<BoxCollider>();
        col.size = size; col.center = new Vector3(0, size.y * 0.3f, 0);
    }

    public void OnPlayerEnterTile(KeyTile3D tile)
    {
        if (currentTile != null && currentTile != tile) currentTile.SetHighlight(false);
        currentTile = tile; currentTile.SetHighlight(true);
    }

    public void OnPlayerExitTile(KeyTile3D tile)
    {
        if (currentTile == tile) { currentTile.SetHighlight(false); currentTile = null; }
    }

    public char? GetCurrentLetter() => currentTile?.letter;
    public void SetTileCorrect(char c) { if (tiles.ContainsKey(c)) tiles[c].SetCorrect(); }
    public void ResetAllTiles() { foreach (var t in tiles.Values) t.ResetState(); }
}
