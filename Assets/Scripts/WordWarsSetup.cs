#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;

public static class WordWarsSetup
{
    [MenuItem("Tools/[WORDWARS] Setup Scene")]
    public static void Build()
    {
        Debug.Log("=== Word Wars — Pummel Party Style v4 ===");
        var existing = GameObject.FindWithTag("Player");
        foreach (var go in Object.FindObjectsOfType<GameObject>())
        {
            if (existing != null && (go == existing || go.transform.IsChildOf(existing.transform))) continue;
            Object.DestroyImmediate(go);
        }

        Lighting();
        BuildArena();

        var kmGO = new GameObject("KeyboardMap");
        kmGO.AddComponent<KeyboardMap>();
        kmGO.transform.position = Vector3.zero;

        var player = SetupPlayer(existing, new Vector3(0f, 1.2f, -8.5f));
        var cam = MakeCam(player);
        player.GetComponent<PlayerController>().cameraTransform = cam.transform;
        new GameObject("WordManager").AddComponent<WordManager>();
        MakeCanvas(player);

        if (!System.IO.Directory.Exists("Assets/Scenes"))
            System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/GameScene.unity");
        AssetDatabase.Refresh();
        Selection.activeGameObject = player;
        AnimatorSetup.CreateAnimator();
        Debug.Log("✅ DONE! Chữ trên đầu đổi màu khi select — E chọn chữ");
    }

