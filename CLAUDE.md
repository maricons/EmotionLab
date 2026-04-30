# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Project Overview

EmotionLab is a Unity VR experience designed as an emotional regulation research tool. It guides participants through task scenarios (office stress, breathing exercises) with a dialog-driven robot guide, questionnaires, and timed events.

- **Unity version:** Requires Unity with URP v14.0.12
- **Render Pipeline:** Universal Render Pipeline (URP)
- **VR SDK:** OpenXR 1.14.3 + XR Interaction Toolkit 2.6.5
- **Scripting:** C# (no custom namespaces — all scripts in global namespace)
- **Language:** Script names and comments are in Spanish
- **Team:** Constanza Quiero & Katalina Pérez — LIDETI

---

## Development Commands

This is a Unity project with no CLI build tooling. All building, running, and testing is done through the Unity Editor.

- **Open project:** Open Unity Hub and load `D:\EMOTIONLAB\EmotionLab`
- **Play:** Press Play in the Unity Editor (starts from the active scene)
- **Build:** File → Build Settings → Build

---

## Scene Flow

```
Waiting Room  →  Oficina (Office) = "Fácil"  OR  Salón (Hall) = "Difícil"  →  Cierre (Closure)
```

- **Waiting Room:** Participant fills out a pre-task questionnaire (clipboard UI) y elige nivel de dificultad en la pizarra ("Fácil" o "Difícil").
- **Oficina:** Nivel **Fácil**. Timed office task with a generator failure interruption event at t=110s.
- **Salón:** Nivel **Difícil**. Guided breathing exercise (3 inhale/hold/exhale cycles with animated balloon).
- **Cierre:** Wrap-up scene with post-experience questionnaires and KPI data flush.

> ⚠️ **Importante:** la pizarra del Waiting Room muestra "Fácil" / "Difícil" pero los nombres internos de las escenas siguen siendo `Oficina` y `Salón`. El mapeo es:
> - Botón "Fácil" → carga escena `Oficina`
> - Botón "Difícil" → carga escena `Salón`
>
> Esto debe documentarse al participante y registrarse como **metadato de sesión** (`difficulty_level: "facil" | "dificil"`) por `TelemetryService` / `SessionStore`, junto con `session_id`. Esencial para análisis posterior — sin esto no se puede correlacionar respuestas con dificultad.

Scene and presentation selection is stored in `PlayerPrefs` by `MenuSelector`:
- `"EscenaSeleccionada"` — target scene name (`"Oficina"` o `"Salón"`)
- `"PresentacionSeleccionada"` — presentation folder name
- (pendiente) `"DificultadSeleccionada"` — `"facil"` | `"dificil"` para que `TelemetryService` lo capture sin re-derivar del nombre de escena.

---

## Code Architecture

All gameplay scripts live in `Assets/Resources/My scripts/`. Key systems:

### Dialog System
Sequential panel-based dialogs. Each dialog script (`DialogoBienvenida`, `DialogoPreparacion`, `DialogoTips`) holds an array of GameObjects (panels). Navigation: `OnNextButton()`, `OnSkipButton()`, `GoToPanel(index)`.

> ⚠️ **Refactor pendiente (Sprint 1):** Las tres clases de diálogo son casi idénticas. Deben unificarse en una sola clase `DialogoBase` con configuración por Inspector, antes de agregar cualquier funcionalidad nueva.

### Timer (`Temporizador`)
Central timer for task scenes. Other scripts poll it via `getTemporizadorActivo()` and `getTiempoRestante()`. Supports MM:SS, MM:SS.ms, and HH:MM:SS display formats.

### Event Triggers (`Interrupciones`)
Monitors `Temporizador` and fires the generator failure event at the 110-second threshold. Controls audio ducking (background music fades, alert SFX plays) and particle effects via boolean flags to prevent re-triggering.

> ⚠️ El umbral de 110s y los volúmenes 0.8f / 0.1f están hardcodeados. Deben moverse a `[SerializeField]`.

### Breathing Exercise (`EjercicioRespiracion`)
Coroutine-based cycle: inhale → hold → exhale. Scales a balloon GameObject to visualize breathing. Integrates with the dialog system to sequence instructions.

