#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static partial class HouseSceneSetup
{
    private static GameObject CreateProp(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat, Vector3? euler = null)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = scale;
        if (euler.HasValue) go.transform.rotation = Quaternion.Euler(euler.Value);
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    private static void CreateDirtSpot(Transform parent, string name, Vector3 pos, Vector3 scale, ToolType tool, string objectiveId, Material mat, BuildContext ctx)
    {
        var go = CreateProp(parent, name, pos, scale, mat);
        var spot = go.AddComponent<CleanableSpot>();
        Configure(spot, ("requiredTool", (int)tool), ("requiredSeconds", 3.5f), ("objectiveId", objectiveId), ("dirtVisual", go));

        ctx.allSpots.Add(go);
        ctx.objectiveCounts[objectiveId] = ctx.objectiveCounts.TryGetValue(objectiveId, out int c) ? c + 1 : 1;
    }

    private static void BuildFurnitureAndDirt(Transform root, Materials mats, BuildContext ctx)
    {
        var furn = new GameObject("Furniture").transform;
        furn.SetParent(root, false);

        // Entrada
        CreateProp(furn, "EntryTable", new Vector3(1.3f, 0.4f, -1.3f), new Vector3(0.6f, 0.8f, 0.4f), mats.furniture);

        // Living
        CreateProp(furn, "Sofa", new Vector3(-2.5f, 0.4f, 4f), new Vector3(2.2f, 0.8f, 0.9f), mats.furniture);
        CreateProp(furn, "CoffeeTable", new Vector3(-2.5f, 0.25f, 5.5f), new Vector3(1f, 0.5f, 0.6f), mats.furniture);
        CreateProp(furn, "TVStand", new Vector3(-3.6f, 0.3f, 7.5f), new Vector3(1.2f, 0.6f, 0.4f), mats.furniture);
        CreateProp(furn, "TVScreen", new Vector3(-3.6f, 1f, 7.3f), new Vector3(1f, 0.7f, 0.05f), mats.entity);
        ctx.livingTvScreenOn = CreateProp(furn, "TVScreenOn", new Vector3(-3.6f, 1f, 7.27f), new Vector3(0.9f, 0.6f, 0.02f), MakeMat(new Color(0.6f, 0.7f, 0.8f)));
        ctx.livingTvScreenOn.SetActive(false);
        ctx.livingPicture = CreateProp(furn, "PictureFrame", new Vector3(-3.95f, 1.6f, 3f), new Vector3(0.03f, 0.6f, 0.5f), mats.furniture).transform;

        CreateDirtSpot(furn, "Dirt_Living_Carpet", new Vector3(-1f, 0.03f, 3f), new Vector3(0.6f, 0.05f, 0.6f), ToolType.Vacuum, "living", mats.dirt, ctx);
        CreateDirtSpot(furn, "Dirt_Living_Floor", new Vector3(0.5f, 0.03f, 6.5f), new Vector3(0.5f, 0.05f, 0.5f), ToolType.Broom, "living", mats.dirt, ctx);
        CreateDirtSpot(furn, "Dirt_Living_Window", new Vector3(-3.95f, 1.4f, 6.5f), new Vector3(0.03f, 0.6f, 0.8f), ToolType.Cloth, "living", mats.dirt, ctx);

        // Cocina
        CreateProp(furn, "Counter", new Vector3(6.6f, 0.5f, 5f), new Vector3(0.5f, 1f, 4f), mats.furniture);
        CreateProp(furn, "Fridge", new Vector3(2.4f, 1f, 3f), new Vector3(0.6f, 2f, 0.6f), mats.furniture);
        CreateProp(furn, "KitchenTable", new Vector3(4.5f, 0.4f, 6.5f), new Vector3(1.2f, 0.7f, 1.2f), mats.furniture);

        CreateDirtSpot(furn, "Dirt_Kitchen_Counter", new Vector3(6.5f, 1.05f, 4f), new Vector3(0.4f, 0.05f, 0.4f), ToolType.Vacuum, "kitchen", mats.dirt, ctx);
        CreateDirtSpot(furn, "Dirt_Kitchen_Table", new Vector3(4.5f, 0.76f, 6.5f), new Vector3(0.5f, 0.02f, 0.5f), ToolType.Cloth, "kitchen", mats.dirt, ctx);
        CreateDirtSpot(furn, "Dirt_Kitchen_Floor", new Vector3(3.5f, 0.03f, 7f), new Vector3(0.6f, 0.05f, 0.6f), ToolType.Broom, "kitchen", mats.dirt, ctx);

        // Pasillo
        CreateProp(furn, "HallwayTable", new Vector3(1.2f, 0.4f, 11.5f), new Vector3(0.4f, 0.8f, 0.4f), mats.furniture);

        // Baño
        CreateProp(furn, "Sink", new Vector3(-4.6f, 0.45f, 9f), new Vector3(0.5f, 0.9f, 0.4f), mats.furniture);
        CreateProp(furn, "Toilet", new Vector3(-4.6f, 0.25f, 11.5f), new Vector3(0.4f, 0.5f, 0.4f), mats.furniture);
        CreateProp(furn, "Mirror", new Vector3(-3f, 1.5f, 11.9f), new Vector3(0.6f, 0.6f, 0.03f), MakeMat(new Color(0.7f, 0.8f, 0.85f)));

        CreateDirtSpot(furn, "Dirt_Bathroom_Mirror", new Vector3(-3f, 1.5f, 11.87f), new Vector3(0.5f, 0.5f, 0.02f), ToolType.Cloth, "bathroom", mats.dirt, ctx);
        CreateDirtSpot(furn, "Dirt_Bathroom_Floor", new Vector3(-2f, 0.03f, 9.5f), new Vector3(0.5f, 0.05f, 0.5f), ToolType.Vacuum, "bathroom", mats.dirt, ctx);

        // Habitación
        CreateProp(furn, "Bed", new Vector3(5.5f, 0.3f, 10f), new Vector3(1.8f, 0.6f, 2.2f), mats.furniture);
        CreateProp(furn, "Pillow", new Vector3(5.5f, 0.65f, 9.1f), new Vector3(1.6f, 0.2f, 0.5f), MakeMat(Color.white));
        CreateProp(furn, "Nightstand", new Vector3(4f, 0.35f, 10f), new Vector3(0.5f, 0.7f, 0.5f), mats.furniture);
        CreateProp(furn, "Closet", new Vector3(2f, 1f, 13.8f), new Vector3(1f, 2f, 0.6f), mats.furniture);

        CreateDirtSpot(furn, "Dirt_Bedroom_Nightstand", new Vector3(4f, 0.72f, 10f), new Vector3(0.4f, 0.04f, 0.4f), ToolType.Vacuum, "bedroom", mats.dirt, ctx);
        CreateDirtSpot(furn, "Dirt_Bedroom_Floor", new Vector3(6f, 0.03f, 13f), new Vector3(0.5f, 0.05f, 0.5f), ToolType.Broom, "bedroom", mats.dirt, ctx);
    }

    private static void BuildLights(Transform root, BuildContext ctx)
    {
        var lightsRoot = new GameObject("Lights").transform;
        lightsRoot.SetParent(root, false);

        CreatePointLight(lightsRoot, "Light_Entrada", new Vector3(0f, 2.3f, 0f), 5f, 1.1f, new Color(1f, 0.92f, 0.8f));
        CreatePointLight(lightsRoot, "Light_Living_1", new Vector3(-2.5f, 2.3f, 4f), 6f, 1.1f, new Color(1f, 0.92f, 0.8f));
        CreatePointLight(lightsRoot, "Light_Living_2", new Vector3(-1f, 2.3f, 7f), 6f, 1f, new Color(1f, 0.92f, 0.8f));
        CreatePointLight(lightsRoot, "Light_Cocina", new Vector3(4.5f, 2.3f, 5f), 6f, 1.2f, new Color(1f, 0.95f, 0.85f));
        CreatePointLight(lightsRoot, "Light_Bano", new Vector3(-3.2f, 2.3f, 10f), 5f, 1.1f, new Color(0.95f, 0.98f, 1f));
        CreatePointLight(lightsRoot, "Light_Habitacion", new Vector3(4f, 2.3f, 11.5f), 6f, 1f, new Color(1f, 0.9f, 0.78f));

        var pasilloLightGo = CreatePointLight(lightsRoot, "Light_Pasillo", new Vector3(0f, 2.4f, 10f), 5f, 1f, new Color(1f, 0.92f, 0.8f));
        ctx.pasilloLight = pasilloLightGo.GetComponent<Light>();

        var basement1 = CreatePointLight(lightsRoot, "Light_Basement_1", new Vector3(-1.5f, -0.9f, 19f), 5f, 0.6f, new Color(0.8f, 0.75f, 0.7f));
        var flicker1 = basement1.AddComponent<FlickeringLight>();
        Configure(flicker1, ("targetLight", basement1.GetComponent<Light>()), ("minIntensity", 0.15f), ("maxIntensity", 0.65f), ("noiseSpeed", 4f));

        var basement2 = CreatePointLight(lightsRoot, "Light_Basement_2", new Vector3(1.5f, -0.9f, 23f), 5f, 0.5f, new Color(0.75f, 0.7f, 0.65f));
        var flicker2 = basement2.AddComponent<FlickeringLight>();
        Configure(flicker2, ("targetLight", basement2.GetComponent<Light>()), ("minIntensity", 0.1f), ("maxIntensity", 0.55f), ("noiseSpeed", 5f));
    }

    private static GameObject CreatePointLight(Transform parent, string name, Vector3 pos, float range, float intensity, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        var l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.range = range;
        l.intensity = intensity;
        l.color = color;
        return go;
    }
}
#endif
