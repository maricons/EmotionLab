using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton que persiste entre escenas y almacena las respuestas de los
/// formularios emocionales del participante. Guarda todo a un JSON en disco.
///
/// Ruta del archivo: Application.persistentDataPath/EmotionLabSesiones/
///   - En PC:   C:\Users\USUARIO\AppData\LocalLow\[Company]\EmotionLab\...
///   - En Quest: /storage/emulated/0/Android/data/[packageName]/files/...
///
/// Uso típico:
///   EmotionDataManager.Instance.IniciarSesion("participante_001");
///   EmotionDataManager.Instance.RegistrarRespuesta(
///       "waiting_room", "sentir_hoy", "¿Cómo te sientes hoy?", "bien", 3);
///   EmotionDataManager.Instance.GuardarAJson();
/// </summary>
public class EmotionDataManager : MonoBehaviour
{
    public static EmotionDataManager Instance { get; private set; }

    [Header("Configuración")]
    [Tooltip("Nombre de carpeta donde se guardan los JSON de sesión.")]
    public string carpetaSesiones = "EmotionLabSesiones";

    [Tooltip("Si está activo, también imprime el JSON en la consola al guardar.")]
    public bool logEnConsola = true;

    [Tooltip("Guarda a disco automáticamente al cambiar de escena.")]
    public bool guardarAlCambiarEscena = true;

    [Tooltip("Guarda a disco automáticamente al cerrar la aplicación o pausarla (Quest).")]
    public bool guardarAlCerrarApp = true;

    // ---- Datos de la sesión actual ----
    private SesionData sesionActual;

    void Awake()
    {
        // Implementación singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Si nadie inició sesión aún, la iniciamos de forma automática
        if (sesionActual == null)
        {
            IniciarSesion(null);
        }

        // Hooks de auto-guardado
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (guardarAlCambiarEscena) GuardarAJson();
    }

    void OnApplicationQuit()
    {
        if (guardarAlCerrarApp) GuardarAJson();
    }

    void OnApplicationPause(bool pausa)
    {
        // En Quest/Android, al quitarse el visor se pausa la app
        if (pausa && guardarAlCerrarApp) GuardarAJson();
    }

    /// <summary>
    /// Inicia una nueva sesión. Llamar al principio del experimento.
    /// Si idParticipante es null/vacío se genera uno automático.
    /// </summary>
    public void IniciarSesion(string idParticipante)
    {
        sesionActual = new SesionData
        {
            idParticipante = string.IsNullOrEmpty(idParticipante)
                ? "participante_" + DateTime.Now.ToString("yyyyMMdd_HHmmss")
                : idParticipante,
            fechaInicio = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            respuestas = new List<RespuestaData>()
        };

        if (logEnConsola)
            Debug.Log($"[EmotionDataManager] Sesión iniciada: {sesionActual.idParticipante}");
    }

    /// <summary>
    /// Registra una respuesta NUEVA en la sesión actual (siempre añade).
    /// Usar para registros tipo log/timeline.
    /// </summary>
    public void RegistrarRespuesta(string escena, string idPregunta, string textoPregunta,
                                   string respuestaLabel, int respuestaIndex)
    {
        if (sesionActual == null) IniciarSesion(null);

        sesionActual.respuestas.Add(new RespuestaData
        {
            escena = escena,
            idPregunta = idPregunta,
            textoPregunta = textoPregunta,
            respuestaLabel = respuestaLabel,
            respuestaIndex = respuestaIndex,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
        });

        if (logEnConsola)
            Debug.Log($"[EmotionDataManager] Registrada: [{escena}] {idPregunta} = {respuestaLabel}");
    }

    /// <summary>
    /// Registra o ACTUALIZA una respuesta (si ya existe una para la misma escena + idPregunta,
    /// la reemplaza en vez de duplicar). Ideal para auto-guardado al seleccionar.
    /// </summary>
    public void RegistrarOActualizarRespuesta(string escena, string idPregunta, string textoPregunta,
                                              string respuestaLabel, int respuestaIndex)
    {
        if (sesionActual == null) IniciarSesion(null);

        // Buscar respuesta existente para misma escena + pregunta
        RespuestaData existente = null;
        foreach (var r in sesionActual.respuestas)
        {
            if (r.escena == escena && r.idPregunta == idPregunta)
            {
                existente = r;
                break;
            }
        }

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        if (existente != null)
        {
            existente.respuestaLabel = respuestaLabel;
            existente.respuestaIndex = respuestaIndex;
            existente.textoPregunta = textoPregunta; // por si se editó
            existente.timestamp = timestamp; // timestamp de la última modificación
        }
        else
        {
            sesionActual.respuestas.Add(new RespuestaData
            {
                escena = escena,
                idPregunta = idPregunta,
                textoPregunta = textoPregunta,
                respuestaLabel = respuestaLabel,
                respuestaIndex = respuestaIndex,
                timestamp = timestamp
            });
        }

        if (logEnConsola)
            Debug.Log($"[EmotionDataManager] {(existente != null ? "Actualizada" : "Registrada")}: " +
                      $"[{escena}] {idPregunta} = {respuestaLabel}");
    }

    /// <summary>
    /// Guarda el estado completo de la sesión a un archivo JSON.
    /// Retorna la ruta absoluta del archivo guardado (o null si falló).
    /// </summary>
    public string GuardarAJson()
    {
        if (sesionActual == null)
        {
            Debug.LogWarning("[EmotionDataManager] No hay sesión activa para guardar.");
            return null;
        }

        try
        {
            string carpeta = Path.Combine(Application.persistentDataPath, carpetaSesiones);
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            string nombreArchivo = $"{sesionActual.idParticipante}.json";
            string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            string json = JsonUtility.ToJson(sesionActual, prettyPrint: true);
            File.WriteAllText(rutaCompleta, json);

            if (logEnConsola)
            {
                Debug.Log($"[EmotionDataManager] JSON guardado en:\n{rutaCompleta}");
                Debug.Log(json);
            }

            return rutaCompleta;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[EmotionDataManager] Error guardando JSON: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Devuelve la ruta completa donde se guardan los JSON (útil para mostrársela al investigador).
    /// </summary>
    public string GetRutaCarpeta()
    {
        return Path.Combine(Application.persistentDataPath, carpetaSesiones);
    }

    public SesionData GetSesionActual() => sesionActual;

    // ======================================================================
    // Estructuras serializables (JsonUtility necesita [Serializable] + campos públicos)
    // ======================================================================

    [Serializable]
    public class SesionData
    {
        public string idParticipante;
        public string fechaInicio;
        public List<RespuestaData> respuestas;
    }

    [Serializable]
    public class RespuestaData
    {
        public string escena;
        public string idPregunta;
        public string textoPregunta;
        public string respuestaLabel;
        public int respuestaIndex;
        public string timestamp;
    }
}
