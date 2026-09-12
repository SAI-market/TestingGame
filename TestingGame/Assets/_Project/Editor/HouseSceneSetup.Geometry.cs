#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static partial class HouseSceneSetup
{
    public class Materials
    {
        public Material floor, wall, ceiling, basementWall, furniture, dirt, door, entity, shadow;
    }

    public class BuildContext
    {
        public SimpleDoor frontDoor;
        public SimpleDoor basementDoor;
        public SimpleDoor banoDoor;
        public SimpleDoor habitacionDoor;
        public Transform livingPicture;
        public GameObject livingTvScreenOn;
        public Light pasilloLight;
        public Vector3 hallwayWestPoint = new Vector3(-1.3f, 1.6f, 10f);
        public Vector3 hallwayEastPoint = new Vector3(1.3f, 1.6f, 10f);
        public Dictionary<string, int> objectiveCounts = new Dictionary<string, int>();
        public List<Vector3> hidingSpotPositions = new List<Vector3>();
        public List<GameObject> allSpots = new List<GameObject>();
    }

    private static Materials BuildMaterials()
    {
        return new Materials
        {
            floor = MakeMat(new Color(0.5f, 0.4f, 0.32f)),
            wall = MakeMat(new Color(0.82f, 0.79f, 0.72f)),
            ceiling = MakeMat(new Color(0.88f, 0.88f, 0.86f)),
            basementWall = MakeMat(new Color(0.22f, 0.21f, 0.19f)),
            furniture = MakeMat(new Color(0.38f, 0.26f, 0.17f)),
            dirt = MakeMat(new Color(0.18f, 0.16f, 0.11f)),
            door = MakeMat(new Color(0.45f, 0.32f, 0.18f)),
            entity = MakeMat(new Color(0.04f, 0.04f, 0.04f)),
            shadow = MakeMat(Color.black),
        };
    }

    // ---------------------------------------------------------------- Low-level primitives

    private static GameObject MakeWallSeg(Transform parent, string name, Vector3 center, Vector3 size, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = center;
        go.transform.localScale = size;
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    private static void WallX(Transform parent, string name, float z, float xMin, float xMax, float? gapMin, float? gapMax,
        List<GameObject> navStatics, Material mat, float baseY = 0f, float height = WallHeight)
    {
        if (gapMin.HasValue && gapMax.HasValue)
        {
            if (gapMin.Value - xMin > 0.05f)
                navStatics.Add(MakeWallSeg(parent, name + "_A", new Vector3((xMin + gapMin.Value) * 0.5f, baseY + height * 0.5f, z), new Vector3(gapMin.Value - xMin, height, WallThickness), mat));
            if (xMax - gapMax.Value > 0.05f)
                navStatics.Add(MakeWallSeg(parent, name + "_B", new Vector3((gapMax.Value + xMax) * 0.5f, baseY + height * 0.5f, z), new Vector3(xMax - gapMax.Value, height, WallThickness), mat));
        }
        else
        {
            navStatics.Add(MakeWallSeg(parent, name, new Vector3((xMin + xMax) * 0.5f, baseY + height * 0.5f, z), new Vector3(xMax - xMin, height, WallThickness), mat));
        }
    }

    private static void WallZ(Transform parent, string name, float x, float zMin, float zMax, float? gapMin, float? gapMax,
        List<GameObject> navStatics, Material mat, float baseY = 0f, float height = WallHeight)
    {
        if (gapMin.HasValue && gapMax.HasValue)
        {
            if (gapMin.Value - zMin > 0.05f)
                navStatics.Add(MakeWallSeg(parent, name + "_A", new Vector3(x, baseY + height * 0.5f, (zMin + gapMin.Value) * 0.5f), new Vector3(WallThickness, height, gapMin.Value - zMin), mat));
            if (zMax - gapMax.Value > 0.05f)
                navStatics.Add(MakeWallSeg(parent, name + "_B", new Vector3(x, baseY + height * 0.5f, (gapMax.Value + zMax) * 0.5f), new Vector3(WallThickness, height, zMax - gapMax.Value), mat));
        }
        else
        {
            navStatics.Add(MakeWallSeg(parent, name, new Vector3(x, baseY + height * 0.5f, (zMin + zMax) * 0.5f), new Vector3(WallThickness, height, zMax - zMin), mat));
        }
    }

    private static GameObject MakeFloor(Transform parent, string name, float xMin, float xMax, float zMin, float zMax, float y, List<GameObject> navStatics, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.layer = GroundLayer;
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3((xMin + xMax) * 0.5f, y - 0.1f, (zMin + zMax) * 0.5f);
        go.transform.localScale = new Vector3(xMax - xMin, 0.2f, zMax - zMin);
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
        navStatics.Add(go);
        return go;
    }

    private static void MakeCeiling(Transform parent, string name, float xMin, float xMax, float zMin, float zMax, float y, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3((xMin + xMax) * 0.5f, y + 0.1f, (zMin + zMax) * 0.5f);
        go.transform.localScale = new Vector3(xMax - xMin, 0.2f, zMax - zMin);
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static void MakeDescendingSteps(Transform parent, string prefix, float startZ, float startY, float rise, float depth, int count, float width, List<GameObject> navStatics, Material mat)
    {
        float finalY = startY - rise * count;
        for (int i = 0; i < count; i++)
        {
            float topY = startY - rise * (i + 1);
            float height = Mathf.Max(0.05f, topY - finalY);
            float z = startZ + depth * i + depth * 0.5f;

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"{prefix}_{i}";
            go.layer = GroundLayer;
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(0f, finalY + height * 0.5f, z);
            go.transform.localScale = new Vector3(width, height, depth);
            if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
            navStatics.Add(go);
        }
    }

    private static SimpleDoor CreateDoor(Transform parent, string name, Vector3 hingePos, float hingeYRotation, float width, float height, float thickness, Material mat)
    {
        var hinge = new GameObject(name);
        hinge.transform.SetParent(parent, false);
        hinge.transform.position = hingePos;
        hinge.transform.rotation = Quaternion.Euler(0f, hingeYRotation, 0f);

        var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panel.name = name + "_Panel";
        panel.transform.SetParent(hinge.transform, false);
        panel.transform.localPosition = new Vector3(width * 0.5f, height * 0.5f, 0f);
        panel.transform.localScale = new Vector3(width, height, thickness);
        if (mat != null) panel.GetComponent<Renderer>().sharedMaterial = mat;

        return hinge.AddComponent<SimpleDoor>();
    }

    // ---------------------------------------------------------------- Rooms

    private static BuildContext BuildRooms(Transform root, Materials mats, List<GameObject> navStatics)
    {
        var ctx = new BuildContext();

        // Floors
        MakeFloor(root, "Floor_Porch", -1.5f, 1.5f, -4f, -2f, 0f, navStatics, mats.floor);
        MakeFloor(root, "Floor_Entrada", -2f, 2f, -2f, 2f, 0f, navStatics, mats.floor);
        MakeFloor(root, "Floor_Living", -4f, 2f, 2f, 8f, 0f, navStatics, mats.floor);
        MakeFloor(root, "Floor_Cocina", 2f, 7f, 2f, 8f, 0f, navStatics, mats.floor);
        MakeFloor(root, "Floor_Pasillo", -1.5f, 1.5f, 8f, 12f, 0f, navStatics, mats.floor);
        MakeFloor(root, "Floor_Bano", -5f, -1.5f, 8.5f, 12f, 0f, navStatics, mats.floor);
        MakeFloor(root, "Floor_Habitacion", 1.5f, 7f, 8.5f, 14.5f, 0f, navStatics, mats.floor);
        MakeFloor(root, "Floor_Basement", -4f, 4f, 16.8f, 24.8f, -3f, navStatics, mats.floor);

        // Ceilings
        MakeCeiling(root, "Ceiling_Entrada", -2f, 2f, -2f, 2f, WallHeight, mats.ceiling);
        MakeCeiling(root, "Ceiling_Living", -4f, 2f, 2f, 8f, WallHeight, mats.ceiling);
        MakeCeiling(root, "Ceiling_Cocina", 2f, 7f, 2f, 8f, WallHeight, mats.ceiling);
        MakeCeiling(root, "Ceiling_Pasillo", -1.5f, 1.5f, 8f, 12f, WallHeight, mats.ceiling);
        MakeCeiling(root, "Ceiling_Bano", -5f, -1.5f, 8.5f, 12f, WallHeight, mats.ceiling);
        MakeCeiling(root, "Ceiling_Habitacion", 1.5f, 7f, 8.5f, 14.5f, WallHeight, mats.ceiling);
        MakeCeiling(root, "Ceiling_StairShaft", -0.95f, 0.95f, 12f, 16.8f, WallHeight, mats.ceiling);
        MakeCeiling(root, "Ceiling_Basement", -4f, 4f, 16.8f, 24.8f, -0.7f, mats.basementWall);

        // Walls
        WallX(root, "Entrada_South", -2f, -2f, 2f, -0.9f, 0.9f, navStatics, mats.wall);
        WallZ(root, "Entrada_West", -2f, -2f, 2f, null, null, navStatics, mats.wall);
        WallZ(root, "Entrada_East", 2f, -2f, 2f, null, null, navStatics, mats.wall);

        WallX(root, "Living_SouthExt", 2f, -4f, -2f, null, null, navStatics, mats.wall);
        WallX(root, "Entrada_Living_Boundary", 2f, -2f, 2f, -1f, 1f, navStatics, mats.wall);

        WallZ(root, "Living_West", -4f, 2f, 8f, null, null, navStatics, mats.wall);
        WallZ(root, "Living_Cocina_Boundary", 2f, 2f, 8f, 4f, 6f, navStatics, mats.wall);
        WallZ(root, "Cocina_East", 7f, 2f, 8f, null, null, navStatics, mats.wall);
        WallX(root, "Cocina_South", 2f, 2f, 7f, null, null, navStatics, mats.wall);
        WallX(root, "Cocina_North", 8f, 2f, 7f, null, null, navStatics, mats.wall);

        WallX(root, "Living_North_West", 8f, -4f, -1.5f, null, null, navStatics, mats.wall);
        WallX(root, "Living_Pasillo_Boundary", 8f, -1.5f, 1.5f, -1f, 1f, navStatics, mats.wall);
        WallX(root, "Living_North_East_Sliver", 8f, 1.5f, 2f, null, null, navStatics, mats.wall);

        WallZ(root, "Pasillo_Bano_Boundary", -1.5f, 8f, 12f, 9f, 10.5f, navStatics, mats.wall);
        WallZ(root, "Pasillo_Habitacion_Boundary", 1.5f, 8f, 12f, 9f, 10.5f, navStatics, mats.wall);
        WallX(root, "Pasillo_North", 12f, -1.5f, 1.5f, -0.9f, 0.9f, navStatics, mats.wall);

        WallZ(root, "Bano_West", -5f, 8.5f, 12f, null, null, navStatics, mats.wall);
        WallX(root, "Bano_South", 8.5f, -5f, -1.5f, null, null, navStatics, mats.wall);
        WallX(root, "Bano_North", 12f, -5f, -1.5f, null, null, navStatics, mats.wall);

        WallZ(root, "Habitacion_East", 7f, 8.5f, 14.5f, null, null, navStatics, mats.wall);
        WallX(root, "Habitacion_South", 8.5f, 1.5f, 7f, null, null, navStatics, mats.wall);
        WallX(root, "Habitacion_North", 14.5f, 1.5f, 7f, null, null, navStatics, mats.wall);

        // Basement stair shaft + room (0.95 half-width: slightly wider than the 1.8m door so the
        // panel swings freely, and wide enough that the default NavMesh bake erosion still leaves
        // a comfortably connected path down for the entity)
        WallZ(root, "StairShaft_West", -0.95f, 12f, 16.8f, null, null, navStatics, mats.basementWall, -3.2f, 6f);
        WallZ(root, "StairShaft_East", 0.95f, 12f, 16.8f, null, null, navStatics, mats.basementWall, -3.2f, 6f);
        MakeDescendingSteps(root, "Stair", 12f, 0f, 0.25f, 0.4f, 12, 1.9f, navStatics, mats.floor);

        WallZ(root, "Basement_West", -4f, 16.8f, 24.8f, null, null, navStatics, mats.basementWall, -3f, 2.3f);
        WallZ(root, "Basement_East", 4f, 16.8f, 24.8f, null, null, navStatics, mats.basementWall, -3f, 2.3f);
        WallX(root, "Basement_North", 24.8f, -4f, 4f, -1f, 1f, navStatics, mats.basementWall, -3f, 2.3f);

        var escapeGo = new GameObject("EscapeTrigger");
        escapeGo.transform.SetParent(root, false);
        escapeGo.transform.position = new Vector3(0f, -3f + 1.15f, 25.5f);
        var escapeCollider = escapeGo.AddComponent<BoxCollider>();
        escapeCollider.isTrigger = true;
        escapeCollider.size = new Vector3(2f, 2.3f, 1.5f);
        escapeGo.AddComponent<EscapeTrigger>();

        // Doors
        ctx.frontDoor = CreateDoor(root, "Door_Front", new Vector3(-0.9f, 0f, -2f), 0f, 1.8f, 2.1f, 0.08f, mats.door);
        ctx.basementDoor = CreateDoor(root, "Door_Basement", new Vector3(-0.9f, 0f, 12f), 0f, 1.8f, 2.1f, 0.08f, mats.door);
        // -90 (not 90): rotating the hinge this way sweeps the panel's local +X into world +Z,
        // which lands it exactly inside the z:9..10.5 doorway gap instead of back into the hallway.
        ctx.banoDoor = CreateDoor(root, "Door_Bano", new Vector3(-1.5f, 0f, 9f), -90f, 1.5f, 2.1f, 0.08f, mats.door);
        ctx.habitacionDoor = CreateDoor(root, "Door_Habitacion", new Vector3(1.5f, 0f, 9f), -90f, 1.5f, 2.1f, 0.08f, mats.door);

        Configure(ctx.basementDoor, ("startsLocked", true), ("lockedMessage", "Está trabada. No hay ninguna razón para bajar ahí todavía."));

        return ctx;
    }
}
#endif
