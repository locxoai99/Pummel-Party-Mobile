#if UNITY_EDITOR1
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public static class AnimatorSetup
{
    [MenuItem("Tools/[GAME] Setup Animator")]
    public static void CreateAnimator()
    {
        // ── Tìm clips ────────────────────────────────────────────────
        var idle = FindClip("Sad Idle");
        var walk = FindClip("Walking");
        var run  = FindClip("Running");

        if (idle == null) { Debug.LogError("❌ Không tìm thấy 'Sad Idle'"); return; }
        if (walk == null) { Debug.LogError("❌ Không tìm thấy 'Walking'");  return; }
        if (run  == null) { Debug.LogError("❌ Không tìm thấy 'Running'");  return; }

        // ── BẬT LOOP cho tất cả clips ────────────────────────────────
        SetLooping(idle, true);
        SetLooping(walk, true);
        SetLooping(run,  true);
        Debug.Log("✅ Đã bật Loop cho: " + idle.name + ", " + walk.name + ", " + run.name);

        // ── Tạo Animator Controller ───────────────────────────────────
        string path = "Assets/PlayerAnim.controller";
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);

        ctrl.AddParameter("Speed",      AnimatorControllerParameterType.Float);
        ctrl.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Jump",       AnimatorControllerParameterType.Trigger);

        var sm       = ctrl.layers[0].stateMachine;
        var sIdle    = sm.AddState("Idle");  sIdle.motion = idle;
        var sWalk    = sm.AddState("Walk");  sWalk.motion = walk;
        var sRun     = sm.AddState("Run");   sRun.motion  = run;
        sm.defaultState = sIdle;

        // Transitions — hasExitTime=false, duration ngắn để chuyển mượt
        Tr(sIdle, sWalk, "Speed", AnimatorConditionMode.Greater, 0.5f,  0.15f);
        Tr(sWalk, sIdle, "Speed", AnimatorConditionMode.Less,    0.5f,  0.15f);
        Tr(sWalk, sRun,  "Speed", AnimatorConditionMode.Greater, 3.5f,  0.1f);
        Tr(sRun,  sWalk, "Speed", AnimatorConditionMode.Less,    3.5f,  0.1f);
        Tr(sIdle, sRun,  "Speed", AnimatorConditionMode.Greater, 3.5f,  0.1f);
        Tr(sRun,  sIdle, "Speed", AnimatorConditionMode.Less,    0.5f,  0.15f);

        // ── Gán vào Player ────────────────────────────────────────────
        var player = GameObject.FindWithTag("Player");
        if (player == null) { Debug.LogError("❌ Không tìm thấy Player!"); return; }

        // Xoá Animator thừa trên Ch03_nonPBR
        var ch03 = player.transform.Find("Ch03_nonPBR");
        if (ch03 != null)
        {
            var extra = ch03.GetComponent<Animator>();
            if (extra != null) { Object.DestroyImmediate(extra); Debug.Log("✅ Xoá Animator thừa trên Ch03_nonPBR"); }
        }

        // Đảm bảo Animator ở trên Player root
        var anim = player.GetComponent<Animator>();
        if (anim == null) anim = player.AddComponent<Animator>();

        anim.runtimeAnimatorController = ctrl;
        anim.applyRootMotion           = false; // QUAN TRỌNG: dùng Rigidbody, không dùng root motion

        var avatar = FindAvatar("Ch03");
        if (avatar != null) { anim.avatar = avatar; Debug.Log("✅ Avatar: " + avatar.name); }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ XONG! Ctrl+S → Play");
    }

    // ── Bật loop cho 1 clip ───────────────────────────────────────────
    static void SetLooping(AnimationClip clip, bool loop)
    {
        if (clip == null) return;
        string path = AssetDatabase.GetAssetPath(clip);

        // Với clips embedded trong FBX, cần dùng ModelImporter
        var importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer != null)
        {
            var clips = importer.clipAnimations;
            if (clips.Length == 0) clips = importer.defaultClipAnimations;

            bool changed = false;
            foreach (var c in clips)
            {
                if (c.name == clip.name)
                {
                    c.loopTime = loop;
                    c.loopPose = loop;
                    changed    = true;
                }
            }
            if (changed)
            {
                importer.clipAnimations = clips;
                importer.SaveAndReimport();
                Debug.Log("✅ Loop ON: " + clip.name);
            }
        }
        else
        {
            // Clip độc lập (không trong FBX)
            var settings          = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime     = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
        }
    }

    static void Tr(AnimatorState from, AnimatorState to,
                   string param, AnimatorConditionMode mode,
                   float threshold, float duration)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration    = duration;
        t.AddCondition(mode, threshold, param);
    }

    static AnimationClip FindClip(string exactName)
    {
        foreach (var g in AssetDatabase.FindAssets("t:AnimationClip"))
        {
            var c = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(g));
            if (c != null && c.name == exactName) return c;
        }
        return null;
    }

    static Avatar FindAvatar(string keyword)
    {
        foreach (var g in AssetDatabase.FindAssets("t:Avatar"))
        {
            var a = AssetDatabase.LoadAssetAtPath<Avatar>(AssetDatabase.GUIDToAssetPath(g));
            if (a != null && a.name.ToLower().Contains(keyword.ToLower())) return a;
        }
        return null;
    }
}
#endif