    static void Lighting()
    {
        var s = new GameObject("Sun"); var l = s.AddComponent<Light>();
        l.type = LightType.Directional; l.intensity = 0.85f;
        l.color = new Color(0.78f, 0.86f, 1f); l.shadows = LightShadows.Soft;
        s.transform.rotation = Quaternion.Euler(65f, -20f, 0f);

        var f = new GameObject("Fill"); var fl = f.AddComponent<Light>();
        fl.type = LightType.Directional; fl.intensity = 0.22f;
        fl.color = new Color(0.28f, 0.18f, 0.68f);
        f.transform.rotation = Quaternion.Euler(10f, 160f, 0f);

        var r = new GameObject("Rim"); var rl = r.AddComponent<Light>();
        rl.type = LightType.Directional; rl.intensity = 0.12f;
        rl.color = new Color(0.12f, 0.80f, 1f);
        r.transform.rotation = Quaternion.Euler(25f, -90f, 0f);

        RenderSettings.ambientLight = new Color(0.07f, 0.09f, 0.16f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.02f, 0.04f, 0.08f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 32f;
        RenderSettings.fogEndDistance   = 65f;
    }

    static void BuildArena()
    {
        // Sàn
        C("Floor", V(0, -0.25f, -1f), V(18, 0.5f, 14), H("0A0E17"));
C("SubFloor", V(0, -0.6f, -1f), V(20, 0.2f, 16), H("050910"));
        // Tường
        Color wall = H("0D1320");
        C("Wall_N", V(0, 3f,  6.7f), V(17, 7f, 1), wall);
C("Wall_S", V(0, 3f, -7.44f), V(17, 7f, 1), wall);
C("Wall_E", V(8.5f, 3f, -1.03f),  V(1, 7f, 13), wall);
C("Wall_W", V(-8.5f, 3f, -0.03f), V(1, 7f, 13), wall);

        // Hazard stripe trên tường (cam neon)
        Color stripe = new Color(1f, 0.7f, 0f);
        Emit(C("Stripe_N", V(0, 6.2f,  5.5f), V(17, 0.3f, 0.45f), stripe), stripe, 1.8f);
Emit(C("Stripe_S", V(0, 6.2f, -7.2f), V(17, 0.3f, 0.45f), stripe), stripe, 1.8f);
Emit(C("Stripe_E", V(8.5f, 6.2f, -1f),  V(0.45f, 0.3f, 13), stripe), stripe, 1.8f);
Emit(C("Stripe_W", V(-8.5f, 6.2f, -1f), V(0.45f, 0.3f, 13), stripe), stripe, 1.8f);

        // KHÔNG CÓ ACCENT NEON (đã bỏ thanh xanh)

        // Grid sàn — mờ nhẹ
        Color grid = H("0D121C");
        for (int i = -3; i <= 3; i++)
        {
            C("GH" + i, V(i * 4.5f, -0.16f, 0), V(0.03f, 0.01f, 26), grid);
            C("GV" + i, V(0, -0.16f, i * 4.5f), V(20, 0.01f, 0.03f), grid);
        }

        // Respawn
        var rz = new GameObject("RespawnZone"); rz.transform.position = V(0, -10f, 0);
        var col = rz.AddComponent<BoxCollider>(); col.size = V(200, 1, 200); col.isTrigger = true;
        var rsp = rz.AddComponent<RespawnZone>(); rsp.respawnPoint = V(0, 1.2f, -8.5f);
    }

    static GameObject SetupPlayer(GameObject ex, Vector3 spawnPos)
{
    GameObject p;
    if (ex != null)
    {
        p = ex;
        p.transform.position = spawnPos;

        var rb = p.GetComponent<Rigidbody>() ?? p.AddComponent<Rigidbody>();
        rb.mass = 2f;
        rb.drag = 5f;
        rb.freezeRotation = true;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        foreach (var mb in p.GetComponents<MonoBehaviour>())
        {
            string t = mb.GetType().Name;
            if (t == "PlayerMovement" || t == "OldPlayerController")
                Object.DestroyImmediate(mb);
        }

        if (!p.GetComponent<PlayerController>())
            p.AddComponent<PlayerController>();

        if (!p.GetComponent<PlayerWordSpeller>())
        {
            var pws = p.AddComponent<PlayerWordSpeller>();
            pws.playerName = "Player 1";
        }

        EnsureVisualModel(p);
    }
    else
    {
        p = new GameObject("Player");
        p.tag = "Player";
        p.layer = 0;
        p.transform.position = spawnPos;

        var capsule = p.AddComponent<CapsuleCollider>();
        capsule.height = 2f;
        capsule.radius = 0.5f;
        capsule.center = new Vector3(0f, 1f, 0f);

        var rb = p.AddComponent<Rigidbody>();
        rb.mass = 2f;
        rb.drag = 5f;
        rb.freezeRotation = true;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        p.AddComponent<PlayerController>();

        var pws = p.AddComponent<PlayerWordSpeller>();
        pws.playerName = "Player 1";

        EnsureVisualModel(p);
    }

    return p;
}
static void EnsureVisualModel(GameObject playerRoot)
{
    var oldVisual = playerRoot.transform.Find("Visual");
    if (oldVisual != null) return;

    var modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Ch03_nonPBR.fbx");
    if (modelPrefab == null)
    {
        Debug.LogWarning("Không tìm thấy model: Assets/Ch03_nonPBR.fbx");
        return;
    }

    var visual = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
    visual.name = "Visual";
    visual.transform.SetParent(playerRoot.transform, false);

    visual.transform.localPosition = Vector3.zero;
    visual.transform.localRotation = Quaternion.identity;
    visual.transform.localScale = Vector3.one;

    foreach (var c in visual.GetComponentsInChildren<Collider>())
        c.enabled = false;
}

    static GameObject MakeCam(GameObject player)
    {
        var g = new GameObject("Main Camera"); g.tag = "MainCamera";
        var cam = g.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.02f, 0.03f, 0.07f);
        cam.fieldOfView = 52f;
        g.AddComponent<AudioListener>();

        var cf = g.AddComponent<CameraFollow>();
        cf.target = player.transform;
        cf.offset = new Vector3(0f, 16f, -14f);
        cf.smoothTime = 0.15f;

        g.transform.position = player.transform.position + new Vector3(0f, 12.8f, 2f);
        g.transform.rotation = Quaternion.Euler(68f, 0f, 0f);

        return g;
    }

    static void MakeCanvas(GameObject player)
    {
        if (!Object.FindObjectOfType<EventSystem>())
        { var es = new GameObject("EventSystem"); es.AddComponent<EventSystem>(); es.AddComponent<StandaloneInputModule>(); }

        var cGO = new GameObject("GameCanvas");
        var cv = cGO.AddComponent<Canvas>(); cv.renderMode = RenderMode.ScreenSpaceOverlay; cv.sortingOrder = 10;
        var sc = cGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1080, 1920); sc.matchWidthOrHeight = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();
        cGO.AddComponent<WordWarsHUD>();
    }

    static GameObject C(string n, Vector3 p, Vector3 s, Color c)
    { var g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = n; g.layer = 0;
      g.transform.position = p; g.transform.localScale = s; M(g, c); return g; }

    static void M(GameObject g, Color c)
    { var r = g.GetComponent<Renderer>(); if (r) r.sharedMaterial = new Material(Shader.Find("Standard")) { color = c }; }

    static void Emit(GameObject g, Color c, float i)
    { var r = g.GetComponent<Renderer>(); if (r == null) return;
      r.sharedMaterial.EnableKeyword("_EMISSION"); r.sharedMaterial.SetColor("_EmissionColor", c * i); }

    static Color H(string h) { ColorUtility.TryParseHtmlString("#" + h, out Color c); return c; }
    static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);
}
#endif