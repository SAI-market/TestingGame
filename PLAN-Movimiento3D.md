# Plan — Sistema de Movimiento 3D en Primera Persona

> Documento de implementación para `TestingGame`. Está escrito para que otro modelo/dev lo ejecute
> fase por fase sin necesitar contexto adicional. Cada fase tiene entregables y criterios de
> aceptación verificables en el editor.

---

## 0. Contexto del proyecto (verificado)

| Dato | Valor |
|---|---|
| Ruta del proyecto Unity | `D:\Github\TestingGame\TestingGame` |
| Versión de Unity | `2022.3.23f1` (LTS) |
| Render Pipeline | **Built-in** (no hay URP/HDRP en el manifest) |
| Input System | **No instalado** — hay que agregarlo |
| Contenido de `Assets/` | Solo `Scenes/SampleScene.unity` |

## Decisiones tomadas

1. **Input:** New Input System (`com.unity.inputsystem`) con asset `.inputactions` y clase generada.
2. **Locomoción:** `CharacterController` (movimiento determinista, gravedad manual, sin física de Rigidbody).
3. **Perspectiva:** Primera persona con **viewmodel de manos** renderizado por una cámara secundaria.
4. **Alcance:** completo — base, sprint, crouch, head bob, sway, y pulido (coyote time, jump buffer, aceleración) + los extras propuestos en la sección 8.

---

## 1. Arquitectura

### 1.1 Principio rector

**Un script = una responsabilidad.** Nada de un `PlayerController.cs` de 800 líneas. El input se lee en
un solo lugar y se expone como datos; los sistemas consumen esos datos. Los valores ajustables viven
en un `ScriptableObject`, no hardcodeados ni dispersos en inspectores.

Flujo de datos por frame:

```
PlayerInputReader  ──(MoveInput, LookInput, JumpQueued, SprintHeld, CrouchHeld)──┐
                                                                                 │
   ┌─────────────────────────────────────────────────────────────────────────────┤
   ▼                          ▼                        ▼                         ▼
PlayerLook               PlayerMotor              PlayerCrouch            (consumidores)
(yaw en body,        (accel/friction,          (altura de cápsula,        HeadBob, ViewmodelSway,
 pitch en pivot)      gravedad, salto,          chequeo de techo)          FovKick, FootstepAudio
                      coyote/buffer)                    │                        ▲
                              │                         │                        │
                              └────────► PlayerState (Grounded/Airborne/Sprint/Crouch, Speed) ──┘
```

`PlayerMotor` es la **única** fuente de verdad de velocidad y de si está en el suelo. Todo lo demás lee
de ahí; nadie más llama a `characterController.Move()`.

### 1.2 Jerarquía de GameObjects

```
Player                              ← CharacterController, PlayerInputReader, PlayerMotor,
│                                     PlayerCrouch, PlayerState, FootstepAudio. Layer: Player
│                                     (el YAW se aplica rotando este transform)
└── CameraPivot                     ← altura de ojos (~1.6 m local Y). El PITCH se aplica acá.
    └── HeadBobPivot                ← HeadBob.cs escribe su localPosition acá.
        └── ImpulsePivot            ← CameraImpulse.cs escribe su localPosition/rotation acá.
            │                         Dos nodos separados y anidados (no uno solo) para que
            │                         bob e impulso se sumen en cascada sin pisarse el
            │                         localPosition mutuamente.
            ├── MainCamera          ← Camera (FOV ~70), CameraFovKick, AudioListener, tag MainCamera.
            │                         Culling Mask: TODO **excepto** Viewmodel.
            │                         Depth = 0. Clear Flags = Skybox.
            └── ViewmodelCamera     ← Camera (FOV ~55–60, propio).
                │                     Culling Mask: **solo** Viewmodel.
                │                     Depth = 1. Clear Flags = Depth only.
                │                     AudioListener DESACTIVADO.
                └── ViewmodelRoot   ← ViewmodelSway aplica offset de posición/rotación acá.
                    └── Hands_FBX   ← malla de manos + Animator + ViewmodelAnimatorDriver.
                                      Layer: Viewmodel (recursivo).
```

