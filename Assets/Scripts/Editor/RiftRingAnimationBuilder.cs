using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Builds all RiftRing animation clips and the AnimatorController from code.
/// Run via: Tools > Build RiftRing Animations
/// Regenerate any time by running the menu item again.
/// </summary>
public static class RiftRingAnimationBuilder
{
    private const string OutputDir        = "Assets/Animations/Enemies";
    private const string ControllerPath   = OutputDir + "/RiftRing.controller";
    private const string ClipIdle         = OutputDir + "/RiftRing_Idle.anim";
    private const string ClipCharge       = OutputDir + "/RiftRing_Charge.anim";
    private const string ClipFire         = OutputDir + "/RiftRing_Fire.anim";
    private const string ClipGhost        = OutputDir + "/RiftRing_Ghost.anim";
    private const string PrefabPath       = "Assets/Prefabs/World 2 Prefabs/Enemies/RiftRing.prefab";

    // ── Child path tokens matching the prefab hierarchy ──────────────────
    private const string PathOuterRing    = "OuterRing";
    private const string PathMidRing      = "MidRing";
    private const string PathInnerRing    = "InnerRing";
    private const string PathCore         = "Core";
    private const string PathTrail        = "Trail";

    [MenuItem("Tools/Build RiftRing Animations")]
    public static void Build()
    {
        Directory.CreateDirectory(OutputDir);

        AnimationClip idle   = BuildIdleClip();
        AnimationClip charge = BuildChargeClip();
        AnimationClip fire   = BuildFireClip();
        AnimationClip ghost  = BuildGhostClip();

        AnimatorController controller = BuildController(idle, charge, fire, ghost);
        WirePrefab(controller);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[RiftRingAnimationBuilder] All clips and controller built successfully.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // IDLE  — slow dual-counter-rotation, gentle core breathe, alpha pulse
    // Duration: 2 s looping
    // ─────────────────────────────────────────────────────────────────────
    private static AnimationClip BuildIdleClip()
    {
        var clip = new AnimationClip { name = "RiftRing_Idle", wrapMode = WrapMode.Loop };
        AnimationUtility.SetAnimationClipSettings(clip, new AnimationClipSettings { loopTime = true });

        // OuterRing: +60 °/s (full 360 at 6 s — we keyframe 0→120 over 2 s, loop handles the rest)
        SetEulerZLinearLoop(clip, PathOuterRing,  0f,  120f, 2f);

        // MidRing: -90 °/s counter
        SetEulerZLinearLoop(clip, PathMidRing,    0f, -180f, 2f);

        // InnerRing: +180 °/s faster spin
        SetEulerZLinearLoop(clip, PathInnerRing,  0f,  360f, 2f);

        // Core scale breathe  1.0 → 1.08 → 1.0 over 2 s
        SetScalePingPong(clip, PathCore, 1.0f, 1.08f, 2f);

        // Core alpha pulse  0.75 → 1.0 → 0.75
        SetAlphaPingPong(clip, PathCore, 0.75f, 1.0f, 2f);

        // OuterRing alpha pulse  0.5 → 0.8 → 0.5  (offset by half period)
        SetAlphaPingPong(clip, PathOuterRing, 0.5f, 0.8f, 2f);

        SaveClip(clip, ClipIdle);
        return clip;
    }

    // ─────────────────────────────────────────────────────────────────────
    // CHARGE — rings accelerate, core flares bright, scale expands
    // Duration: 0.6 s, plays once then loops
    // ─────────────────────────────────────────────────────────────────────
    private static AnimationClip BuildChargeClip()
    {
        var clip = new AnimationClip { name = "RiftRing_Charge", wrapMode = WrapMode.Loop };
        AnimationUtility.SetAnimationClipSettings(clip, new AnimationClipSettings { loopTime = true });

        // Rings ramp to fast spin over 0.6 s
        SetEulerZLinearLoop(clip, PathOuterRing,  0f,  108f, 0.6f);   // 180 °/s
        SetEulerZLinearLoop(clip, PathMidRing,    0f, -144f, 0.6f);   // -240 °/s
        SetEulerZLinearLoop(clip, PathInnerRing,  0f,  216f, 0.6f);   // 360 °/s

        // Core expands to 1.3 and oscillates
        SetScalePingPong(clip, PathCore, 1.15f, 1.3f, 0.6f);

        // Core alpha max
        SetAlphaPingPong(clip, PathCore, 0.9f, 1.0f, 0.6f);

        // OuterRing flares
        SetAlphaPingPong(clip, PathOuterRing, 0.7f, 1.0f, 0.6f);

        // Trail stretches horizontally
        SetScaleXPingPong(clip, PathTrail, 1.0f, 1.4f, 0.3f);

        SaveClip(clip, ClipCharge);
        return clip;
    }

    // ─────────────────────────────────────────────────────────────────────
    // FIRE  — rings locked at full speed, trail fully extended, bright flash
    // Duration: 0.4 s looping (while projectile travels)
    // ─────────────────────────────────────────────────────────────────────
    private static AnimationClip BuildFireClip()
    {
        var clip = new AnimationClip { name = "RiftRing_Fire", wrapMode = WrapMode.Loop };
        AnimationUtility.SetAnimationClipSettings(clip, new AnimationClipSettings { loopTime = true });

        SetEulerZLinearLoop(clip, PathOuterRing,  0f,  140f, 0.4f);   // 350 °/s
        SetEulerZLinearLoop(clip, PathMidRing,    0f, -180f, 0.4f);   // -450 °/s
        SetEulerZLinearLoop(clip, PathInnerRing,  0f,  240f, 0.4f);   // 600 °/s

        // Core constant max scale flash
        SetScalePingPong(clip, PathCore, 1.25f, 1.35f, 0.2f);

        // All rings full alpha
        SetAlphaConstant(clip, PathCore, 1.0f, 0.4f);
        SetAlphaConstant(clip, PathOuterRing, 1.0f, 0.4f);
        SetAlphaConstant(clip, PathMidRing, 1.0f, 0.4f);

        // Trail fully stretched
        SetScaleXConstant(clip, PathTrail, 1.6f, 0.4f);

        SaveClip(clip, ClipFire);
        return clip;
    }

    // ─────────────────────────────────────────────────────────────────────
    // GHOST — everything fades to near-zero, rings slow and drift
    // Duration: 0.5 s (fade out), then loops silently
    // ─────────────────────────────────────────────────────────────────────
    private static AnimationClip BuildGhostClip()
    {
        var clip = new AnimationClip { name = "RiftRing_Ghost", wrapMode = WrapMode.ClampForever };
        AnimationUtility.SetAnimationClipSettings(clip, new AnimationClipSettings { loopTime = false });

        // Slow drift rotation
        SetEulerZLinearLoop(clip, PathOuterRing,  0f, 18f,  0.5f);
        SetEulerZLinearLoop(clip, PathMidRing,    0f, -12f, 0.5f);
        SetEulerZLinearLoop(clip, PathInnerRing,  0f, 30f,  0.5f);

        // Fade all renderers to ghost alpha 0.08
        FadeAlpha(clip, PathOuterRing, 1.0f, 0.08f, 0.5f);
        FadeAlpha(clip, PathMidRing,   1.0f, 0.08f, 0.5f);
        FadeAlpha(clip, PathInnerRing, 1.0f, 0.08f, 0.5f);
        FadeAlpha(clip, PathCore,      1.0f, 0.08f, 0.5f);
        FadeAlpha(clip, PathTrail,     1.0f, 0.0f,  0.5f);

        // Scale down slightly to sell the vanish
        FadeScale(clip, PathCore, 1.0f, 0.7f, 0.5f);

        SaveClip(clip, ClipGhost);
        return clip;
    }

    // ─────────────────────────────────────────────────────────────────────
    // CONTROLLER
    // Parameters: isCharging (Bool), isFiring (Trigger), isLethal (Bool)
    // States: Idle → Charge → Fire → Idle   |   any → Ghost
    // ─────────────────────────────────────────────────────────────────────
    private static AnimatorController BuildController(
        AnimationClip idle, AnimationClip charge,
        AnimationClip fire, AnimationClip ghost)
    {
        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

        controller.AddParameter("isCharging", AnimatorControllerParameterType.Bool);
        controller.AddParameter("isFiring",   AnimatorControllerParameterType.Trigger);
        controller.AddParameter("isLethal",   AnimatorControllerParameterType.Bool);

        var root = controller.layers[0].stateMachine;
        root.entryPosition          = new Vector3(0,   -120, 0);
        root.anyStatePosition       = new Vector3(0,   -60,  0);
        root.exitPosition           = new Vector3(0,   -180, 0);

        // ── States ──────────────────────────────────────────────────────
        var stIdle   = root.AddState("Idle",   new Vector3(260,   0, 0));
        var stCharge = root.AddState("Charge", new Vector3(520,   0, 0));
        var stFire   = root.AddState("Fire",   new Vector3(780,   0, 0));
        var stGhost  = root.AddState("Ghost",  new Vector3(260, 130, 0));

        stIdle.motion   = idle;
        stCharge.motion = charge;
        stFire.motion   = fire;
        stGhost.motion  = ghost;

        root.defaultState = stIdle;

        // ── Transitions ─────────────────────────────────────────────────
        // Idle → Charge
        var t = stIdle.AddTransition(stCharge);
        t.hasExitTime      = false;
        t.duration         = 0.1f;
        t.hasFixedDuration = true;
        t.AddCondition(AnimatorConditionMode.If, 0, "isCharging");

        // Charge → Idle (cancel)
        t = stCharge.AddTransition(stIdle);
        t.hasExitTime      = false;
        t.duration         = 0.1f;
        t.hasFixedDuration = true;
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "isCharging");

        // Charge → Fire
        t = stCharge.AddTransition(stFire);
        t.hasExitTime      = false;
        t.duration         = 0.05f;
        t.hasFixedDuration = true;
        t.AddCondition(AnimatorConditionMode.If, 0, "isFiring");

        // Fire → Idle (exit time — one travel loop)
        t = stFire.AddTransition(stIdle);
        t.hasExitTime = true;
        t.exitTime    = 1f;
        t.duration    = 0.08f;
        t.hasFixedDuration = true;

        // Any → Ghost (when isLethal becomes false)
        var anyGhost = root.AddAnyStateTransition(stGhost);
        anyGhost.hasExitTime      = false;
        anyGhost.duration         = 0.2f;
        anyGhost.hasFixedDuration = true;
        anyGhost.canTransitionToSelf = false;
        anyGhost.AddCondition(AnimatorConditionMode.IfNot, 0, "isLethal");

        // Ghost → Idle (when isLethal becomes true)
        t = stGhost.AddTransition(stIdle);
        t.hasExitTime      = false;
        t.duration         = 0.2f;
        t.hasFixedDuration = true;
        t.AddCondition(AnimatorConditionMode.If, 0, "isLethal");

        EditorUtility.SetDirty(controller);
        return controller;
    }

