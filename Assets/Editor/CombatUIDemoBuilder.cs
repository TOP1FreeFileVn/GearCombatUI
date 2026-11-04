#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CombatUIDemoBuilder
{
    const string Root = "Assets/CombatUI";
    const string ScenePath = Root + "/CombatUI_Demo.unity";

    [MenuItem("Combat UI/Rebuild Demo Scene")]
    public static void Build()
    {
        // move exported prefabs into Assets/CombatUI/Prefabs (one-time)
        if (AssetDatabase.IsValidFolder("Assets/ZZ_UIExport"))
        {
            if (!AssetDatabase.IsValidFolder(Root + "/Prefabs")) AssetDatabase.CreateFolder(Root, "Prefabs");
            foreach (var g in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/ZZ_UIExport" }))
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                AssetDatabase.MoveAsset(p, Root + "/Prefabs/" + System.IO.Path.GetFileName(p));
            }
            AssetDatabase.DeleteAsset("Assets/ZZ_UIExport");
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.2f, 0.45f, 0.65f); // stand-in for the sea background
        camGo.transform.position = new Vector3(0, 0, -10);

        GameObject Spawn(string name, Vector3 pos, float scaleMul = 1f)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{Root}/Prefabs/{name}.prefab");
            if (prefab == null) { Debug.LogWarning("missing prefab " + name); return null; }
            var anchor = new GameObject(name + " (anchor)");
            anchor.transform.position = pos;
            anchor.transform.localScale = Vector3.one * scaleMul;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, anchor.transform);
            go.SetActive(true);
            if (go.GetComponent<Canvas>()?.renderMode == RenderMode.WorldSpace) go.transform.localPosition = Vector3.zero;
            return go;
        }

        // main screen-space combat HUD
        var ui = Spawn("UI Canvas", Vector3.zero);
        var canvas = ui.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 5;
        Undo.RecordObject(ui, "init");
        // states normally set by gameplay scripts at runtime
        foreach (var t in ui.GetComponentsInChildren<Transform>(true))
        {
            if ((t.name == "ShopContainer" || t.name == "ButtonContainer") && t.localScale == Vector3.zero) t.localScale = Vector3.one;
            if (t.name == "TargetGoal") t.GetComponent<CanvasGroup>().alpha = 0; // level-start banner, hidden by default
        }
        var popups = ui.transform.Find("Popups");
        var sw = ui.AddComponent<UIPreviewSwitcher>();
        sw.popupsRoot = popups;
        sw.Show(-1);

        // world-space combat UI (normally attached to boat / enemies)
        // boat (player)
        // (offsets compensate for each prefab's internal child offsets)
        Spawn("BoatContainer_HealthBar", new Vector3(1.6f, -3.36f, 0));
        Spawn("BoatContainer_ShieldController", new Vector3(1.6f, -3.4f, 0));
        Spawn("BoatContainer_Boat_Status_Canvas", new Vector3(1.6f, -3.6f, 0));
        // enemies
        Spawn("Boss_HealthBar", new Vector3(0, 0.8f, 0));
        Spawn("Enemy_HealthBar", new Vector3(-1.3f, 1.5f, 0));
        Spawn("Enemy_Status_Canvas", new Vector3(-1.3f, 1.1f, 0));
        Spawn("Enemy_HealthBar", new Vector3(1.2f, 1.73f, 0));
        Spawn("PoolObject_DamageNumberFX_DamageText", new Vector3(-0.43f, 2.05f, 0));

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.defaultScreenWidth = 540;
        PlayerSettings.defaultScreenHeight = 960;
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Combat UI/Reimport Shaders")]
    public static void ReimportShaders()
    {
        foreach (var g in AssetDatabase.FindAssets("t:Shader", new[] { "Assets" }))
            AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(g), ImportAssetOptions.ForceUpdate);
    }

    // batch: -executeMethod CombatUIDemoBuilder.Shot -shot out.png [-popup N]
    public static void Shot()
    {
        var a = System.Environment.GetCommandLineArgs();
        string outPath = "shot.png"; int popup = -1;
        for (int i = 0; i < a.Length - 1; i++)
        {
            if (a[i] == "-shot") outPath = a[i + 1];
            if (a[i] == "-popup") popup = int.Parse(a[i + 1]);
        }
        EditorSceneManager.OpenScene(ScenePath);
        Object.FindObjectOfType<UIPreviewSwitcher>().Show(popup);
        var cam = Camera.main;
        var rt = new RenderTexture(1080, 1920, 24);
        cam.targetTexture = rt;
        Canvas.ForceUpdateCanvases();
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(1080, 1920, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 1080, 1920), 0, 0);
        tex.Apply();
        System.IO.File.WriteAllBytes(outPath, tex.EncodeToPNG());
        cam.targetTexture = null;
    }
}
#endif