**Por qué dos cámaras:** con una sola, las manos atraviesan paredes al arrimarse y el FOV que se ve
bien para el mundo deforma las manos. La cámara de viewmodel dibuja encima con su propio FOV y su
propio clipping (`Near = 0.01`), y nunca se clipea contra la geometría del nivel.

### 1.3 Estructura de carpetas

```
Assets/_Project/
├── Input/            PlayerControls.inputactions
├── Scripts/
│   ├── Input/        PlayerInputReader.cs
│   ├── Player/       PlayerMotor.cs, PlayerLook.cs, PlayerCrouch.cs, PlayerState.cs
│   ├── Camera/       HeadBob.cs, CameraFovKick.cs, CameraImpulse.cs
│   ├── Viewmodel/    ViewmodelSway.cs, ViewmodelAnimatorDriver.cs
│   ├── Audio/        FootstepAudio.cs, SurfaceType.cs
│   └── Debug/        PlayerDebugOverlay.cs
├── Settings/         MovementSettings.asset, LookSettings.asset, ViewmodelSettings.asset
├── Art/Viewmodel/    (FBX de manos + materiales)
├── Audio/Footsteps/
└── Scenes/           Playground.unity
```

### 1.4 Setup de proyecto necesario

- **Layers a crear:** `Player`, `Viewmodel`, `Ground`.
- **Physics matrix:** `Viewmodel` sin colisión contra nada. `Player` sin colisión contra sí mismo.
- **Package Manager:** instalar `com.unity.inputsystem`. Al aceptar el reinicio, Unity setea
  `Active Input Handling = Input System Package (New)`. Si se quiere convivencia, usar `Both`.
- **Project Settings → Time:** dejar `Fixed Timestep` en 0.02; el motor corre en `Update`, no en `FixedUpdate`.

---

## 2. Fase 1 — Andamiaje e Input

**Entregables**

1. Instalar `com.unity.inputsystem`, crear layers y carpetas.
2. Escena `Playground.unity`: piso grande, rampas de 15° / 30° / 50°, escalones de 0.2 m y 0.4 m,
   un pasillo angosto, un borde para caer, y una caja para probar colisión lateral. Iluminación básica.
3. `PlayerControls.inputactions`, Action Map **Player**:

| Action | Tipo | Bindings |
|---|---|---|
| `Move` | Value / Vector2 | 2D Vector Composite WASD + left stick |
| `Look` | Value / Vector2 | `<Mouse>/delta` + right stick |
| `Jump` | Button | `<Keyboard>/space`, gamepad South |
| `Sprint` | Button (Hold) | `<Keyboard>/leftShift`, gamepad Left Stick Press |
| `Crouch` | Button | `<Keyboard>/leftCtrl`, gamepad East |
| `ToggleCursor` | Button | `<Keyboard>/escape` |

   Marcar **Generate C# Class** en el inspector del asset.

4. `PlayerInputReader.cs` — implementa la interfaz generada `IPlayerActions` y expone:
   `Vector2 Move`, `Vector2 Look`, `bool SprintHeld`, `bool CrouchHeld`, `bool CrouchPressed`,
   más `bool ConsumeJump()` (devuelve true una sola vez y limpia el buffer).
   Habilita/deshabilita el mapa en `OnEnable`/`OnDisable`. Bloquea el cursor al arrancar.

**Criterio de aceptación:** un `Debug.Log` muestra los valores de input correctos con teclado; el
proyecto compila sin errores; la escena se puede recorrer en modo Scene.

---

## 3. Fase 2 — Cámara y look

**Entregables**

