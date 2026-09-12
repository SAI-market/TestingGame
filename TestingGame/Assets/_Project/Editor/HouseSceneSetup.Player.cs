#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static partial class HouseSceneSetup
{
    public class PlayerRefs
    {
        public PlayerInteractor interactor;
        public ToolController toolController;
        public FlashlightController flashlight;
        public PlayerInputReader input;
    }

    private static PlayerRefs ExtendPlayer(GameObject player)
    {
        var refs = new PlayerRefs { input = player.GetComponent<PlayerInputReader>() };

        Transform mainCameraT = player.transform.Find("CameraPivot/HeadBobPivot/ImpulsePivot/MainCamera");
        Transform viewmodelRootT = player.transform.Find("CameraPivot/HeadBobPivot/ImpulsePivot/ViewmodelCamera/ViewmodelRoot");

        refs.interactor = player.AddComponent<PlayerInteractor>();
        Configure(refs.interactor, ("rayOrigin", mainCameraT), ("input", refs.input), ("maxDistance", 2.5f));

        GameObject vacuum = BuildVacuumVisual(viewmodelRootT);
        GameObject broom = BuildBroomVisual(viewmodelRootT);
        GameObject cloth = BuildClothVisual(viewmodelRootT);
        GameObject flashlightVisual = BuildFlashlightVisual(viewmodelRootT, out Light flashlightLight);

        refs.flashlight = flashlightVisual.AddComponent<FlashlightController>();
        Configure(refs.flashlight, ("spotLight", flashlightLight), ("maxBattery", 100f), ("drainPerSecond", 3.5f));

        refs.toolController = player.AddComponent<ToolController>();
        ConfigureObjectArray(refs.toolController, "toolVisuals", new Object[] { vacuum, broom, cloth, flashlightVisual });
        Configure(refs.toolController, ("input", refs.input), ("interactor", refs.interactor), ("flashlight", refs.flashlight));

        return refs;
    }

    private static GameObject BuildVacuumVisual(Transform parent)
    {
        var root = new GameObject("Tool_Vacuum");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = new Vector3(0.05f, -0.15f, 0.4f);

        var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Canister";
        body.transform.SetParent(root.transform, false);
        body.transform.localScale = new Vector3(0.1f, 0.18f, 0.1f);
        body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        Object.DestroyImmediate(body.GetComponent<Collider>());
        body.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.6f, 0.1f, 0.1f));

        var nozzle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        nozzle.name = "Nozzle";
        nozzle.transform.SetParent(root.transform, false);
        nozzle.transform.localPosition = new Vector3(0f, 0f, 0.25f);
        nozzle.transform.localScale = new Vector3(0.03f, 0.22f, 0.03f);
        nozzle.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        Object.DestroyImmediate(nozzle.GetComponent<Collider>());
        nozzle.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.2f, 0.2f, 0.2f));

        SetLayerRecursive(root, ViewmodelLayer);
        return root;
    }

    private static GameObject BuildBroomVisual(Transform parent)
    {
        var root = new GameObject("Tool_Broom");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = new Vector3(0.05f, -0.2f, 0.4f);

        var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        handle.name = "Handle";
        handle.transform.SetParent(root.transform, false);
        handle.transform.localScale = new Vector3(0.015f, 0.35f, 0.015f);
        handle.transform.localRotation = Quaternion.Euler(80f, 0f, 0f);
        Object.DestroyImmediate(handle.GetComponent<Collider>());
        handle.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.5f, 0.35f, 0.2f));

        var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "Head";
        head.transform.SetParent(root.transform, false);
        head.transform.localPosition = new Vector3(0f, -0.05f, 0.62f);
        head.transform.localScale = new Vector3(0.22f, 0.06f, 0.05f);
        Object.DestroyImmediate(head.GetComponent<Collider>());
        head.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.85f, 0.7f, 0.3f));

        SetLayerRecursive(root, ViewmodelLayer);
        return root;
    }

    private static GameObject BuildClothVisual(Transform parent)
    {
        var root = new GameObject("Tool_Cloth");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = new Vector3(0.1f, -0.1f, 0.38f);

        var cloth = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cloth.name = "ClothMesh";
        cloth.transform.SetParent(root.transform, false);
        cloth.transform.localScale = new Vector3(0.18f, 0.02f, 0.18f);
        Object.DestroyImmediate(cloth.GetComponent<Collider>());
        cloth.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.9f, 0.9f, 0.85f));

        SetLayerRecursive(root, ViewmodelLayer);
        return root;
    }

    private static GameObject BuildFlashlightVisual(Transform parent, out Light spotLight)
    {
        var root = new GameObject("Tool_Flashlight");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = new Vector3(0.05f, -0.1f, 0.4f);

        var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Body";
        body.transform.SetParent(root.transform, false);
        body.transform.localScale = new Vector3(0.03f, 0.12f, 0.03f);
        body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        Object.DestroyImmediate(body.GetComponent<Collider>());
        body.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.15f, 0.15f, 0.15f));

        var lightGo = new GameObject("Beam");
        lightGo.transform.SetParent(root.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 0f, 0.15f);
        spotLight = lightGo.AddComponent<Light>();
        spotLight.type = LightType.Spot;
        spotLight.range = 12f;
        spotLight.spotAngle = 45f;
        spotLight.intensity = 2.2f;
        spotLight.color = new Color(1f, 0.95f, 0.85f);
        spotLight.shadows = LightShadows.Soft;
        spotLight.enabled = false;

        SetLayerRecursive(root, ViewmodelLayer);
        return root;
    }
}
#endif
