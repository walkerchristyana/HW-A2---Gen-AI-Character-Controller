using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// Opens the existing populated scene. This does not construct the neighborhood.
[InitializeOnLoad]
public static class ProjectStartup
{
    const string ScenePath = "Assets/Scenes/Neighborhood.unity";
    static string FirstOpenKey => "JuniperBlock.Complete2D.Opened." + Application.dataPath;

    static ProjectStartup() { EditorApplication.delayCall += FirstOpen; }
    static void FirstOpen()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += FirstOpen;
            return;
        }
        if (EditorPrefs.GetBool(FirstOpenKey, false)) return;
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        PlayerSettings.productName = "Juniper Block";
        PlayerSettings.companyName = "ChrissyGames";
        PlayerSettings.defaultScreenWidth = 1024;
        PlayerSettings.defaultScreenHeight = 576;
        PlayerSettings.resizableWindow = true;
        QualitySettings.antiAliasing = 0;
        // Native animation import rebind: let this installed Unity editor compute
        // its own animation curve binding data while preserving all asset GUIDs.
        RefreshAnimationBindings();
        if (File.Exists(ScenePath) && !SceneManager.GetActiveScene().isDirty)
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        FrameScene();
        AssetDatabase.SaveAssets();
        EditorPrefs.SetBool(FirstOpenKey, true);
    }
    static void RefreshAnimationBindings()
    {
        foreach (string state in new[] {"Idle", "Walk"})
        foreach (string direction in new[] {"Down", "Left", "Right", "Up"})
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/" + state + direction + ".anim");
            if (clip == null) continue;
            bool walking = state == "Walk";
            var frames = new ObjectReferenceKeyframe[walking ? 5 : 2];
            for (int i = 0; i < frames.Length; i++)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Characters/Boy_" + direction + "_" + (walking ? i % 4 : 0) + ".png");
                frames[i] = new ObjectReferenceKeyframe { time = walking ? i / 8f : i / 2f, value = sprite };
            }
            AnimationUtility.SetObjectReferenceCurve(clip,
                new EditorCurveBinding { path = "", type = typeof(SpriteRenderer), propertyName = "m_Sprite" }, frames);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
        }
    }
    [MenuItem("Juniper Block/Open Neighborhood Scene")]
    public static void OpenScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        FrameScene();
    }
    static void FrameScene()
    {
        foreach (SceneView view in SceneView.sceneViews)
        {
            view.in2DMode = true;
            view.LookAt(Vector3.zero, Quaternion.identity, 27, true, true);
        }
    }
}
