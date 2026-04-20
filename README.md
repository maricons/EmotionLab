# 🧠 EmotionLab: Experiencia de Regulación Emocional en VR

> **Proyecto de Investigación y Desarrollo — LIDETI**
> Herramienta inmersiva para el entrenamiento de habilidades blandas y gestión del estrés académico en entornos controlados.

![Status](https://img.shields.io/badge/Status-En%20Desarrollo-yellow)
![Unity](https://img.shields.io/badge/Engine-Unity%202022.3%20LTS-black)
![Platform](https://img.shields.io/badge/Platform-Meta%20Quest%202%2F3%20(OpenXR)-blue)

---

## 📖 Descripción General

**EmotionLab** es una plataforma de Realidad Virtual diseñada para promover el bienestar emocional y el aprendizaje autorregulado. Utiliza un entorno **"Safe-to-fail"** donde los estudiantes enfrentan desafíos de oratoria y fallas técnicas para entrenar la identificación y gestión de emociones mediante técnicas de respiración diafragmática y biofeedback simulado.

---

## 🎮 Flujo de la Experiencia (Scene Flow)

La experiencia sigue un recorrido diseñado para transitar desde la preparación hasta la recuperación emocional:

### 1. Waiting Room (Sala de Preparación)
Es el nodo central de configuración donde el usuario define su experiencia:
* **Check-in Emocional:** Selección de estado anímico inicial mediante UI diegética.
* **Configuración de Tarea:** * Elección del **tema de la presentación**.
    * Selección del entorno de exposición: **Oficina** o **Classroom (Aula)**.
* **Nivel de Dificultad (Próximamente):**
    * *Fácil:* Entorno estable con distracciones mínimas.
    * *Difícil:* Incremento de ruido ambiental, interrupciones y fallos técnicos frecuentes.

### 2. Módulos de Tarea (Exposición)
El usuario es transportado al entorno elegido para realizar su presentación.
* **Evento Disruptivo:** A los 110 segundos, ocurre una falla técnica (PPT/Generador) que induce un pico de estrés.
* **Intervención:** El guía **Labbu Robot** interviene para dirigir al usuario hacia un ejercicio de respiración guiada (3 ciclos) para regular su frecuencia cardíaca simulada.

### 3. Sala de Cierre (Módulo de Gratificación)
Una vez superada la tarea, el usuario accede a un espacio de relajación final:
* **Experiencia de Patitos:** Un mini-juego lúdico donde el usuario interactúa recolectando patos en una canasta, sirviendo como cierre positivo y descarga de tensión.
* **Check-out Emocional (Próximamente):** Evaluación final para comparar el estado anímico pre y post experiencia.
* **Exportación de Datos (Próximamente):** Generación automática de logs de rendimiento y KPIs.

---

## 🛠️ Características Técnicas (Key Features)

* **❤️ Biofeedback Simulado:** Algoritmo de frecuencia cardíaca reactivo que aumenta ante fallos y disminuye durante la respiración.
* **📊 Telemetría Avanzada:** Captura de datos conductuales y autoreportados exportados a `summary.csv`. (Próximamente)
* **🤖 Labbu Robot:** Sistema de diálogos dinámicos que actúa como facilitador de la experiencia.
* **🕹️ Mecánicas de Interacción:** Basadas en XR Interaction Toolkit para una manipulación natural de objetos (punteros, agarre de patitos).

---

## 🗺️ Roadmap e Hitos (Planificación 2026)

## 🗺️ Roadmap e Hitos (Planificación 2026)

El desarrollo se organiza en 4 Sprints técnicos, con un fuerte enfoque en la estabilidad del código y la captura de datos para análisis posterior.

| Estado | Fase | Periodo | Actividades Clave y Entregables | KPI / Hito Técnico |
| :---: | :--- | :--- | :--- | :--- |
| 🔄 | **Sprint 1: Estabilidad y Telemetría Base** | 13/04 - 22/05 | • Refactorización de Diálogos y corrección de concurrencia.<br>• Pantalla de Consentimiento Informado.<br>• Implementación de `TelemetryService` y `SessionStore`. | `emotion_check_in` (Pre/Post) y Formularios (UEQ-S). |
| 📅 | **Sprint 2: Niveles y Refactorización** | 25/05 - 22/06 | • Desacoplamiento de scripts mediante `UnityEvent`.<br>• Mejora de niveles (Base y Medio): audio y avatares.<br>• Muestreo de HR (BPM) cada 1s. | `breathing_technique_used` y Tasa de completitud. |
| 📅 | **Sprint 3: Nivel Difícil y Facilitador** | 23/06 - 10/07 | • Implementación de Nivel Difícil (ruido intenso y distracciones).<br>• Construcción del `FacilitatorPanel` (Rúbrica de discurso).<br>• Registro de emociones en 3 momentos. | `facilitator_error_log` y Limpieza de código. |
| 📅 | **Sprint 4: Exportación y Cierre** | 14/07 - 29/07 | • Verificación de datos para equipo de IA.<br>• `LocalStorageWriter`: Exportación de JSON Maestro y CSV.<br>• Documentación técnica y pruebas de regresión. | Validación de Dataset y Cierre de Proyecto. |
---

## 🚀 Instalación y Desarrollo

### Requisitos Previos
* **Unity Hub** instalado.
* **Editor Unity 2022.3.62f2** (Android Build Support).

### Configuración del Repositorio
1.  **Clonar el repositorio:**
    ```bash
    git clone https://github.com/KatalinaPerez/EmotionLab.git
    ```
2.  Abrir el proyecto en Unity.
3.  **Importar Assets:** Asegurarse de importar los *Starter Assets* de **XR Interaction Toolkit** desde el Package Manager.
4.  **Pruebas:** Para pruebas sin lentes, utilizar el **XR Device Simulator** incluido en el objeto `[XR Rig]`.

### Convenciones de Código
* Uso de `[SerializeField]` para configuración en Inspector.
* Comunicación mediante `UnityEvent` para desacoplar sistemas.
* **Prohibido:** Uso de `transform.Find()` en runtime; usar referencias directas.

---

## 👥 Equipo y Créditos

Este proyecto es una colaboración desarrollada por:

* **Katalina Pérez**: Lead UX/UI & Interaction Designer. Responsable del diseño de la experiencia de usuario, interfaces interactivas y el desarrollo de módulos de regulación emocional.
* **Constanza Quiero**: Lead Systems & Immersive Logic Developer. Responsable de la arquitectura de sistemas, persistencia de datos (JSON) y la creación de atmósferas sonoras inmersivas.
* **LIDETI**: Laboratorio de Investigación y Desarrollo de Tecnologías Inmersivas.

---
© 2026 EmotionLab. Todos los derechos reservados.

**Autor:** Constanza Quiero B.
**Licencia:** Uso Académico.
