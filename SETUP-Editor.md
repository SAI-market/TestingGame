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

---

# Limpieza Paranormal — vertical slice completo

A partir de acá el proyecto ya no es solo el controller de movimiento: es el juego completo
("Limpieza Paranormal") descripto en el diseño — casa jugable, limpieza, objetivos, sistema
paranormal semi-aleatorio, entidad con IA, sótano y final. Igual que con el player, no tengo forma
de controlar la GUI de Unity desde acá, así que todo está armado como código + una herramienta de
Editor que construye la escena entera de un solo click.

## Paso 0 — Dejar que el proyecto compile

Abrí el proyecto, esperá a que termine de compilar todos los scripts nuevos (`Assets/_Project/Scripts/**`
tiene ahora carpetas `Interaction`, `Cleaning`, `Tools`, `Objectives`, `Paranormal/Activity`,
`Paranormal/Events`, `Paranormal/Entity`, `Environment`, `Audio`, `UI`, `Core`) y mirá la consola: no
debería haber nada rojo.

## Paso 1 — Correr el bootstrap de la casa

Menú **`Tools → LimpiezaParanormal → Construir Casa (Vertical Slice)`**.

A diferencia de `PlayerSceneSetup`, esta herramienta **no** construye sobre la escena que tengas
abierta: crea (o reabre) una escena dedicada en `Assets/_Project/Scenes/LimpiezaParanormal.unity`,
la registra en Build Settings (así `SceneManager.LoadScene` puede reiniciar la partida) y arma ahí
adentro:

- La casa completa con primitivas: entrada, living, cocina, pasillo, baño, habitación y un sótano
  bajo escaleras, inicialmente bloqueado con `Door_Basement` (`SimpleDoor` con `startsLocked = true`).
- Muebles simples (sofá, mesada, cama, etc.) y ~10 manchas de suciedad (`CleanableSpot`) repartidas
  en 4 objetivos: cocina, living, baño, habitación.
- El player (reutilizando `PlayerSceneSetup.BuildPlayer`) parado en la entrada, extendido con
  `PlayerInteractor`, `ToolController` y las 4 herramientas (aspiradora/escoba/trapo/linterna) como
  geometría simple bajo el viewmodel.
- El sistema paranormal completo: `ParanormalActivityManager` (actividad interna 0-100, nunca
  mostrada), `ParanormalEventScheduler` con 8 anomalías registradas (puerta, luz, objeto que se
  mueve, dos sonidos ambiente, TV, sombra, falla de herramienta), y la `Entity` (cápsula placeholder
  con `NavMeshAgent` + `EntityController`, estados Idle/Observing/Manifesting/Chasing/Attacking).
- `GameManager` orquestando la progresión: limpieza → objetivos completos → puerta principal se
  traba → puerta del sótano se abre → el jugador baja → la entidad persigue de verdad → escape o
  muerte.
- La UI completa (canvas con crosshair, prompt de interacción, herramienta actual, batería de
  linterna, lista de objetivos, mensajes narrativos y pantalla de final) y el NavMesh ya horneado.

Después de correrla, la escena queda **guardada** (a diferencia de `PlayerSceneSetup`, esta sí
guarda sola, porque necesita un path fijo para registrarse en Build Settings).

## Paso 2 — Jugar

Play. Controles: `WASD` moverse, mouse mirar, `E` interactuar, click izquierdo usar la herramienta
equipada (mantené apretado sobre una mancha para limpiarla) o encender/apagar la linterna, `1`-`4`
cambiar de herramienta, `Shift` correr, `Ctrl` agachar, `F3` overlay de debug, `R` reiniciar desde
las pantallas de final.

Progresión esperada: limpiás los 4 objetivos → un mensaje avisa que terminaste → la puerta principal
no abre → se escucha algo en el sótano → la puerta del sótano se destraba → al entrar, la entidad
empieza a perseguir en serio → llegar al hueco al fondo del sótano (`EscapeTrigger`) es el final de
escape; que te agarre la entidad es el final de muerte.

## Limitación conocida: audio

El sistema de audio (ambiente, drone de tensión, pasos, puertas, sonidos paranormales, sonidos de la
entidad) está completamente armado — `AudioSource`s 3D, slots de `AudioClip` en cada evento,
crossfade por nivel de actividad — pero **no hay ningún clip de audio en el proyecto**, así que por
ahora todo eso es silencioso. Es la única pieza que queda 100% provisional a propósito: no hay forma
de generar o conseguir audio libre de copyright desde acá. Para completarlo alcanza con arrastrar
clips a los campos correspondientes en el Inspector (`Event_*` bajo `ParanormalEvents`, `AmbientAudio`,
`FootstepAudio` en el Player, `Entity`) — no hace falta tocar código.

## Si algo no compila o no funciona

- **Falta un asset esperado**: corré primero `Tools → TestingGame → Setup Player + Test Scene` al
  menos una vez (o esperá a que termine de importar) para que existan `MovementSettings.asset`,
  `LookSettings.asset`, `ViewmodelSettings.asset` y el `PlayerControls.inputactions`; el bootstrap de
  la casa los reutiliza y crea `ParanormalSettings.asset`/`EntitySettings.asset` solo si no existen.
- **"la escena ya tiene House_Root construido"**: borrá `Assets/_Project/Scenes/LimpiezaParanormal.unity`
  (o los objetos `House_Root`/`Systems`/`Player`/`HUD_Canvas` dentro de ella) si querés reconstruir
  desde cero.
- **La entidad no se mueve / warnings de NavMesh**: abrí `Window → AI → Navigation` y confirmá que
  hay datos horneados; si tocaste la geometría de la casa a mano después de correr el bootstrap,
  volvé a hornear desde ahí.
