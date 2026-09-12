#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// One-click build of the whole "Limpieza Paranormal" vertical slice: house geometry, tools,
/// dirt, objectives, paranormal systems, entity, UI and player, all wired up, saved into a
/// dedicated scene. Run from Tools/LimpiezaParanormal/Construir Casa (Vertical Slice).
/// </summary>
public static partial class HouseSceneSetup
{
    private const string ScenePath = "Assets/_Project/Scenes/LimpiezaParanormal.unity";
    private const string SettingsPath = "Assets/_Project/Settings/";
    private const string InputActionsPath = "Assets/_Project/Input/PlayerControls.inputactions";

    internal const int PlayerLayer = 8;
    internal const int GroundLayer = 9;
    internal const int ViewmodelLayer = 10;

    internal const float WallHeight = 2.6f;
    internal const float WallThickness = 0.15f;

    [MenuItem("Tools/LimpiezaParanormal/Construir Casa (Vertical Slice)")]
    public static void Setup()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("HouseSceneSetup: salí de Play Mode antes de correr esto.");
            return;
        }

        var movementSettings = AssetDatabase.LoadAssetAtPath<MovementSettings>(SettingsPath + "MovementSettings.asset");
        var lookSettings = AssetDatabase.LoadAssetAtPath<LookSettings>(SettingsPath + "LookSettings.asset");
        var viewmodelSettings = AssetDatabase.LoadAssetAtPath<ViewmodelSettings>(SettingsPath + "ViewmodelSettings.asset");
        var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);

        if (movementSettings == null || lookSettings == null || viewmodelSettings == null || inputActions == null)
        {
            Debug.LogError("HouseSceneSetup: no encontré uno o más assets esperados en " +
                            $"'{SettingsPath}' o '{InputActionsPath}'. Dejá que el proyecto termine de compilar y reintentá.");
            return;
        }

        var paranormalSettings = EnsureAsset<ParanormalSettings>(SettingsPath + "ParanormalSettings.asset");
        var entitySettings = EnsureAsset<EntitySettings>(SettingsPath + "EntitySettings.asset");

        if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
        }

        Scene scene;
        if (System.IO.File.Exists(ScenePath))
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (GameObject.Find("House_Root") != null)
            {
                Debug.LogWarning("HouseSceneSetup: la escena ya tiene 'House_Root' construido. Borrá la escena " +
                                  "(o los objetos House_Root/Systems/Player/HUD_Canvas) si querés reconstruir desde cero.");
                return;
            }
        }
        else
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        for (int otherLayer = 0; otherLayer < 32; otherLayer++)
        {
            Physics.IgnoreLayerCollision(ViewmodelLayer, otherLayer, true);
        }

        PlayerSceneSetup.EnsureDirectionalLight();
        foreach (var l in Object.FindObjectsOfType<Light>())
        {
            if (l.type == LightType.Directional)
            {
                l.intensity = 0.15f;
                l.color = new Color(0.55f, 0.6f, 0.75f);
            }
        }

        var navStatics = new List<GameObject>();
        var houseRoot = new GameObject("House_Root").transform;

        var mats = BuildMaterials();

        var context = BuildRooms(houseRoot, mats, navStatics);

        // Bake the NavMesh from the room shell right away, before any NavMeshAgent exists,
        // so the entity never has to warp onto navigation data that isn't there yet.
        foreach (var go in navStatics)
        {
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.NavigationStatic);
        }
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();

        BuildFurnitureAndDirt(houseRoot, mats, context);
        BuildLights(houseRoot, context);

        var eventRefs = BuildParanormalEvents(houseRoot, context);
        var hidingSpots = BuildHidingSpots(houseRoot, context);

        var uiRefs = BuildUI();

        GameObject player = PlayerSceneSetup.BuildPlayer(movementSettings, lookSettings, viewmodelSettings, inputActions, new Vector3(0f, 1f, -1f));
        var playerRefs = ExtendPlayer(player);

        GameObject entityGo = BuildEntity(houseRoot, entitySettings, player.transform, hidingSpots);

        var systemsRoot = new GameObject("Systems").transform;
        BuildSystems(systemsRoot, context, eventRefs, uiRefs, playerRefs, entityGo, paranormalSettings);

        WireUI(uiRefs, playerRefs, systemsRoot);

        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(scene, ScenePath);
        EnsureSceneInBuildSettings(ScenePath);

        Selection.activeGameObject = player;
        EditorGUIUtility.PingObject(player);

        Debug.Log("HouseSceneSetup: listo. La escena 'LimpiezaParanormal' quedó guardada en " + ScenePath +
                   ". Dale Play. WASD para moverse, mouse para mirar, E para interactuar, click " +
                   "izquierdo para usar la herramienta equipada, 1-4 para cambiar de herramienta, F3 debug overlay.");
    }

    private static T EnsureAsset<T>(string path) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;

        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void EnsureSceneInBuildSettings(string path)
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(s => s.path == path)) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // ---------------------------------------------------------------- Generic wiring helpers

    private static void Configure(Object target, params (string field, object value)[] values)
    {
        var so = new SerializedObject(target);
        foreach (var (field, value) in values)
        {
            var p = so.FindProperty(field);
            if (p == null)
            {
                Debug.LogError($"HouseSceneSetup: campo '{field}' no encontrado en {target.GetType().Name}.");
                continue;
            }
            if (value == null) { p.objectReferenceValue = null; continue; }
            switch (value)
            {
                case float f: p.floatValue = f; break;
                case int i: p.intValue = i; break;
                case bool b: p.boolValue = b; break;
                case string s: p.stringValue = s; break;
                case Color c: p.colorValue = c; break;
                case Object o: p.objectReferenceValue = o; break;
                default: Debug.LogError($"HouseSceneSetup: tipo no soportado para '{field}': {value.GetType()}"); break;
            }
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ConfigureObjectArray(Object target, string field, Object[] values)
    {
        var so = new SerializedObject(target);
        var p = so.FindProperty(field);
        if (p == null)
        {
            Debug.LogError($"HouseSceneSetup: campo de lista '{field}' no encontrado en {target.GetType().Name}.");
            return;
        }
        p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ConfigureObjectives(ObjectiveManager mgr, List<(string id, string label, int count)> defs)
    {
        var so = new SerializedObject(mgr);
        var p = so.FindProperty("objectives");
        p.arraySize = defs.Count;
        for (int i = 0; i < defs.Count; i++)
        {
            var el = p.GetArrayElementAtIndex(i);
            el.FindPropertyRelative("id").stringValue = defs[i].id;
            el.FindPropertyRelative("label").stringValue = defs[i].label;
            el.FindPropertyRelative("requiredCount").intValue = defs[i].count;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform) SetLayerRecursive(child.gameObject, layer);
    }

    private static Material MakeMat(Color color)
    {
        var mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        return mat;
    }
}
#endif
