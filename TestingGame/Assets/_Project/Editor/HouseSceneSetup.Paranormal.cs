#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public static partial class HouseSceneSetup
{
    public class EventRefs
    {
        public List<ParanormalEventBase> all = new List<ParanormalEventBase>();
    }

    private static EventRefs BuildParanormalEvents(Transform root, BuildContext ctx)
    {
        var refs = new EventRefs();
        var eventsRoot = new GameObject("ParanormalEvents").transform;
        eventsRoot.SetParent(root, false);

        var doorSlamGo = new GameObject("Event_DoorSlam");
        doorSlamGo.transform.SetParent(eventsRoot, false);
        doorSlamGo.transform.position = ctx.banoDoor.transform.position;
        var doorSlam = doorSlamGo.AddComponent<DoorSlamEvent>();
        Configure(doorSlam, ("doorPivot", ctx.banoDoor.transform), ("minLevel", 1), ("cooldownSeconds", 90f), ("weight", 1f), ("activityBump", 3f));
        refs.all.Add(doorSlam);

        var lightFlickerGo = new GameObject("Event_LightFlicker");
        lightFlickerGo.transform.SetParent(eventsRoot, false);
        lightFlickerGo.transform.position = ctx.pasilloLight.transform.position;
        var lightFlicker = lightFlickerGo.AddComponent<LightFlickerEvent>();
        ConfigureObjectArray(lightFlicker, "lights", new UnityEngine.Object[] { ctx.pasilloLight });
        Configure(lightFlicker, ("minLevel", 1), ("cooldownSeconds", 75f), ("weight", 1.2f), ("activityBump", 3f), ("flickerDuration", 2.5f), ("endOff", false));
        refs.all.Add(lightFlicker);

        var objMoveGo = new GameObject("Event_ObjectMove");
        objMoveGo.transform.SetParent(eventsRoot, false);
        objMoveGo.transform.position = ctx.livingPicture.position;
        var objMove = objMoveGo.AddComponent<ObjectMoveEvent>();
        Configure(objMove, ("prop", ctx.livingPicture), ("minLevel", 1), ("cooldownSeconds", 85f), ("weight", 1f), ("activityBump", 3f));
        refs.all.Add(objMove);

        var sound1Go = new GameObject("Event_Sound_Hallway");
        sound1Go.transform.SetParent(eventsRoot, false);
        sound1Go.transform.position = new Vector3(0f, 2.3f, 10f);
        var sound1 = sound1Go.AddComponent<AmbientSoundEvent>();
        Configure(sound1, ("minLevel", 1), ("cooldownSeconds", 45f), ("weight", 2f), ("activityBump", 2f));
        refs.all.Add(sound1);

        var sound2Go = new GameObject("Event_Sound_Bedroom");
        sound2Go.transform.SetParent(eventsRoot, false);
        sound2Go.transform.position = new Vector3(4f, 2.3f, 12f);
        var sound2 = sound2Go.AddComponent<AmbientSoundEvent>();
        Configure(sound2, ("minLevel", 2), ("cooldownSeconds", 50f), ("weight", 1.5f), ("activityBump", 3f));
        refs.all.Add(sound2);

        var tvGo = new GameObject("Event_TVStatic");
        tvGo.transform.SetParent(eventsRoot, false);
        tvGo.transform.position = ctx.livingTvScreenOn.transform.position;
        var tv = tvGo.AddComponent<TVStaticEvent>();
        Configure(tv, ("screenOnVisual", ctx.livingTvScreenOn), ("minLevel", 1), ("cooldownSeconds", 100f), ("weight", 0.8f), ("activityBump", 4f), ("onDuration", 4f));
        refs.all.Add(tv);

        var shadowVisual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        shadowVisual.name = "ShadowFigureVisual";
        shadowVisual.transform.SetParent(eventsRoot, false);
        shadowVisual.transform.localScale = new Vector3(0.5f, 0.9f, 0.5f);
        Object.DestroyImmediate(shadowVisual.GetComponent<Collider>());
        shadowVisual.GetComponent<Renderer>().sharedMaterial = MakeMat(Color.black);
        shadowVisual.SetActive(false);

        var shadowStart = new GameObject("ShadowStart").transform;
        shadowStart.SetParent(eventsRoot, false);
        shadowStart.position = ctx.hallwayWestPoint;
        var shadowEnd = new GameObject("ShadowEnd").transform;
        shadowEnd.SetParent(eventsRoot, false);
        shadowEnd.position = ctx.hallwayEastPoint;

        var shadowEventGo = new GameObject("Event_ShadowFigure");
        shadowEventGo.transform.SetParent(eventsRoot, false);
        var shadowEvent = shadowEventGo.AddComponent<ShadowFigureEvent>();
        Configure(shadowEvent, ("shadowVisual", shadowVisual), ("startPoint", shadowStart), ("endPoint", shadowEnd),
            ("minLevel", 2), ("cooldownSeconds", 70f), ("weight", 1f), ("activityBump", 4f), ("crossDuration", 0.8f));
        refs.all.Add(shadowEvent);

        var malfunctionGo = new GameObject("Event_ToolMalfunction");
        malfunctionGo.transform.SetParent(eventsRoot, false);
        var malfunction = malfunctionGo.AddComponent<ToolMalfunctionEvent>();
        Configure(malfunction, ("minLevel", 3), ("cooldownSeconds", 110f), ("weight", 1f), ("activityBump", 5f), ("malfunctionDuration", 3.5f));
        refs.all.Add(malfunction);

        return refs;
    }

    private static Transform[] BuildHidingSpots(Transform root, BuildContext ctx)
    {
        var hidingRoot = new GameObject("HidingSpots").transform;
        hidingRoot.SetParent(root, false);

        Vector3[] points =
        {
            new Vector3(6.5f, 0f, 2.5f),
            new Vector3(-4.7f, 0f, 8.8f),
            new Vector3(6.5f, 0f, 14f),
            new Vector3(-3.7f, 0f, 7.7f),
            new Vector3(-3.5f, -3f, 17.2f),
            new Vector3(3.5f, -3f, 24f),
        };

        var result = new Transform[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            var t = new GameObject($"HidingSpot_{i}").transform;
            t.SetParent(hidingRoot, false);
            t.position = points[i];
            result[i] = t;
        }
        return result;
    }

    private static GameObject BuildEntity(Transform root, EntitySettings settings, Transform player, Transform[] hidingSpots)
    {
        var entityGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        entityGo.name = "Entity";
        entityGo.transform.SetParent(root, false);
        entityGo.transform.position = new Vector3(0f, -2f, 20f);
        entityGo.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        entityGo.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.04f, 0.04f, 0.04f));

        var agent = entityGo.AddComponent<NavMeshAgent>();
        agent.radius = 0.35f;
        agent.height = 2f;
        agent.speed = settings.observeSpeed;
        agent.acceleration = 14f;
        agent.stoppingDistance = 0.1f;

        var stinger = entityGo.AddComponent<AudioSource>();
        stinger.playOnAwake = false;
        stinger.spatialBlend = 1f;

        var moveLoop = entityGo.AddComponent<AudioSource>();
        moveLoop.playOnAwake = false;
        moveLoop.loop = true;
        moveLoop.spatialBlend = 1f;
        moveLoop.volume = 0.6f;

        var controller = entityGo.AddComponent<EntityController>();
        Configure(controller, ("settings", settings), ("player", player), ("audioSource", stinger), ("moveLoopSource", moveLoop));
        ConfigureObjectArray(controller, "hidingSpots", hidingSpots.Cast<UnityEngine.Object>().ToArray());
        ConfigureObjectArray(controller, "visualRenderers", new UnityEngine.Object[] { entityGo.GetComponent<Renderer>() });

        return entityGo;
    }
}
#endif