- `LookSettings.asset` (ScriptableObject): `sensitivity` (X/Y separados), `invertY`,
  `minPitch = -85`, `maxPitch = 85`, `smoothing` (0 = raw, recomendado empezar en 0).
- `PlayerLook.cs`:
  - **Yaw** → `transform.Rotate(Vector3.up, lookX)` en el root `Player`. Esto hace que el movimiento
    sea automáticamente relativo a la cámara: `transform.forward` ya apunta a donde se mira.
  - **Pitch** → acumula en un float, `Mathf.Clamp(pitch, min, max)`, aplica
    `cameraPivot.localRotation = Quaternion.Euler(pitch, 0, 0)`. **Nunca** leer `localEulerAngles`
    para acumular: se rompe al cruzar 360°.
  - Corre en `Update`. El delta de mouse del New Input System **ya viene por frame** → *no*
    multiplicar por `Time.deltaTime` (sí hacerlo para el stick del gamepad).
  - `Escape` alterna `Cursor.lockState` entre `Locked` y `None` y congela el look.

**Criterio de aceptación:** mirar en 360° horizontal sin límite, vertical clampeado a ±85° sin
gimbal flip ni "rebote" al llegar al tope. Sin jitter a 30 y a 144 FPS.

---

## 4. Fase 3 — Locomoción base

`MovementSettings.asset` (todos los tunables en un solo lugar; valores iniciales sugeridos):

| Campo | Valor | Nota |
|---|---|---|
| `walkSpeed` | 4.5 | m/s |
| `sprintSpeed` | 7.5 | |
| `crouchSpeed` | 2.0 | |
| `groundAcceleration` | 60 | m/s² hasta velocidad objetivo |
| `groundFriction` | 50 | desaceleración al soltar input |
| `airAcceleration` | 12 | control aéreo limitado |
| `airFriction` | 2 | |
| `gravity` | -22 | más fuerte que -9.81: se siente mejor |
| `jumpHeight` | 1.15 | m — se convierte a velocidad con `√(2·h·|g|)` |
| `coyoteTime` | 0.12 | s |
| `jumpBuffer` | 0.15 | s |
| `maxSlopeAngle` | 50 | ° antes de deslizar |
| `slideSpeed` | 8 | velocidad de deslizamiento en pendiente empinada |
| `groundCheckDistance` | 0.15 | |
| `standHeight` / `crouchHeight` | 1.8 / 1.0 | |
| `crouchTransitionTime` | 0.15 | s |

`PlayerMotor.cs`, orden dentro de `Update()`:

1. **Ground check.** No confiar solo en `CharacterController.isGrounded` (es poco fiable en bordes y
   rampas). Usar `Physics.SphereCast` desde el centro de la cápsula hacia abajo contra la layer
   `Ground`, guardando también la **normal** del suelo. `isGrounded = hit && angle <= maxSlopeAngle`.
2. **Timers.** Actualizar `coyoteTimer` (se resetea a `coyoteTime` mientras está grounded, si no
   decrece) y `jumpBufferTimer` (se setea al presionar salto, decrece siempre).
3. **Velocidad horizontal.** Calcular `wishDir = (transform.right * move.x + transform.forward * move.y).normalized`.
   Elegir `targetSpeed` según estado (walk / sprint / crouch). Interpolar la velocidad horizontal
   actual hacia `wishDir * targetSpeed` con `MoveTowards` usando `acceleration` si hay input, o
   `friction` si no lo hay. Usar los valores de aire cuando `!isGrounded`.
4. **Gravedad.** Si está grounded y `velocityY < 0`, fijar `velocityY = -2` (mantiene el pegado al
   suelo sin acumular). Si no, `velocityY += gravity * dt`.
5. **Salto.** Si `jumpBufferTimer > 0 && coyoteTimer > 0`: `velocityY = Mathf.Sqrt(2 * jumpHeight * -gravity)`,
   y poner ambos timers en 0 (evita doble salto por buffer residual).