> ⚠️ Las duraciones 4f, 7f, 8f están hardcodeadas y deben ser `[SerializeField]`. Las llamadas a `StartCoroutine(MostrarCuentaAtras(...))` sin `yield return` (líneas ~77, 81, 85) generan cuentas regresivas en paralelo — bug crítico a corregir en Sprint 1.

### Music Persistence (`DontDestroyMusic`)
Singleton via `DontDestroyOnLoad` + tag check (`"MusicPlayer"`). Prevents duplicate audio players across scene loads.

### Spawners
`Person1Spawner` y `VehiculeSpawner` usan `InvokeRepeating` para instanciar prefabs aleatorios desde arrays asignados por Inspector. `DestruirAlContacto` limpia objetos en los límites de zona.

> ⚠️ El nombre "Vehicule" es incorrecto en código — debería ser "Vehicle". Corregir en Sprint 1 junto con la estandarización de nombres.

### VR Interaction
- VR controller tag: `"ControladorVR"`
- `ArreglarGenerador` uses trigger collider detection for the generator repair mechanic
- `RobotMover` optionally looks at a `lookTarget` transform (the VR player)

### Telemetry & KPI (nuevo — Sprint 1–4)
Sistema de instrumentación para captura automática de KPIs de investigación. Ver sección **Medición de KPIs** más abajo.

---

## Key Tags
`"ControladorVR"`, `"Person"`, `"Vehicule"`, `"MusicPlayer"`

---

## Presentation Slides
Slides are loaded at runtime from `Resources/imagenes/{nombrePresentacion}/` as `Sprite` arrays by `CargarPresentacion`.

---

## Nivel Difícil (Sprint 3)
El Nivel Difícil es una nueva variante de la escena de Oficina con mayor carga de estímulos: más avatares en movimiento, ruido ambiental intenso, distractores visuales superpuestos y alertas más frecuentes. El objetivo es aumentar la dificultad de regulación emocional para evaluar la respuesta del participante bajo mayor presión.

---

## Medición de KPIs

El sistema de telemetría captura automáticamente los KPIs del estudio sin backend. Todos los datos se almacenan localmente en `Application.persistentDataPath`.

### Arquitectura de componentes

| Script | Responsabilidad |
|---|---|
| `ConsentManager` | Primera pantalla de la app. Habilita o deshabilita el registro según decisión del participante. |
| `TelemetryService` | Singleton. Punto único de entrada: `LogEvent(name, phase, payload)`. |
| `SessionStore` | Mantiene en memoria la sesión activa y la serializa al cierre. |
| `LocalStorageWriter` | Escribe JSON maestro por sesión y CSV resumen agregado. |
| `FacilitatorPanel` | Escena separada para que el facilitador ingrese KPIs observacionales en paralelo. |

### Eventos instrumentados

| Evento | KPI | Momento |
|---|---|---|
| `session_started` / `session_completed` | Tasa de completitud (KPI 1.4) | Inicio y cierre de sesión |
| `emotion_check_in` | Identificación y variación emocional (KPI 2.1, 2.2) | Pre / durante / post exposición |
| `breathing_technique_used` | Uso de técnicas de afrontamiento (KPI 2.3) | Al iniciar y completar EjercicioRespiracion |
| `hr_sample` | Variación de FC simulada (KPI 2.4) | Muestreo cada 1s desde SimulacionBPM |
| `retry_clicked` | Repetición voluntaria (KPI 4.2) | Click en botón "Volver a intentar" |
| `distractor_interference` | Interferencia de distractores (KPI 3.3) | Post-exposición + conteo automático |
| `ueq_response` | Satisfacción del usuario UEQ-S (KPI 1.1) | Formulario al finalizar sesión |
| `future_use_intent` | Intención de uso futuro (KPI 4.1) | Likert 1–5 al finalizar |
| `learning_value_rating` | Valoración del aprendizaje emocional (KPI 4.3) | Likert 1–5 al finalizar |
| `facilitator_error_log` | Tasa de errores de uso (KPI 1.3) | Panel del facilitador, +1 por incidente |
| `facilitator_rubric_speech` | Claridad y fluidez del discurso (KPI 3.2) | Rúbrica del facilitador |

