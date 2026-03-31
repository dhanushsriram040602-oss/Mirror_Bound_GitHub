using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Builds the CeilingCrawler bone-animation rig from code.
/// Run via:  Tools > Build CeilingCrawler Animations
/// </summary>
public static class CeilingCrawlerAnimationBuilder
{
    private const string OutputDir      = "Assets/Animations/Enemies";
    private const string ControllerPath = OutputDir + "/CeilingCrawler.controller";
    private const string ClipPatrol     = OutputDir + "/CeilingCrawler_Patrol.anim";
    private const string ClipFireDrop   = OutputDir + "/CeilingCrawler_FireDrop.anim";
    private const string ClipTurnAround = OutputDir + "/CeilingCrawler_TurnAround.anim";
    private const string ClipGhost      = OutputDir + "/CeilingCrawler_Ghost.anim";
    private const string ClipDeath      = OutputDir + "/CeilingCrawler_Death.anim";
    private const string PrefabPath     = "Assets/Prefabs/World 2 Prefabs/Enemies/CeilingCrawler.prefab";
    private const string SpritePath     = "Assets/Sprites/Enemies/CeilingCrawler-Photoroom.png";

    private const string PBody  = "Body";
    private const string PLegFL = "LegFL";
    private const string PLegFR = "LegFR";
    private const string PLegML = "LegML";
    private const string PLegMR = "LegMR";
    private const string PLegBL = "LegBL";
    private const string PLegBR = "LegBR";
    private const string PEye   = "EyeGlow";

    private const float RestFL = -50f;
    private const float RestFR =  50f;
    private const float RestML = -80f;
    private const float RestMR =  80f;
    private const float RestBL = -110f;
    private const float RestBR =  110f;

