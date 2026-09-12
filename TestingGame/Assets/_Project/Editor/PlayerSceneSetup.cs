#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// One-click bootstrap: builds the Player hierarchy (Fase 1-6 of PLAN-Movimiento3D.md) and a
/// small test playground (ramps, steps, corridor, cliff edge) in the currently open scene, and
/// wires every serialized reference. Run from Tools/TestingGame/Setup Player + Test Scene, then
/// save the scene by hand.
/// </summary>
public static class PlayerSceneSetup
{
    private const string SettingsPath = "Assets/_Project/Settings/";
    private const string InputActionsPath = "Assets/_Project/Input/PlayerControls.inputactions";

    private const int PlayerLayer = 8;
    private const int GroundLayer = 9;
    private const int ViewmodelLayer = 10;

    [MenuItem("Tools/TestingGame/Setup Player + Test Scene")]
    public static void Setup()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("PlayerSceneSetup: salí de Play Mode antes de correr esto.");
            return;
        }

        if (GameObject.Find("Player") != null)
        {
            Debug.LogWarning("PlayerSceneSetup: ya existe un GameObject 'Player' en la escena activa. " +
                              "Borralo (junto con 'Playground_Geometry' si también existe) y volvé a correr esto si querés reconstruir desde cero.");
            return;
        }

        var movementSettings = AssetDatabase.LoadAssetAtPath<MovementSettings>(SettingsPath + "MovementSettings.asset");
        var lookSettings = AssetDatabase.LoadAssetAtPath<LookSettings>(SettingsPath + "LookSettings.asset");
        var viewmodelSettings = AssetDatabase.LoadAssetAtPath<ViewmodelSettings>(SettingsPath + "ViewmodelSettings.asset");
        var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);

        if (movementSettings == null || lookSettings == null || viewmodelSettings == null || inputActions == null)
        {
            Debug.LogError("PlayerSceneSetup: no encontré uno o más assets esperados en " +
                            $"'{SettingsPath}' o '{InputActionsPath}'. Revisá que el proyecto haya terminado de importar.");
            return;
        }

        for (int otherLayer = 0; otherLayer < 32; otherLayer++)
        {
            Physics.IgnoreLayerCollision(ViewmodelLayer, otherLayer, true);
        }

        EnsureDirectionalLight();
        BuildGeometry();
        GameObject player = BuildPlayer(movementSettings, lookSettings, viewmodelSettings, inputActions);

        AssetDatabase.SaveAssets();

        Selection.activeGameObject = player;
        EditorGUIUtility.PingObject(player);

        Debug.Log("PlayerSceneSetup: listo. Guardá la escena (Ctrl+S) y dale Play. " +
                   "F3 muestra el overlay de debug. Revisá SETUP-Editor.md si algo no se ve bien.");
    }

    private static void EnsureDirectionalLight()
    {
        foreach (var light in Object.FindObjectsOfType<Light>())
        {
            if (light.type == LightType.Directional) return;
        }

        var lightGo = new GameObject("Directional Light");
        var l = lightGo.AddComponent<Light>();
        l.type = LightType.Directional;
        l.intensity = 1f;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    // ---------------------------------------------------------------- Geometry

    private static void BuildGeometry()
    {
        var root = new GameObject("Playground_Geometry").transform;

        // Floor spans X -45..45, Z -15..25. The open edge at Z=25 (nothing beyond) is the
        // cliff for coyote-time testing; walk toward +Z from any lane to reach it.
        CreateGroundBox("Floor", root, new Vector3(0f, -0.5f, 5f), new Vector3(90f, 1f, 40f));

        // Safety net so falling off the cliff (or any bug) doesn't fall forever.
        CreateGroundBox("CatchFloor", root, new Vector3(0f, -15f, 5f), new Vector3(120f, 1f, 150f));

        CreateRamp(root, "Ramp_15deg", new Vector3(-25f, 0f, 0f), 15f, 8f, 0.3f, 4f);
        CreateRamp(root, "Ramp_30deg", new Vector3(-15f, 0f, 0f), 30f, 8f, 0.3f, 4f);
        CreateRamp(root, "Ramp_50deg", new Vector3(-5f, 0f, 0f), 50f, 6f, 0.3f, 4f);

        CreateSteps(root, "Steps_0_2", new Vector3(5f, 0f, 0f), 0.2f, 1.2f, 4, 4f);
        CreateSteps(root, "Steps_0_4", new Vector3(15f, 0f, 0f), 0.4f, 1.4f, 3, 4f);

        CreateGroundBox("Corridor_WallLeft", root, new Vector3(25f - 1.05f, 1.5f, 5f), new Vector3(0.3f, 3f, 8f));
        CreateGroundBox("Corridor_WallRight", root, new Vector3(25f + 1.05f, 1.5f, 5f), new Vector3(0.3f, 3f, 8f));

        CreateGroundBox("TestBox", root, new Vector3(35f, 0.5f, 3f), new Vector3(1f, 1f, 1f));
    }

    private static GameObject CreateGroundBox(string name, Transform parent, Vector3 position, Vector3 scale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.layer = GroundLayer;
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.localScale = scale;
        return go;
    }

    /// <summary>
    /// Rotates around the near-bottom edge of the ramp (a line parallel to world X passing
    /// through nearBottomEdgeWorldPos), so that edge stays pinned to the floor regardless of
    /// tilt direction. If it climbs the wrong way, flip the sign of angleDeg by hand.
    /// </summary>
    private static void CreateRamp(Transform parent, string name, Vector3 nearBottomEdgeWorldPos, float angleDeg, float length, float thickness, float width)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.layer = GroundLayer;
        go.transform.SetParent(parent, false);
        go.transform.localScale = new Vector3(width, thickness, length);
        go.transform.position = nearBottomEdgeWorldPos + new Vector3(0f, thickness * 0.5f, length * 0.5f);
        go.transform.RotateAround(nearBottomEdgeWorldPos, Vector3.right, -angleDeg);
    }

    private static void CreateSteps(Transform parent, string namePrefix, Vector3 startPos, float riseHeight, float treadDepth, int count, float width)
    {
        for (int i = 0; i < count; i++)
        {
            float height = riseHeight * (i + 1);
            Vector3 pos = startPos + new Vector3(0f, height * 0.5f, treadDepth * i + treadDepth * 0.5f);
            CreateGroundBox($"{namePrefix}_{i}", parent, pos, new Vector3(width, height, treadDepth));
        }
    }

    // ---------------------------------------------------------------- Player hierarchy

    private static GameObject BuildPlayer(MovementSettings movementSettings, LookSettings lookSettings, ViewmodelSettings viewmodelSettings, InputActionAsset inputActions)
    {
        var player = new GameObject("Player");
        player.layer = PlayerLayer;
        player.transform.position = new Vector3(0f, 1f, -10f);

        var controller = player.AddComponent<CharacterController>();
        controller.radius = 0.35f;
        controller.height = movementSettings.standHeight;
        controller.center = new Vector3(0f, movementSettings.standHeight * 0.5f, 0f);
        controller.stepOffset = 0.35f;
        controller.slopeLimit = movementSettings.maxSlopeAngle;
        controller.skinWidth = 0.05f;

        var inputReader = player.AddComponent<PlayerInputReader>();
        SetField(inputReader, "inputActions", inputActions);

        GameObject cameraPivotGo = CreateChild(player.transform, "CameraPivot", new Vector3(0f, movementSettings.standHeight - 0.15f, 0f));
        GameObject headBobPivotGo = CreateChild(cameraPivotGo.transform, "HeadBobPivot", Vector3.zero);
        GameObject impulsePivotGo = CreateChild(headBobPivotGo.transform, "ImpulsePivot", Vector3.zero);

        GameObject mainCameraGo = CreateChild(impulsePivotGo.transform, "MainCamera", Vector3.zero);
        var mainCamera = mainCameraGo.AddComponent<Camera>();
        mainCamera.fieldOfView = 70f;
        mainCamera.clearFlags = CameraClearFlags.Skybox;
        mainCamera.cullingMask = ~(1 << ViewmodelLayer);
        mainCamera.depth = 0;
        mainCameraGo.AddComponent<AudioListener>();
        mainCameraGo.tag = "MainCamera";

        GameObject viewmodelCameraGo = CreateChild(impulsePivotGo.transform, "ViewmodelCamera", Vector3.zero);
        var viewmodelCamera = viewmodelCameraGo.AddComponent<Camera>();
        viewmodelCamera.fieldOfView = 58f;
        viewmodelCamera.clearFlags = CameraClearFlags.Depth;
        viewmodelCamera.cullingMask = 1 << ViewmodelLayer;
        viewmodelCamera.depth = 1;
        viewmodelCamera.nearClipPlane = 0.01f;

        GameObject viewmodelRootGo = CreateChild(viewmodelCameraGo.transform, "ViewmodelRoot", new Vector3(0.25f, -0.25f, 0.4f));
        BuildPlaceholderHands(viewmodelRootGo.transform);

        var look = player.AddComponent<PlayerLook>();
        SetField(look, "settings", lookSettings);
        SetField(look, "cameraPivot", cameraPivotGo.transform);
        SetField(look, "input", inputReader);

        var crouch = player.AddComponent<PlayerCrouch>();
        SetField(crouch, "settings", movementSettings);
        SetField(crouch, "input", inputReader);
        SetField(crouch, "cameraPivot", cameraPivotGo.transform);

        var motor = player.AddComponent<PlayerMotor>();
        SetField(motor, "settings", movementSettings);
        SetField(motor, "input", inputReader);
        SetField(motor, "crouch", crouch);

        var state = player.AddComponent<PlayerState>();
        SetField(state, "motor", motor);
        SetField(state, "crouch", crouch);

        var headBob = headBobPivotGo.AddComponent<HeadBob>();
        SetField(headBob, "motor", motor);
        SetField(headBob, "state", state);

        var impulse = impulsePivotGo.AddComponent<CameraImpulse>();
        SetField(impulse, "motor", motor);

        var fovKick = mainCameraGo.AddComponent<CameraFovKick>();
        SetField(fovKick, "motor", motor);

        var sway = viewmodelRootGo.AddComponent<ViewmodelSway>();
        SetField(sway, "settings", viewmodelSettings);
        SetField(sway, "input", inputReader);
        SetField(sway, "motor", motor);

        var animDriver = viewmodelRootGo.AddComponent<ViewmodelAnimatorDriver>();
        SetField(animDriver, "motor", motor);

        var audioSource = player.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        var footsteps = player.AddComponent<FootstepAudio>();
        SetField(footsteps, "headBob", headBob);
        SetField(footsteps, "motor", motor);

        var debugOverlay = player.AddComponent<PlayerDebugOverlay>();
        SetField(debugOverlay, "motor", motor);
        SetField(debugOverlay, "state", state);

        return player;
    }

    private static void BuildPlaceholderHands(Transform parent)
    {
        CreateHandCube(parent, "Hand_L_Placeholder", new Vector3(-0.15f, 0f, 0.3f));
        CreateHandCube(parent, "Hand_R_Placeholder", new Vector3(0.15f, 0f, 0.3f));
    }

    private static void CreateHandCube(Transform parent, string name, Vector3 localPosition)
    {
        var hand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hand.name = name;
        hand.layer = ViewmodelLayer;
        hand.transform.SetParent(parent, false);
        hand.transform.localPosition = localPosition;
        hand.transform.localScale = new Vector3(0.08f, 0.08f, 0.35f);
        Object.DestroyImmediate(hand.GetComponent<Collider>());
    }

    private static GameObject CreateChild(Transform parent, string name, Vector3 localPosition)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        return go;
    }

    private static void SetField(Object target, string fieldName, Object value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop == null)
        {
            Debug.LogError($"PlayerSceneSetup: no encontré el campo '{fieldName}' en {target.GetType().Name}.");
            return;
        }
        prop.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
#endif