### Metadatos de sesión (session-level)

Además de los eventos puntuales, cada sesión guarda metadatos estables que se asignan al inicio y aplican a toda la captura:

| Metadato | Valores | Origen | Propósito |
|---|---|---|---|
| `session_id` | UUID v4 | Generado al inicio | Identificador único anonimizado |
| `device_id_hash` | SHA-256 hash | `SystemInfo.deviceUniqueIdentifier` | Trazabilidad por dispositivo |
| `difficulty_level` | `"facil"` \| `"dificil"` | Botón pizarra Waiting Room (Fácil/Difícil) | Correlacionar respuestas y KPIs con nivel |
| `presentation_id` | string | Selección en Waiting Room | Qué presentación se usó |
| `started_at` / `ended_at` | ISO timestamp | Inicio/fin de sesión | Duración total |

> Sin `difficulty_level` registrado, no se puede analizar si las emociones / KPIs varían entre niveles. Es **obligatorio** capturarlo al cargar la escena de Oficina o Salón.

### Anonimización
- No se almacenan nombres ni correos.
- Exportar archivos al final de cada jornada y eliminarlos del dispositivo.

### Archivos generados
```
Application.persistentDataPath/
├── sessions/
│   └── 2026-MM-DD_{uuid}.json   ← detalle completo por sesión
├── summary.csv                  ← una fila por sesión, KPIs agregados
└── consent_log.csv              ← log de aceptaciones de consentimiento
```

---

## Bugs críticos a resolver ANTES de cualquier funcionalidad nueva

> Estos issues pueden causar crashes o comportamiento incorrecto en producción. Resolverlos es prioridad en **Sprint 1**.

1. **Acceso a arrays sin verificar límites** — `DialogoBienvenida`, `DialogoTips`, `DialogoPreparacion` y `Temporizador` acceden a `textPanels[currentPanelIndex]` sin validar el índice. Si el array tiene menos paneles de lo esperado, el juego crashea.

2. **Corrutinas en paralelo no intencionadas** — `EjercicioRespiracion.cs` (líneas ~77, 81, 85): `StartCoroutine(MostrarCuentaAtras(...))` sin `yield return` lanza múltiples cuentas regresivas simultáneas que se pisan entre sí.

3. **Referencias sin null check** — `DialogoBienvenida` (`robotMover`), `Interrupciones` (`temporizador`), `SimulacionBPM` (`displayText`), `Temporizador` (`robot`). Un GameObject no asignado en el Inspector lanza NullReferenceException en runtime.

---

## Refactors pendientes por sprint

### Sprint 1 — Estabilidad base
- [x] Unificar `DialogoBienvenida`, `DialogoPreparacion` y `DialogoTips` en una sola clase `DialogoBase` configurable por Inspector
- [x] Corregir acceso a arrays sin verificar límites (todos los scripts de diálogo y Temporizador)
- [x] Corregir corrutinas en paralelo en `EjercicioRespiracion.cs`
- [x] Agregar null checks en `DialogoBienvenida.robotMover`, `Interrupciones.temporizador`, `SimulacionBPM.displayText`, `Temporizador.robot`
- [ ] Implementar pantalla de consentimiento informado (`ConsentManager`)
- [ ] Implementar `TelemetryService` singleton + `SessionStore`
- [ ] Integrar eventos KPI en formularios de cierre (`ueq_response`, `future_use_intent`, `learning_value_rating`)
- [x] Guardar emociones iniciales en JSON (`emotion_check_in` pre)
- [ ] Implementar preguntas de emoción en cierre y estructura JSON de respuestas

### Sprint 2 — Sistema de niveles y mejoras
- [ ] Extraer todos los números mágicos a variables `[SerializeField]` (umbrales, volúmenes, duraciones, rangos BPM)
- [ ] Desacoplar scripts con eventos de C# (`UnityEvent` o `Action`) — eliminar dependencias directas entre `Temporizador` → UI/robot y `ArreglarGenerador` → `Interrupciones`
- [ ] Registrar `session_started` / `session_completed` (tasa de completitud KPI 1.4)
- [ ] Implementar evento `breathing_technique_used` en `EjercicioRespiracion`
- [ ] Implementar muestreo `hr_sample` cada 1s en `SimulacionBPM`
- [ ] Implementar eventos `retry_clicked` y `distractor_interference`
- [ ] Mejora de ruido ambiental y movimientos de avatares — Nivel Base
- [ ] Incremento de avatares y distracción auditiva — Nivel Medio