6. **Pendiente empinada.** Si hay suelo pero el ángulo supera `maxSlopeAngle`, proyectar la velocidad
   sobre el plano de la normal y sumar deslizamiento hacia abajo; bloquear el salto ahí.
7. **Mover una sola vez:** `controller.Move((horizontal + Vector3.up * velocityY) * dt)`.
8. **Anti-techo:** si `(controller.collisionFlags & CollisionFlags.Above) != 0 && velocityY > 0`,
   cortar `velocityY = 0` (evita quedarse pegado al techo).

Configuración del `CharacterController`: `Slope Limit = 50`, `Step Offset = 0.35`,
`Skin Width = 0.05` (≈10% del radio), `Radius = 0.35`, `Height = 1.8`, `Center = (0, 0.9, 0)`.

Exponer como propiedades públicas de solo lectura: `IsGrounded`, `Velocity`,
`HorizontalSpeed`, `IsSprinting`, y eventos `OnJumped`, `OnLanded(float impactSpeed)`.

**Criterio de aceptación:** camina y frena suave sin patinar; sube rampas de 30° y escalones de
0.2/0.4 sin trabarse; resbala en la rampa de 50°; salta ~1.15 m; el salto funciona presionándolo
0.1 s *antes* de tocar el suelo (buffer) y 0.1 s *después* de dejar el borde (coyote); no se pega a
paredes ni tiembla en esquinas.

---

## 5. Fase 4 — Sprint y crouch

- **Sprint:** solo válido con input hacia adelante (`move.y > 0.1`) y sin estar agachado. Cancela al
  soltar la tecla. Dispara `CameraFovKick`.
- **Crouch** (`PlayerCrouch.cs`):
  - Lerp de `controller.height` y `controller.center` entre `standHeight` y `crouchHeight` en
    `crouchTransitionTime`; mover `CameraPivot.localPosition.y` en el mismo lerp (si no, la cámara
    "salta").
  - **Chequeo de techo antes de pararse:** `Physics.CheckCapsule` / `SphereCast` hacia arriba. Si hay
    obstáculo, seguir agachado aunque se suelte la tecla.
  - Configurable como *hold* o *toggle* mediante un bool en settings.

**Criterio de aceptación:** agacharse y pararse es continuo (sin pop de cámara); bajo un techo de
1.2 m no se puede parar; el sprint no se activa caminando en reversa.

---

## 6. Fase 5 — Manos (viewmodel)

### 6.1 De dónde sacar el modelo

Todas gratuitas y de uso comercial permitido — verificar la licencia individual antes de commitear:

| Fuente | Qué sirve |
|---|---|
| **Mixamo** (mixamo.com) | Cuerpo humanoide completo con animaciones; se puede usar solo la parte superior. Requiere cuenta Adobe gratuita. |
| **Quaternius** (quaternius.com) | Packs CC0, estilo low-poly, incluye personajes riggeados. |
| **Kenney** (kenney.nl) | CC0, muy limpio, ideal para prototipo. |
| **Poly Pizza / Sketchfab** | Filtrar por CC0 o CC-BY; buscar "FPS hands" / "first person arms". |
| **Unity Asset Store** | Filtrar por gratis; hay varios packs de FPS hands riggeadas. |

Para *prototipar sin descargar nada*: dos cubos alargados como stand-in de brazos ubicados en
`ViewmodelRoot`. Toda la Fase 5 se puede validar con eso y reemplazar el modelo después sin tocar
código.

### 6.2 Import y configuración

1. FBX a `Assets/_Project/Art/Viewmodel/`.
2. Inspector → **Rig**: `Humanoid` si viene de Mixamo, `Generic` si no. **Materials**: `Extract` y
   reasignar shader **Standard** (el built-in RP muestra magenta con materiales de URP).
