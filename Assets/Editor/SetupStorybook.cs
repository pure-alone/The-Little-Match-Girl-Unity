#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class SetupStorybook
{
    const string ScenePath = "Assets/Main.unity";

    static SetupStorybook()
    {
        EditorApplication.delayCall += EnsureScene;
    }

    [MenuItem("Tools/Little Match Girl/Rebuild Main Scene")]
    public static void EnsureScene()
    {
        if (System.IO.File.Exists(ScenePath))
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root = new GameObject("LittleMatchGirlStorybook");
        root.AddComponent<StorybookController>();
        EditorSceneManager.SaveScene(scene, ScenePath);

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.productName = "The Little Match Girl";
        PlayerSettings.companyName = "PROG2006 Storybook";
        PlayerSettings.defaultScreenWidth = 720;
        PlayerSettings.defaultScreenHeight = 1280;
        Debug.Log("Little Match Girl scene created at Assets/Main.unity");
    }

    [MenuItem("Tools/Little Match Girl/Build WebGL")]
    public static void BuildWebGL()
    {
        EnsureScene();
        const string output = "WebGLBuild";
        if (!System.IO.Directory.Exists(output)) System.IO.Directory.CreateDirectory(output);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = output,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildPipeline.BuildPlayer(options);
        Debug.Log("WebGL build finished in: " + output);
    }
}
#endif
