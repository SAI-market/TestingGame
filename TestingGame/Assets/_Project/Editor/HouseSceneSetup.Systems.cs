#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static partial class HouseSceneSetup
{
    private static void BuildSystems(Transform systemsRoot, BuildContext ctx, EventRefs eventRefs, UIRefs uiRefs,
        PlayerRefs playerRefs, GameObject entityGo, ParanormalSettings paranormalSettings)
    {
        var objectiveManager = systemsRoot.gameObject.AddComponent<ObjectiveManager>();
        ConfigureObjectives(objectiveManager, new List<(string, string, int)>
        {
            ("kitchen", "Limpiar cocina", ctx.objectiveCounts.TryGetValue("kitchen", out int k) ? k : 1),
            ("living", "Limpiar living", ctx.objectiveCounts.TryGetValue("living", out int lv) ? lv : 1),
            ("bathroom", "Limpiar baño", ctx.objectiveCounts.TryGetValue("bathroom", out int b) ? b : 1),
            ("bedroom", "Limpiar habitación", ctx.objectiveCounts.TryGetValue("bedroom", out int bd) ? bd : 1),
        });

        var activityGo = new GameObject("ParanormalActivityManager");
        activityGo.transform.SetParent(systemsRoot, false);
        var activityManager = activityGo.AddComponent<ParanormalActivityManager>();
        Configure(activityManager, ("settings", paranormalSettings));

        var schedulerGo = new GameObject("ParanormalEventScheduler");
        schedulerGo.transform.SetParent(systemsRoot, false);
        var scheduler = schedulerGo.AddComponent<ParanormalEventScheduler>();
        Configure(scheduler, ("activityManager", activityManager));
        ConfigureObjectArray(scheduler, "events", eventRefs.all.Cast<UnityEngine.Object>().ToArray());

        var audioGo = new GameObject("AmbientAudio");
        audioGo.transform.SetParent(systemsRoot, false);
        var houseAmbience = audioGo.AddComponent<AudioSource>();
        houseAmbience.loop = true;
        houseAmbience.playOnAwake = false;
        houseAmbience.spatialBlend = 0f;
        houseAmbience.volume = 0.5f;
        var tensionDrone = audioGo.AddComponent<AudioSource>();
        tensionDrone.loop = true;
        tensionDrone.playOnAwake = false;
        tensionDrone.spatialBlend = 0f;
        tensionDrone.volume = 0f;
        var ambientController = audioGo.AddComponent<AmbientAudioController>();
        Configure(ambientController, ("activityManager", activityManager), ("houseAmbience", houseAmbience), ("tensionDrone", tensionDrone));

        var gameManagerGo = new GameObject("GameManager");
        gameManagerGo.transform.SetParent(systemsRoot, false);
        var gameManager = gameManagerGo.AddComponent<GameManager>();
        Configure(gameManager,
            ("frontDoor", ctx.frontDoor),
            ("basementDoor", ctx.basementDoor),
            ("entity", entityGo.GetComponent<EntityController>()),
            ("activityManager", activityManager),
            ("eventScheduler", scheduler),
            ("ambientAudio", ambientController),
            ("playerInput", playerRefs.input));
    }

    private static void WireUI(UIRefs uiRefs, PlayerRefs playerRefs, Transform systemsRoot)
    {
        var hud = uiRefs.canvas.gameObject.AddComponent<HUDController>();
        Configure(hud,
            ("interactor", playerRefs.interactor),
            ("toolController", playerRefs.toolController),
            ("crosshairImage", uiRefs.crosshair),
            ("promptText", uiRefs.promptText),
            ("toolLabelText", uiRefs.toolLabelText),
            ("batteryContainer", uiRefs.batteryContainer),
            ("batteryFillImage", uiRefs.batteryFill));

        var objectiveManager = systemsRoot.GetComponent<ObjectiveManager>();
        var objectivesUI = uiRefs.canvas.gameObject.AddComponent<ObjectivesUI>();
        Configure(objectivesUI, ("objectiveManager", objectiveManager), ("label", uiRefs.objectivesText));

        var messageUI = uiRefs.messageGroup.gameObject.AddComponent<MessageUI>();
        Configure(messageUI, ("group", uiRefs.messageGroup), ("label", uiRefs.messageText), ("fadeSpeed", 3f));

        // Lives on the always-active Canvas (not the panel itself) so Awake runs even though
        // the panel it controls starts deactivated.
        var endScreenUI = uiRefs.canvas.gameObject.AddComponent<EndScreenUI>();
        Configure(endScreenUI, ("panel", uiRefs.endScreenPanel), ("titleText", uiRefs.endTitle), ("subtitleText", uiRefs.endSubtitle));
    }
}
#endif