3. Asignar la layer `Viewmodel` al objeto **y a todos sus hijos** (Unity pregunta al cambiar la layer
   del padre — responder "Yes, change children").
4. Escala: ajustar en `ViewmodelRoot`, nunca en el FBX importado, para no romper las animaciones.
5. Animaciones mínimas: `Idle`, `Walk`, `Sprint`, `Jump`, `Land`. Marcar `Loop Time` en las cíclicas.
   Animator Controller con un blend simple manejado por `ViewmodelAnimatorDriver.cs`, que lee
   `PlayerMotor.HorizontalSpeed` normalizada y `IsGrounded` y los escribe como parámetros.

### 6.3 `ViewmodelSway.cs`

En `LateUpdate`, sobre `ViewmodelRoot` (local space):

- **Sway de mirada:** offset de posición y rotación opuesto al delta del mouse, con `SmoothDamp` de
  vuelta a cero. Clampear el offset máximo (~2–4 cm y ~4°) para que no se vaya de cuadro.
- **Sway de movimiento:** leve inclinación lateral según `move.x` (efecto "strafe lean").
- **Bob de manos:** misma onda seno que el head bob pero con amplitud menor y ligero desfase.
- **Impulsos:** hundir las manos al aterrizar y al saltar, reutilizando `CameraImpulse`.

### 6.4 `HeadBob.cs`

En `LateUpdate`, sobre `CameraShake.localPosition`:

- Onda: `y = sin(t * freq) * amp`, `x = cos(t * freq * 0.5) * amp * 0.5`. El contador `t` avanza
  proporcional a la velocidad horizontal → parado no hay bob, corriendo es más rápido y amplio.
- Amplitud/frecuencia distintas por estado (walk / sprint / crouch), interpoladas, no conmutadas de golpe.
- Al llegar a la parte baja de la onda, disparar el evento de paso que consume `FootstepAudio`.
- Volver a cero con `Lerp` al detenerse, para que no quede cortado a mitad de onda.

**Criterio de aceptación:** las manos se ven en pantalla, nunca atraviesan paredes ni se clipean al
pegarse a una caja, siguen la mirada con retardo natural, y el head bob no marea a velocidad de
caminata (empezar con amplitudes chicas: 0.04 m).

---

## 7. Fase 6 — Pulido y feedback

- `CameraFovKick.cs`: FOV +8 al esprintar, `SmoothDamp` de ~0.25 s, sobre **ambas** cámaras solo si se
  quiere que las manos también reaccionen (recomendado: solo la principal).
- `CameraImpulse.cs`: API `AddImpulse(Vector3 pos, Vector3 rot)` con retorno por resorte amortiguado.
  Usado por el aterrizaje (`OnLanded`, escalando con `impactSpeed`) y el salto.
- `FootstepAudio.cs`: pool de `AudioClip` por `SurfaceType`, detectado con el `PhysicMaterial` o un
  componente `SurfaceTag` en el collider bajo los pies. Variación aleatoria de pitch (0.9–1.1) y
  volumen. Clip distinto para aterrizaje.
- `PlayerDebugOverlay.cs`: OnGUI con velocidad horizontal, `isGrounded`, estado, ángulo de suelo,
  timers de coyote/buffer. Activable con F3. Más `OnDrawGizmosSelected` dibujando la esfera del ground
  check y la normal del suelo.

---

## 8. Extras propuestos (más allá de lo pedido)

Ordenados por relación valor/esfuerzo. No son obligatorios para cerrar el sistema, pero es lo que
separa un controller "que funciona" de uno que se siente bien.

1. **Soporte de plataformas móviles.** Si el `SphereCast` de suelo golpea un objeto con
   `MovingPlatform`, sumar el delta de posición de la plataforma al `Move()` del frame (y su delta de
   yaw a la rotación del player). Sin esto, el jugador se resbala de cualquier ascensor.
