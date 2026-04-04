using UnityEngine;
using System.Collections.Generic;

public class KeyboardMap : MonoBehaviour
{
    public static KeyboardMap Instance { get; private set; }

    [Header("Tile Settings")]
    public float tileSize = 1.8f;
    public float tileHeight = 0.02f;
    public float tileSpacing = 0.3f;

    private static readonly Color COL_BODY = new Color(0.04f, 0.04f, 0.05f);
    private static readonly Color COL_TOP = new Color(0.16f, 0.18f, 0.23f);

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

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BuildKeyboard();
    }

    public void BuildKeyboard()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        tiles.Clear();

        float step = tileSize + tileSpacing;
        float totalD = rows.Length * step - tileSpacing;

        for (int r = 0; r < rows.Length; r++)
        {
            string row = rows[r];
            float rowW = row.Length * step - tileSpacing;
            float startX = -rowW / 2f + tileSize / 2f;
            float z = r * step - totalD / 2f + tileSize / 2f;

            for (int c = 0; c < row.Length; c++)
            {
                char letter = row[c];
                float x = startX + c * step;
                Color neon = neonColors[(letter - 'A') % neonColors.Length];
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
        go.transform.localPosition = localPos + new Vector3(0f, 2.8f, 0f);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        // 1. LỒNG ĐÈN
        GameObject lanternGO = null;
#if UNITY_EDITOR
var lanternPrefab = Resources.Load<GameObject>("lantern_hoian");
if (lanternPrefab != null)
    lanternGO = Instantiate(lanternPrefab);
#endif
        if (lanternGO != null)
        {
            lanternGO.name = "Lantern";
            lanternGO.transform.SetParent(go.transform);
            lanternGO.transform.localPosition = Vector3.zero;
            lanternGO.transform.localScale = Vector3.one * 57f;
            lanternGO.transform.localRotation = Quaternion.identity;
            foreach (var r in lanternGO.GetComponentsInChildren<Renderer>())
            {
                var mat = new Material(Shader.Find("Standard"));
                mat.color = neonColor * 0.6f;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", neonColor * 1.2f);
                r.material = mat;
            }
            foreach (var c in lanternGO.GetComponentsInChildren<Collider>())
                c.enabled = false;
        }
        var lanternMat = lanternGO != null ? lanternGO.GetComponentInChildren<Renderer>()?.material : null;



        // 5. CHỮ CÁI
        var lbl = new GameObject("Label");
        lbl.transform.SetParent(go.transform);
        lbl.transform.localPosition = new Vector3(0, 0.11f, 0.04f);
        lbl.transform.localRotation = Quaternion.Euler(44f, 0f, 0f);
        lbl.transform.localScale = Vector3.one * 0.34f;

        var tm = lbl.AddComponent<TextMesh>();
        tm.text = letter.ToString();
        tm.fontSize = 56;
        tm.fontStyle = FontStyle.Bold;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.characterSize = 0.5f;
        tm.color = new Color(0.15f, 0.1f, 0.05f);

        // 6. COLLIDER
        var col = go.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = s * 0.45f;
        col.center = Vector3.zero;


        // 8. TRIGGER detect player
        var trigGO = new GameObject("Trigger");
        trigGO.transform.SetParent(go.transform);
        trigGO.transform.localPosition = Vector3.zero;
        var trig = trigGO.AddComponent<SphereCollider>();
        trig.isTrigger = true;
        trig.radius = s * 0.5f;

        // 9. COMPONENT
        var kt = go.AddComponent<KeyTile3D>();
        kt.letter = letter;
        kt.neonColor = neonColor;
        kt.baseMat = lanternMat;
        kt.tileBase = COL_BODY;

        return kt;
    }

    GameObject MakeCube(Transform parent, string name, Vector3 lp, Vector3 ls, Color col)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        g.transform.SetParent(parent);
        g.transform.localPosition = lp;
        g.transform.localScale = ls;
        g.transform.localRotation = Quaternion.identity;

        Destroy(g.GetComponent<Collider>());

        var mat = new Material(Shader.Find("Standard"));
        mat.color = col;
        g.GetComponent<Renderer>().material = mat;

        return g;
    }

    void MakeNeon(Transform parent, Vector3 lp, Vector3 ls, Color color)
    {
        var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bar.name = "Neon";
        bar.transform.SetParent(parent);
        bar.transform.localPosition = lp;
        bar.transform.localScale = ls;
        bar.transform.localRotation = Quaternion.identity;

        Destroy(bar.GetComponent<Collider>());

        var mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * 8f);

        bar.GetComponent<Renderer>().material = mat;
    }

    void MakeSlope(Transform parent, Vector3 pos, Vector3 size, float angle, bool zAxis)
    {
        var slope = new GameObject("Slope");
        slope.transform.SetParent(parent);
        slope.transform.localPosition = pos;
        slope.transform.localRotation = zAxis
            ? Quaternion.Euler(angle, 0, 0)
            : Quaternion.Euler(0, 0, angle);

        var col = slope.AddComponent<BoxCollider>();
        col.size = size;
        col.center = new Vector3(0, size.y * 0.3f, 0);
    }

    public void OnPlayerEnterTile(KeyTile3D tile)
    {
        if (currentTile != null && currentTile != tile)
            currentTile.SetHighlight(false);

        currentTile = tile;
        currentTile.SetHighlight(true);
    }

    public void OnPlayerExitTile(KeyTile3D tile)
    {
        if (currentTile == tile)
        {
            currentTile.SetHighlight(false);
            currentTile = null;
        }
    }

    public char? GetCurrentLetter()
    {
        return currentTile?.letter;
    }

    public void SetTileCorrect(char c)
    {
        if (tiles.ContainsKey(c))
            tiles[c].SetCorrect();
    }

    public void ResetAllTiles()
    {
        foreach (var t in tiles.Values)
            t.ResetState();
    }
}