    // ─────────────────────────────────────────────────────────────────────
    // PREFAB WIRING
    // ─────────────────────────────────────────────────────────────────────
    private static void WirePrefab(AnimatorController controller)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogWarning("[RiftRingAnimationBuilder] RiftRing prefab not found at " + PrefabPath);
            return;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);

        EnsureChildRenderer(root, PathOuterRing,  new Vector3(0, 0, 0),  new Vector3(0.9f, 0.9f, 1),   0,   new Color(2f, 1f, 0f, 0.6f),  1);
        EnsureChildRenderer(root, PathMidRing,     new Vector3(0, 0, 0),  new Vector3(0.65f, 0.65f, 1), 22f, new Color(2.5f, 1.2f, 0f, 0.5f), 2);
        EnsureChildRenderer(root, PathInnerRing,   new Vector3(0, 0, 0),  new Vector3(0.4f, 0.4f, 1),   -15f,new Color(3f, 1.5f, 0f, 0.75f), 3);
        EnsureChildRenderer(root, PathCore,        new Vector3(0, 0, 0),  new Vector3(0.18f, 0.18f, 1), 0f,  new Color(6f, 3f, 0f, 1f),     4);
        EnsureChildRenderer(root, PathTrail,       new Vector3(0, 0, 0),  new Vector3(0.5f, 0.12f, 1),  0f,  new Color(2f, 1f, 0f, 0.4f),   2);

        // Add/update Animator on root
        var animator = root.GetComponent<Animator>() ?? root.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.cullingMode               = AnimatorCullingMode.AlwaysAnimate;

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        PrefabUtility.UnloadPrefabContents(root);

        Debug.Log("[RiftRingAnimationBuilder] Prefab wired: Animator + " + 5 + " bone-children added.");
    }

    /// <summary>
    /// Finds or creates a child GameObject at <paramref name="childName"/> under <paramref name="parent"/>,
    /// ensures it has a SpriteRenderer using the RiftPulseRing sprite, and sets its transform.
    /// </summary>
    private static void EnsureChildRenderer(
        GameObject parent,
        string childName,
        Vector3 localPos,
        Vector3 localScale,
        float localRotZ,
        Color color,
        int sortingOrder)
    {
        Transform existing = parent.transform.Find(childName);
        GameObject go = existing != null
            ? existing.gameObject
            : new GameObject(childName);

        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition    = localPos;
        go.transform.localScale       = localScale;
        go.transform.localEulerAngles = new Vector3(0, 0, localRotZ);

        var sr = go.GetComponent<SpriteRenderer>() ?? go.AddComponent<SpriteRenderer>();

        // Load the ring sprite
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Sprites/Enemies/RiftPulseRing-Photoroom.png")
            as Sprite[];

        if (sprites != null && sprites.Length > 0)
        {
            foreach (var s in sprites)
                if (s is Sprite sp) { sr.sprite = sp; break; }
        }

        sr.color        = color;
        sr.sortingOrder = sortingOrder;
    }

    // ─────────────────────────────────────────────────────────────────────
    // CURVE HELPERS
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Linear Z-rotation from startAngle to endAngle over <paramref name="duration"/> s, loops.</summary>
    private static void SetEulerZLinearLoop(AnimationClip clip, string path, float startAngle, float endAngle, float duration)
    {
        var curve = new AnimationCurve(
            new Keyframe(0f,        startAngle, 0, (endAngle - startAngle) / duration),
            new Keyframe(duration,  endAngle,   (endAngle - startAngle) / duration, 0)
        );
        clip.SetCurve(path, typeof(Transform), "localEulerAngles.z", curve);
    }

    /// <summary>Scale X and Y ping-pong between min and max over <paramref name="duration"/> s.</summary>
    private static void SetScalePingPong(AnimationClip clip, string path, float min, float max, float duration)
    {
        float half = duration * 0.5f;
        var curve = new AnimationCurve(
            new Keyframe(0f,        min,  0, 0),
            new Keyframe(half,      max,  0, 0),
            new Keyframe(duration,  min,  0, 0)
        );
        SetKeyframeTangentsAuto(curve);
        clip.SetCurve(path, typeof(Transform), "m_LocalScale.x", curve);
        clip.SetCurve(path, typeof(Transform), "m_LocalScale.y", curve);
    }

    /// <summary>Scale X only ping-pong (for trail stretch).</summary>
    private static void SetScaleXPingPong(AnimationClip clip, string path, float min, float max, float duration)
    {
        float half = duration * 0.5f;
        var curve = new AnimationCurve(
            new Keyframe(0f,   min, 0, 0),
            new Keyframe(half, max, 0, 0),
            new Keyframe(duration, min, 0, 0)
        );
        SetKeyframeTangentsAuto(curve);
        clip.SetCurve(path, typeof(Transform), "m_LocalScale.x", curve);
    }

    /// <summary>Scale X constant (for trail locked).</summary>
    private static void SetScaleXConstant(AnimationClip clip, string path, float value, float duration)
    {
        var curve = AnimationCurve.Constant(0f, duration, value);
        clip.SetCurve(path, typeof(Transform), "m_LocalScale.x", curve);
    }

    /// <summary>SpriteRenderer alpha ping-pong between min and max over duration.</summary>
    private static void SetAlphaPingPong(AnimationClip clip, string path, float min, float max, float duration)
    {
        float half = duration * 0.5f;
        var curve = new AnimationCurve(
            new Keyframe(0f,        min, 0, 0),
            new Keyframe(half,      max, 0, 0),
            new Keyframe(duration,  min, 0, 0)
        );
        SetKeyframeTangentsAuto(curve);
        clip.SetCurve(path, typeof(SpriteRenderer), "m_Color.a", curve);
    }

    /// <summary>SpriteRenderer alpha constant.</summary>
    private static void SetAlphaConstant(AnimationClip clip, string path, float value, float duration)
    {
        var curve = AnimationCurve.Constant(0f, duration, value);
        clip.SetCurve(path, typeof(SpriteRenderer), "m_Color.a", curve);
    }

    /// <summary>Fade SpriteRenderer alpha from <paramref name="from"/> to <paramref name="to"/> over <paramref name="duration"/>.</summary>
    private static void FadeAlpha(AnimationClip clip, string path, float from, float to, float duration)
    {
        var curve = new AnimationCurve(
            new Keyframe(0f,       from, 0, (to - from) / duration),
            new Keyframe(duration, to,   (to - from) / duration, 0)
        );
        clip.SetCurve(path, typeof(SpriteRenderer), "m_Color.a", curve);
    }

    /// <summary>Fade Transform uniform scale from <paramref name="from"/> to <paramref name="to"/> over <paramref name="duration"/>.</summary>
    private static void FadeScale(AnimationClip clip, string path, float from, float to, float duration)
    {
        var curve = new AnimationCurve(
            new Keyframe(0f,       from, 0, 0),
            new Keyframe(duration, to,   0, 0)
        );
        SetKeyframeTangentsAuto(curve);
        clip.SetCurve(path, typeof(Transform), "m_LocalScale.x", curve);
        clip.SetCurve(path, typeof(Transform), "m_LocalScale.y", curve);
    }

    private static void SetKeyframeTangentsAuto(AnimationCurve curve)
    {
        for (int i = 0; i < curve.length; i++)
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
    }

    private static void SaveClip(AnimationClip clip, string path)
    {
        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing != null)
            EditorUtility.CopySerialized(clip, existing);
        else
            AssetDatabase.CreateAsset(clip, path);
    }
}