    [MenuItem("Tools/Build CeilingCrawler Animations")]
    public static void Build()
    {
        Directory.CreateDirectory(OutputDir);

        var patrol     = BuildPatrolClip();
        var fireDrop   = BuildFireDropClip();
        var turnAround = BuildTurnAroundClip();
        var ghost      = BuildGhostClip();
        var death      = BuildDeathClip();
        var ctrl       = BuildController(patrol, fireDrop, turnAround, ghost, death);

        WirePrefab(ctrl);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CeilingCrawler] Build complete.");
    }

    // ── PATROL — 0.55s loop, alternating tripod gait ─────────────────────
    private static AnimationClip BuildPatrolClip()
    {
        var clip = MakeClip("CeilingCrawler_Patrol", loop: true);
        float d = 0.55f;

        // Group A (FL, ML, BR) — leads
        CurveZ(clip, PLegFL, d, RestFL, RestFL - 20f, phase: 0f);
        CurveZ(clip, PLegML, d, RestML, RestML - 18f, phase: 0f);
        CurveZ(clip, PLegBR, d, RestBR, RestBR + 18f, phase: 0f);
        LegScaleY(clip, new[]{ PLegFL, PLegML, PLegBR }, d, 1f, 1.18f, phase: 0f);

        // Group B (FR, MR, BL) — 180° offset
        CurveZ(clip, PLegFR, d, RestFR, RestFR + 20f, phase: 0.5f);
        CurveZ(clip, PLegMR, d, RestMR, RestMR + 18f, phase: 0.5f);
        CurveZ(clip, PLegBL, d, RestBL, RestBL - 18f, phase: 0.5f);
        LegScaleY(clip, new[]{ PLegFR, PLegMR, PLegBL }, d, 1f, 1.18f, phase: 0.5f);

        clip.SetCurve(PBody, typeof(Transform), "localEulerAngles.z",
            Auto(C(KF(0f,0f),KF(d*.25f,3f),KF(d*.5f,0f),KF(d*.75f,-3f),KF(d,0f))));
        clip.SetCurve(PBody, typeof(Transform), "m_LocalPosition.y",
            Auto(C(KF(0f,0f),KF(d*.25f,0.03f),KF(d*.5f,0f),KF(d*.75f,0.03f),KF(d,0f))));
        clip.SetCurve(PEye, typeof(SpriteRenderer), "m_Color.a",
            Auto(C(KF(0f,0.65f),KF(d*.25f,1f),KF(d*.5f,0.65f),KF(d*.75f,1f),KF(d,0.65f))));

        return Save(clip, ClipPatrol);
    }

    // ── FIRE DROP — 0.55s once ────────────────────────────────────────────
    private static AnimationClip BuildFireDropClip()
    {
        var clip = MakeClip("CeilingCrawler_FireDrop", loop: false);
        float d = 0.55f;

        float[] rest  = { RestFL, RestFR, RestML, RestMR, RestBL, RestBR };
        float[] splay = { -14f, 14f, -20f, 20f, -22f, 22f };
        string[] legs = { PLegFL, PLegFR, PLegML, PLegMR, PLegBL, PLegBR };

        for (int i = 0; i < legs.Length; i++)
        {
            float s = rest[i] + splay[i];
            clip.SetCurve(legs[i], typeof(Transform), "localEulerAngles.z",
                Auto(C(KF(0f,rest[i]),KF(d*.12f,s),KF(d*.7f,s),KF(d,rest[i]))));
        }

        clip.SetCurve(PBody, typeof(Transform), "m_LocalPosition.y",
            Auto(C(KF(0f,0f),KF(d*.18f,0.16f),KF(d*.45f,-0.04f),KF(d*.65f,0.02f),KF(d,0f))));
        clip.SetCurve(PBody, typeof(Transform), "m_LocalScale.x",
            Auto(C(KF(0f,1f),KF(d*.18f,1.25f),KF(d*.4f,0.88f),KF(d,1f))));
        clip.SetCurve(PBody, typeof(Transform), "m_LocalScale.y",
            Auto(C(KF(0f,1f),KF(d*.18f,0.78f),KF(d*.4f,1.12f),KF(d,1f))));

        clip.SetCurve(PEye, typeof(SpriteRenderer), "m_Color.r", Const(0f, d, 5f));
        clip.SetCurve(PEye, typeof(SpriteRenderer), "m_Color.g",
            Auto(C(KF(0f,0.2f),KF(d*.18f,2f),KF(d*.45f,0.2f),KF(d,0.2f))));
        clip.SetCurve(PEye, typeof(SpriteRenderer), "m_Color.a",
            Auto(C(KF(0f,0.8f),KF(d*.18f,1f),KF(d,0.8f))));

        return Save(clip, ClipFireDrop);
    }

    // ── TURN AROUND — 0.28s once ──────────────────────────────────────────
    private static AnimationClip BuildTurnAroundClip()
    {
        var clip = MakeClip("CeilingCrawler_TurnAround", loop: false);
        float d = 0.28f;

        clip.SetCurve(PBody, typeof(Transform), "localEulerAngles.z",
            Auto(C(KF(0f,0f),KF(d*.35f,12f),KF(d*.6f,-8f),KF(d,0f))));
        clip.SetCurve(PBody, typeof(Transform), "m_LocalScale.x",
            Auto(C(KF(0f,1f),KF(d*.35f,0.84f),KF(d,1f))));
        clip.SetCurve(PBody, typeof(Transform), "m_LocalScale.y",
            Auto(C(KF(0f,1f),KF(d*.35f,1.16f),KF(d,1f))));

        float[] rest = { RestFL, RestFR, RestML, RestMR, RestBL, RestBR };
        float[] tuck = {  22f,  -22f,    30f,   -30f,    32f,   -32f };
        string[] legs= { PLegFL, PLegFR, PLegML, PLegMR, PLegBL, PLegBR };
        for (int i = 0; i < legs.Length; i++)
            clip.SetCurve(legs[i], typeof(Transform), "localEulerAngles.z",
                Auto(C(KF(0f,rest[i]),KF(d*.35f,rest[i]+tuck[i]),KF(d,rest[i]))));

        return Save(clip, ClipTurnAround);
    }

    // ── GHOST — 0.4s clamp ───────────────────────────────────────────────
    private static AnimationClip BuildGhostClip()
    {
        var clip = MakeClip("CeilingCrawler_Ghost", loop: false);
        float d = 0.4f;

        clip.SetCurve(PBody, typeof(SpriteRenderer), "m_Color.a", C(KF(0f,1f),KF(d,0.045f)));
        clip.SetCurve(PEye,  typeof(SpriteRenderer), "m_Color.a", C(KF(0f,0.8f),KF(d,0f)));

        float[] rest  = { RestFL, RestFR, RestML, RestMR, RestBL, RestBR };
        float[] droop = { -15f, 15f, -10f, 10f, -12f, 12f };
        string[] legs = { PLegFL, PLegFR, PLegML, PLegMR, PLegBL, PLegBR };
        for (int i = 0; i < legs.Length; i++)
            clip.SetCurve(legs[i], typeof(Transform), "localEulerAngles.z",
                C(KF(0f,rest[i]),KF(d,droop[i])));

        clip.SetCurve(PBody, typeof(Transform), "m_LocalPosition.y", C(KF(0f,0f),KF(d,0.05f)));

        return Save(clip, ClipGhost);
    }

    // ── DEATH — 0.6s clamp ───────────────────────────────────────────────
    private static AnimationClip BuildDeathClip()
    {
        var clip = MakeClip("CeilingCrawler_Death", loop: false);
        float d = 0.6f;

        clip.SetCurve(PBody, typeof(Transform), "m_LocalPosition.y",
            Auto(C(KF(0f,0f),KF(d*.25f,0.05f),KF(d*.55f,-0.38f),KF(d,-0.38f))));
        clip.SetCurve(PBody, typeof(Transform), "m_LocalScale.x",
            Auto(C(KF(0f,1f),KF(d*.55f,1.4f),KF(d*.7f,0.75f),KF(d,0.9f))));
        clip.SetCurve(PBody, typeof(Transform), "m_LocalScale.y",
            Auto(C(KF(0f,1f),KF(d*.55f,0.65f),KF(d*.7f,1.2f),KF(d,0.9f))));

        float[] rest = { RestFL, RestFR, RestML, RestMR, RestBL, RestBR };
        float[] fly  = { -38f, 38f, -42f, 42f, -36f, 36f };
        string[] legs= { PLegFL, PLegFR, PLegML, PLegMR, PLegBL, PLegBR };
        for (int i = 0; i < legs.Length; i++)
            clip.SetCurve(legs[i], typeof(Transform), "localEulerAngles.z",
                Auto(C(KF(0f,rest[i]),KF(d*.2f,rest[i]),KF(d*.55f,rest[i]+fly[i]),KF(d,rest[i]+fly[i]))));

        clip.SetCurve(PBody, typeof(SpriteRenderer), "m_Color.a",
            C(KF(0f,1f),KF(d*.6f,1f),KF(d,0f)));
        clip.SetCurve(PEye, typeof(SpriteRenderer), "m_Color.a",
            C(KF(0f,0.8f),KF(d*.5f,0.8f),KF(d,0f)));

        return Save(clip, ClipDeath);
    }

    // ── CONTROLLER ────────────────────────────────────────────────────────
    private static AnimatorController BuildController(
        AnimationClip patrol, AnimationClip fireDrop,
        AnimationClip turnAround, AnimationClip ghost, AnimationClip death)
    {
        if (File.Exists(ControllerPath)) AssetDatabase.DeleteAsset(ControllerPath);

        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        ctrl.AddParameter("isFiring",  AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("isTurning", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("isActive",  AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("isDead",    AnimatorControllerParameterType.Trigger);

        var sm = ctrl.layers[0].stateMachine;
        var stPatrol     = sm.AddState("Patrol",     new Vector3(260, 0,   0));
        var stFireDrop   = sm.AddState("FireDrop",   new Vector3(540, 0,   0));
        var stTurnAround = sm.AddState("TurnAround", new Vector3(260, 120, 0));
        var stGhost      = sm.AddState("Ghost",      new Vector3(540, 120, 0));
        var stDeath      = sm.AddState("Death",      new Vector3(800, 0,   0));

        stPatrol.motion     = patrol;
        stFireDrop.motion   = fireDrop;
        stTurnAround.motion = turnAround;
        stGhost.motion      = ghost;
        stDeath.motion      = death;
        sm.defaultState     = stPatrol;

        Tr(stPatrol,     stFireDrop,   0.05f).AddCondition(AnimatorConditionMode.If,    0, "isFiring");
        Tr(stPatrol,     stTurnAround, 0.05f).AddCondition(AnimatorConditionMode.If,    0, "isTurning");
        TrExit(stFireDrop,   stPatrol,     1f, 0.08f);
        TrExit(stTurnAround, stPatrol,     1f, 0.06f);
        Tr(stGhost, stPatrol, 0.15f).AddCondition(AnimatorConditionMode.If, 0, "isActive");

        var ag = sm.AddAnyStateTransition(stGhost);
        ag.hasExitTime = false; ag.duration = 0.15f; ag.hasFixedDuration = true;
        ag.canTransitionToSelf = false;
        ag.AddCondition(AnimatorConditionMode.IfNot, 0, "isActive");

        var ad = sm.AddAnyStateTransition(stDeath);
        ad.hasExitTime = false; ad.duration = 0.05f; ad.hasFixedDuration = true;
        ad.canTransitionToSelf = false;
        ad.AddCondition(AnimatorConditionMode.If, 0, "isDead");

        EditorUtility.SetDirty(ctrl);
        return ctrl;
    }

    // ── PREFAB WIRING ─────────────────────────────────────────────────────
    private static void WirePrefab(AnimatorController ctrl)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
        if (root == null) { Debug.LogError("Prefab not found: " + PrefabPath); return; }

        Sprite spr = LoadSprite(SpritePath);

        // Disable root SpriteRenderer — Body child owns the visual
        var rootSr = root.GetComponent<SpriteRenderer>();
        if (rootSr != null) rootSr.enabled = false;

        Bone(root, PBody,  new Vector3(0f,     0f,    0f), new Vector3(1f,    1f,    1f), 0f,     spr, new Color(2f,   2f,   2f,   1f), 2);
        Bone(root, PLegFL, new Vector3(-0.55f, 0.05f, 0f), new Vector3(0.09f, 0.45f, 1f), RestFL, spr, new Color(1.6f, 1.6f, 1.6f, 1f), 1);
        Bone(root, PLegFR, new Vector3( 0.55f, 0.05f, 0f), new Vector3(0.09f, 0.45f, 1f), RestFR, spr, new Color(1.6f, 1.6f, 1.6f, 1f), 1);
        Bone(root, PLegML, new Vector3(-0.64f, 0f,    0f), new Vector3(0.08f, 0.48f, 1f), RestML, spr, new Color(1.6f, 1.6f, 1.6f, 1f), 1);
        Bone(root, PLegMR, new Vector3( 0.64f, 0f,    0f), new Vector3(0.08f, 0.48f, 1f), RestMR, spr, new Color(1.6f, 1.6f, 1.6f, 1f), 1);
        Bone(root, PLegBL, new Vector3(-0.52f,-0.04f, 0f), new Vector3(0.08f, 0.42f, 1f), RestBL, spr, new Color(1.6f, 1.6f, 1.6f, 1f), 1);
        Bone(root, PLegBR, new Vector3( 0.52f,-0.04f, 0f), new Vector3(0.08f, 0.42f, 1f), RestBR, spr, new Color(1.6f, 1.6f, 1.6f, 1f), 1);
        Bone(root, PEye,   new Vector3(0f,     0.08f, 0f), new Vector3(0.22f, 0.10f, 1f), 0f,     spr, new Color(5f,   0.1f, 0f,   0.85f), 4);

        var anim = root.GetComponent<Animator>() ?? root.AddComponent<Animator>();
        anim.runtimeAnimatorController = ctrl;
        anim.cullingMode               = AnimatorCullingMode.AlwaysAnimate;
        anim.updateMode                = AnimatorUpdateMode.Normal;
        anim.applyRootMotion           = false;

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log("[CeilingCrawler] Prefab wired — 8 bones, AlwaysAnimate, controller updated.");
    }

    // ── HELPERS ───────────────────────────────────────────────────────────

    private static AnimationClip MakeClip(string name, bool loop)
    {
        var clip = new AnimationClip { name = name, wrapMode = loop ? WrapMode.Loop : WrapMode.ClampForever };
        var s = AnimationUtility.GetAnimationClipSettings(clip);
        s.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, s);
        return clip;
    }

    private static AnimationClip Save(AnimationClip clip, string path)
    {
        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing != null) EditorUtility.CopySerialized(clip, existing);
        else                  AssetDatabase.CreateAsset(clip, path);
        return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
    }

    private static Sprite LoadSprite(string path)
    {
        foreach (var a in AssetDatabase.LoadAllAssetsAtPath(path))
            if (a is Sprite s) return s;
        return null;
    }

    private static void Bone(GameObject root, string name,
        Vector3 pos, Vector3 scale, float rotZ, Sprite sprite, Color color, int order)
    {
        Transform t = root.transform.Find(name);
        GameObject go = t != null ? t.gameObject : new GameObject(name);
        go.transform.SetParent(root.transform, false);
        go.transform.localPosition    = pos;
        go.transform.localScale       = scale;
        go.transform.localEulerAngles = new Vector3(0f, 0f, rotZ);
        var sr = go.GetComponent<SpriteRenderer>() ?? go.AddComponent<SpriteRenderer>();
        if (sprite != null) sr.sprite = sprite;
        sr.color = color; sr.sortingOrder = order;
    }

    private static void CurveZ(AnimationClip clip, string path,
        float dur, float rest, float apex, float phase)
    {
        AnimationCurve curve = phase == 0f
            ? C(KF(0f,rest),KF(dur*.25f,apex),KF(dur*.5f,rest),KF(dur,rest))
            : C(KF(0f,rest),KF(dur*.5f,apex), KF(dur*.75f,rest),KF(dur,rest));
        Auto(curve);
        clip.SetCurve(path, typeof(Transform), "localEulerAngles.z", curve);
    }

    private static void LegScaleY(AnimationClip clip, string[] paths,
        float dur, float rest, float peak, float phase)
    {
        AnimationCurve curve = phase == 0f
            ? C(KF(0f,rest),KF(dur*.125f,peak),KF(dur*.25f,rest),KF(dur,rest))
            : C(KF(0f,rest),KF(dur*.5f,rest),KF(dur*.625f,peak),KF(dur*.75f,rest),KF(dur,rest));
        Auto(curve);
        foreach (var p in paths)
            clip.SetCurve(p, typeof(Transform), "m_LocalScale.y", curve);
    }

    private static AnimatorStateTransition Tr(AnimatorState from, AnimatorState to, float dur)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false; t.duration = dur; t.hasFixedDuration = true;
        return t;
    }

    private static void TrExit(AnimatorState from, AnimatorState to, float exitTime, float dur)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = true; t.exitTime = exitTime;
        t.duration = dur; t.hasFixedDuration = true;
    }

    private static Keyframe KF(float t, float v) => new Keyframe(t, v);
    private static AnimationCurve C(params Keyframe[] k) => new AnimationCurve(k);
    private static AnimationCurve Const(float s, float e, float v) => AnimationCurve.Constant(s, e, v);

    private static AnimationCurve Auto(AnimationCurve c)
    {
        for (int i = 0; i < c.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(c,  i, AnimationUtility.TangentMode.Auto);
            AnimationUtility.SetKeyRightTangentMode(c, i, AnimationUtility.TangentMode.Auto);
        }
        return c;
    }
}