### Sprint 3 — Nivel Difícil
- [ ] Diseñar e implementar Nivel Difícil (ruido intenso, más avatares, distractores visuales y auditivos simultáneos)
- [ ] Eliminar código comentado en `EjercicioRespiracion.cs` (líneas 66, 68, 94, 96…)
- [ ] Estandarizar nombres: renombrar `"Movimiento estatico robot.cs"` → clase `RobotVolador`; corregir `"Vehicule"` → `"Vehicle"` en spawners y tags
- [ ] Optimizar checks en `Update()` de `DialogoTips` y `SimulacionBPM` reemplazándolos por eventos puntuales
- [ ] Reemplazar `transform.Find("SelectedImage")` en `PreguntasFormulario.cs` por referencia directa en Inspector
- [ ] Construir `FacilitatorPanel` con `facilitator_error_log` y `facilitator_rubric_speech`
- [ ] Registrar `emotion_check_in` en 3 momentos (pre / durante / post)

### Sprint 4 — Medición KPIs, datos y cierre
- [ ] Implementar `LocalStorageWriter`: JSON maestro por sesión + CSV resumen con KPIs calculados
- [ ] Calcular KRIs derivados e integrar alertas booleanas en CSV (umbrales definidos en documento base)
- [ ] Validar captura con al menos 3 sesiones de prueba (checklist completo)
- [ ] Verificar y exportar dataset de emociones para equipo de IA
- [ ] Documentación técnica final del proyecto
- [ ] Prueba de regresión completa en lentes
- [ ] Ajustes finales y cierre

---

## Convenciones de código

- Todos los valores configurables deben exponerse con `[SerializeField]`, nunca hardcodeados
- Usar `UnityEvent` o `System.Action` para comunicación entre scripts — evitar referencias directas entre sistemas
- Validar índices de arrays antes de acceder: `if (index >= 0 && index < array.Length)`
- Agregar null check antes de usar referencias de Inspector: `if (robot != null)`
- No usar `transform.Find()` en runtime — asignar referencias en el Inspector
- Evitar lógica en `Update()` que pueda reemplazarse con eventos o callbacks
- Los nombres de clases y archivos deben coincidir exactamente

---

## Dashboard de avance (generado en Claude CoWork)

Este archivo (`CLAUDE.md`) se usa como contexto para generar un dashboard de progreso del proyecto en Claude Code mode **CoWork**.

### Cómo usarlo

1. **Copia todo este contenido** (CLAUDE.md completo)
2. **Abre Claude Code → CoWork mode**
3. **Pega CLAUDE.md como contexto** (primer mensaje)
4. **Adjunta tu Gantt** (exportado como CSV, imagen, o descripción)
5. **Solicita al sistema**: _"Genera un dashboard de progreso basado en este CLAUDE.md y el Gantt. Incluye: % completado global, estado por sprint, bugs críticos abiertos, últimos cambios relevantes, próximas acciones recomendadas."_

### Qué espera el dashboard

El sistema generará un markdown o HTML con:

| Sección | Contenido |
|---|---|
| **Resumen ejecutivo** | % global, sprint actual, ETA de hitos |
| **Estado por sprint** | ✅/⏳/❌ items, burndown visual, riesgos |
| **Bugs críticos** | Issues Sprint 1-4 sin resolver |
| **Últimos 5 commits** | `git log --oneline -5` (relevantes) |
| **Próximas acciones** | Top 3 tareas "hacer ahora" |
| **Métricas clave** | Velocidad (items/sprint), trend |

### Frecuencia de actualización

- **Cada semana** tras revisar avances
- **Tras cierre de sprint** para snapshot
- **Ad-hoc** si solicitas estado específico en CoWork

### Nota

El dashboard **no se versionea** en git. Es un documento de trabajo local generado bajo demanda en CoWork. CLAUDE.md es la "fuente de verdad" que alimenta su generación.
