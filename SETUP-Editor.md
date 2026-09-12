# Setup en el Editor

No tengo forma de controlar la GUI de Unity directamente (no hay automatización de escritorio
disponible en esta sesión), así que en vez de darte una lista de clicks escribí una herramienta de
Editor que hace todo el armado de una — la corrés vos con un solo click, una vez que tengas el
proyecto abierto.

## Ya hecho (en el repo, no tocar a mano)

- `Packages/manifest.json` → `com.unity.inputsystem` agregado.
- `ProjectSettings/ProjectSettings.asset` → `Active Input Handling = Both`.
- `ProjectSettings/TagManager.asset` → layers **Player** (8), **Ground** (9), **Viewmodel** (10).
- `Assets/_Project/Input/PlayerControls.inputactions` → action map `Player` con Move, Look,
  Jump, Sprint, Crouch, ToggleCursor.
- `Assets/_Project/Scripts/**` → todos los componentes (Fases 1 a 6 del plan).
- `Assets/_Project/Settings/*.asset` → `MovementSettings`, `LookSettings`, `ViewmodelSettings`
  con los valores por defecto de la tabla del plan.
- `Assets/_Project/Editor/PlayerSceneSetup.cs` → la herramienta de bootstrap (ver abajo).

## Paso 0 — Dejar que el proyecto termine de importar

Con el editor ya abierto, esperá a que:
1. Resuelva el paquete `com.unity.inputsystem` (puede pedir reiniciar el editor — aceptar).
2. Compile todos los scripts nuevos sin errores (mirar la consola; no debería haber nada rojo).

## Paso 1 — Correr el bootstrap

1. Creá o abrí la escena donde querés trabajar (por ejemplo, duplicá `SampleScene` y renombrala
   `Playground`, o simplemente usá la que tenga abierta).
2. **Asegurate de que esa escena no tenga ya un GameObject llamado "Player"** (la herramienta
   aborta si lo encuentra, para no duplicar).
3. Menú **`Tools → TestingGame → Setup Player + Test Scene`**.

Esto arma en la escena activa, sin que tengas que tocar nada más:

- Un piso de prueba de 90×40 con rampas a 15°/30°/50°, dos tramos de escalones (0.2 m y 0.4 m),
  un pasillo angosto, una caja suelta, y un borde abierto al final (Z=25) para probar coyote time.
  Un "catch floor" mucho más abajo evita que te caigas al vacío infinito si te vas de un borde.
- La jerarquía completa del Player (`Player → CameraPivot → HeadBobPivot → ImpulsePivot →
  MainCamera / ViewmodelCamera → ViewmodelRoot → manos placeholder`) exactamente como en la
  sección 1.2 del plan, con el `Character Controller` configurado (altura 1.8, radio 0.35, step
  offset 0.35, slope limit 50).
- Todos los componentes (`PlayerInputReader`, `PlayerLook`, `PlayerMotor`, `PlayerCrouch`,
  `PlayerState`, `HeadBob`, `CameraImpulse`, `CameraFovKick`, `ViewmodelSway`,
  `ViewmodelAnimatorDriver`, `FootstepAudio`, `PlayerDebugOverlay`) agregados **y con todas sus
  referencias ya cableadas** (los tres `.asset` de Settings, el input actions, y las referencias
  cruzadas entre componentes).
- Las manos son dos cubos alargados en layer `Viewmodel` (placeholder — reemplazalos por el FBX
  real cuando lo consigas, ver sección 6.1 del plan; no hace falta tocar código, solo arrastrar el
  modelo nuevo bajo `ViewmodelRoot` y borrar los cubos).
- La matriz de colisión de física: la layer `Viewmodel` queda excluida de colisionar con todo lo
  demás (por código, vía `Physics.IgnoreLayerCollision`).
- Una Directional Light, si la escena no tenía ninguna.

Después de correrlo: **guardá la escena** (Ctrl+S) — la herramienta no guarda por vos a propósito,
para no forzar un diálogo de "guardar como" si la escena todavía no tiene archivo.

## Paso 2 — Probar

Play. `WASD` para moverse, mouse para mirar, Space para saltar, Shift para correr, Ctrl para
agacharte, Escape para soltar el cursor, F3 para el overlay de debug (velocidad, grounded,
estado). Contrastá contra los criterios de aceptación de cada fase en `PLAN-Movimiento3D.md`.

## Problemas típicos al primer Play

- **Consola llena de errores antes de tocar Play**: el proyecto no terminó de importar el paquete
  Input System. Cerrá y reabrí el proyecto.
- **"PlayerSceneSetup: no encontré uno o más assets esperados"**: mismo motivo, o corriste la
  herramienta antes de que terminara de compilar.
- **El player atraviesa el piso**: si agregaste geometría propia (no la que genera la
  herramienta), asegurate de que esté en layer **Ground**.
- **Las manos no se ven o se ven en el mundo entero**: si reemplazaste el FBX, verificá que el
  modelo (y sus hijos) haya quedado en layer **Viewmodel**.
- **Input no responde**: `PlayerInputReader` sin `Input Actions` asignado — no debería pasar si
  usaste la herramienta, revisar si se corrió sin errores en la consola.

## Paso 3 — Prefab (opcional, cuando esté a gusto)

Convertí `Player` en un Prefab (`Assets/_Project/Prefabs/Player.prefab`) para no tener que
rearmar la jerarquía a mano en escenas nuevas — para eso ya no hace falta la herramienta, alcanza
con arrastrar el GameObject a la carpeta de Prefabs.

## Si preferís armarlo a mano

Toda la jerarquía y el cableado de referencias que hace `PlayerSceneSetup.cs` está descrito en
detalle en la sección 1.2 y en el Paso 3/4 de versiones anteriores de este documento — podés leer
directamente el código de la herramienta (`Assets/_Project/Editor/PlayerSceneSetup.cs`) como
referencia exacta de qué campo va en qué componente si querés replicarlo manualmente o adaptarlo.