2. **Presets de "feel" como assets.** Al ser `MovementSettings` un ScriptableObject, tener
   `Movement_Realistic.asset`, `Movement_Arcade.asset`, `Movement_Floaty.asset` y poder cambiar el
   asset en Play Mode para comparar. Es la forma más rápida de afinar el tacto.
3. **Gravedad asimétrica.** Multiplicador de gravedad mayor durante la caída (~1.4×) y salto de altura
   variable (si se suelta la tecla mientras sube, cortar `velocityY *= 0.5`). Muy pocos renglones,
   cambia mucho la sensación del salto.
4. **Step-up asistido.** El `Step Offset` del `CharacterController` es rígido; un raycast frontal que
   detecte escalones bajos y aplique un ajuste vertical suave elimina el "tirón" al subir.
5. **Raycast de interacción.** Un `PlayerInteractor` que lance un ray desde la cámara (~3 m) contra
   una layer `Interactable` con una interfaz `IInteractable`, más prompt en pantalla. Es la base
   natural del próximo sistema y cuesta poco dejarlo enganchado ahora.
6. **Escala de altura y sensibilidad en un menú de opciones.** Sensibilidad, invert Y, toggle vs hold
   de crouch, intensidad de head bob (incluido **0 = desactivado**, que es un requisito real de
   accesibilidad: el bob provoca mareo a mucha gente).
7. **Rebinding en runtime.** El New Input System lo trae de fábrica (`PerformInteractiveRebinding`);
   es casi gratis habiéndolo elegido como base.
8. **Lean lateral (Q/E).** Rotación en Z del `CameraPivot` con un `SphereCast` que impida atravesar
   paredes. Encaja bien con el viewmodel.
9. **Modo noclip/espectador para debug.** Vuelo libre con el mismo input, desactivando el
   `CharacterController`. Ahorra muchísimo tiempo al probar niveles.
10. **Tests de PlayMode** para lo que es matemática pura y regresiona fácil: altura de salto real vs
    configurada, que el coyote time no permita doble salto, y que el chequeo de techo bloquee el
    pararse.

---

## 9. Orden de ejecución y checklist

| # | Fase | Entregable verificable | Estado |
|---|---|---|---|
| 1 | Andamiaje + Input | Input logueado, escena Playground | ☐ |
| 2 | Cámara + Look | Mouse look clampeado, sin jitter | ☐ |
| 3 | Locomoción base | Caminar, gravedad, salto, coyote, buffer, rampas | ☐ |
| 4 | Sprint + Crouch | Transiciones suaves, chequeo de techo | ☐ |
| 5 | Viewmodel | Manos visibles, sin clipping, sway + bob | ☐ |
| 6 | Pulido | FOV kick, impulsos, pasos, overlay de debug | ☐ |
| 7 | Extras (sección 8) | Según prioridad | ☐ |

**Regla de trabajo:** no avanzar de fase sin cumplir el criterio de aceptación de la anterior. Commit
al cerrar cada fase, con la escena `Playground` en un estado jugable.

---

## 10. Errores conocidos a evitar

- Multiplicar el delta del mouse por `Time.deltaTime` → la sensibilidad varía con los FPS.
- Acumular pitch leyendo `localEulerAngles` → salto al cruzar 0/360.
- Llamar a `controller.Move()` más de una vez por frame desde distintos scripts → colisiones erráticas.
- Confiar solo en `CharacterController.isGrounded` → falsos negativos en bordes y rampas.
- Head bob y look rotation en el mismo transform → se pisan entre sí.
- Cambiar la layer del padre del FBX sin propagar a los hijos → las manos las dibuja la cámara
  principal y se clipean contra las paredes.
- Bob/sway en `Update` en vez de `LateUpdate` → un frame de retraso visible respecto de la cámara.
- Olvidar desactivar el `AudioListener` de la cámara de viewmodel → warning y audio duplicado.
- No normalizar `wishDir` → moverse en diagonal es ~1.41× más rápido.
