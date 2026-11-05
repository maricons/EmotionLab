using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

/// <summary>
/// Singleton que persiste entre escenas y almacena las respuestas de los
/// formularios emocionales del participante. Guarda todo a un JSON en disco
/// y lo sube al backend en la nube.
///
/// Ruta del archivo: Application.persistentDataPath/EmotionLabSesiones/
///   - En PC:   C:\Users\USUARIO\AppData\LocalLow\[Company]\EmotionLab\...
///   - En Quest: /storage/emulated/0/Android/data/[packageName]/files/...
/// </summary>
public class EmotionDataManager : MonoBehaviour
{
    public static EmotionDataManager Instance { get; private set; }

    [Header("Configuración")]
    [Tooltip("Nombre de carpeta donde se guardan los JSON de sesión.")]
    public string carpetaSesiones = "EmotionLabSesiones";

    [SerializeField]
    public string apiUrl = "https://cntbcwfel0.execute-api.us-east-1.amazonaws.com/prod/data";

    [Tooltip("Si está activo, también imprime el JSON en la consola al guardar.")]
    public bool logEnConsola = true;

    [Tooltip("Guarda a disco automáticamente al cambiar de escena.")]
    public bool guardarAlCambiarEscena = true;

    [Tooltip("Guarda a disco automáticamente al cerrar la aplicación o pausarla (Quest).")]
    public bool guardarAlCerrarApp = true;

    [Header("Seguimiento de Flujo")]
    [Tooltip("Última escena jugada (Oficina o Salon) para contextualizar retroalimentación.")]
    public string ultimaEscenaJugada = "";

    [Tooltip("Contador de visitas a la escena de Retroalimentación.")]
    public int visitasRetroalimentacion = 0;

    // ---- Datos de la sesión actual ----
    public SesionData sesionActual;

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

        // Hooks de auto-guardado y tracking de escenas
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string nombre = scene.name;
        if (logEnConsola)
            Debug.Log($"[EmotionDataManager] Escena cargada: {nombre}");

        if (nombre.IndexOf("Oficina", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            ultimaEscenaJugada = "Oficina";
            MarcarModoCompletado("facil");
            SetDifficultyLevel("facil");
        }
        else if (nombre.IndexOf("Sal", StringComparison.OrdinalIgnoreCase) >= 0) // Salón / Salon
        {
            ultimaEscenaJugada = "Salon";
            MarcarModoCompletado("dificil");
            SetDifficultyLevel("dificil");
        }
        else if (nombre.IndexOf("Retroalimentacion", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            visitasRetroalimentacion++;
            if (logEnConsola)
                Debug.Log($"[EmotionDataManager] Entrada a Retroalimentación #{visitasRetroalimentacion} (procedente de: {ultimaEscenaJugada})");

            LogEvent("visita_retroalimentacion", $"visita_{visitasRetroalimentacion}", ultimaEscenaJugada);
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        string nombre = scene.name;
        if (nombre.IndexOf("Oficina", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            ultimaEscenaJugada = "Oficina";
        }
        else if (nombre.IndexOf("Sal", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            ultimaEscenaJugada = "Salon";
        }

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
        // Si ya hay una sesión activa y con ID válido, no la reiniciamos para no perder respuestas
        if (sesionActual != null && !string.IsNullOrEmpty(sesionActual.idParticipante) && string.IsNullOrEmpty(idParticipante))
        {
            if (logEnConsola)
                Debug.Log($"[EmotionDataManager] Sesión existente conservada: {sesionActual.idParticipante}");
            return;
        }

        // Generamos un código alfanumérico único de 8 caracteres al final (GUID)
        string hashUnico = Guid.NewGuid().ToString().Substring(0, 8);

        string idFinal = string.IsNullOrEmpty(idParticipante) 
            ? "participante_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + hashUnico
            : idParticipante;

        sesionActual = new SesionData
        {
            idParticipante = idFinal,
            fechaInicio = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            respuestas = new List<RespuestaData>(),
            eventos = new List<EventoData>()
        };

        visitasRetroalimentacion = 0;
        ultimaEscenaJugada = "";

        if (logEnConsola)
            Debug.Log($"[EmotionDataManager] Sesión iniciada CON ÉXITO: {sesionActual.idParticipante}");

        LogEvent("session_started", "inicio");
    }

    /// <summary>
    /// Punto único de entrada para registrar eventos KPI puntuales
    /// (ej. "session_started", "session_completed", "breathing_technique_used").
    /// </summary>
    public void LogEvent(string nombre, string fase = null, string payload = null)
    {
        if (sesionActual == null || string.IsNullOrEmpty(sesionActual.idParticipante)) IniciarSesion(null);
        if (sesionActual.eventos == null)
            sesionActual.eventos = new List<EventoData>();

        sesionActual.eventos.Add(new EventoData
        {
            nombre = nombre,
            fase = fase,
            payload = payload,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
        });

        if (logEnConsola)
            Debug.Log($"[EmotionDataManager] Evento: {nombre} ({fase}) {payload}");
    }

    /// <summary>
    /// Resuelve el nombre de escena para guardar la respuesta.
    /// Si la respuesta es de Retroalimentación, la contextualiza como
    /// "Retroalimentacion_Oficina" o "Retroalimentacion_Salon" para evitar
    /// que la segunda pasada sobreescriba las respuestas de la primera.
    /// </summary>
    public string ResolverEscenaContextual(string escena)
    {
        if (string.IsNullOrEmpty(escena)) return escena;

        if (escena.IndexOf("Retroalimentacion", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            // Si venimos de Salón, o es la 2da visita o modo difícil ya se completó:
            if (ultimaEscenaJugada == "Salon" || visitasRetroalimentacion >= 2 || (sesionActual != null && sesionActual.dificilCompletado))
            {
                return "Retroalimentacion_Salon";
            }
            else
            {
                return "Retroalimentacion_Oficina";
            }
        }

        return escena;
    }

    /// <summary>
    /// Registra una respuesta NUEVA en la sesión actual (siempre añade).
    /// Usar para registros tipo log/timeline.
    /// </summary>
    public void RegistrarRespuesta(string escena, string idPregunta, string textoPregunta,
                                   string respuestaLabel, int respuestaIndex)
    {
        if (sesionActual == null || string.IsNullOrEmpty(sesionActual.idParticipante)) IniciarSesion(null);

        string escenaContextual = ResolverEscenaContextual(escena);

        sesionActual.respuestas.Add(new RespuestaData
        {
            escena = escenaContextual,
            idPregunta = idPregunta,
            textoPregunta = textoPregunta,
            respuestaLabel = respuestaLabel,
            respuestaIndex = respuestaIndex,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
        });

        if (logEnConsola)
            Debug.Log($"[EmotionDataManager] Registrada: [{escenaContextual}] {idPregunta} = {respuestaLabel}");
    }

    /// <summary>
    /// Registra o ACTUALIZA una respuesta (si ya existe una para la misma escena + idPregunta,
    /// la reemplaza en vez de duplicar).
    /// Si la escena es Retroalimentación, contextualiza automáticamente según la fase jugada
    /// (Oficina o Salón) para conservar ambas respuestas sin sobreescribirlas.
    /// </summary>
    public void RegistrarOActualizarRespuesta(string escena, string idPregunta, string textoPregunta,
                                              string respuestaLabel, int respuestaIndex)
    {
        if (sesionActual == null || string.IsNullOrEmpty(sesionActual.idParticipante)) IniciarSesion(null);
        if (sesionActual.respuestas == null)
            sesionActual.respuestas = new List<RespuestaData>();

        string escenaContextual = ResolverEscenaContextual(escena);

        // Buscar respuesta existente para la misma escena contextual + pregunta
        RespuestaData existente = null;
        foreach (var r in sesionActual.respuestas)
        {
            if (r.escena == escenaContextual && r.idPregunta == idPregunta)
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
            existente.textoPregunta = textoPregunta;
            existente.timestamp = timestamp;
        }
        else
        {
            sesionActual.respuestas.Add(new RespuestaData
            {
                escena = escenaContextual,
                idPregunta = idPregunta,
                textoPregunta = textoPregunta,
                respuestaLabel = respuestaLabel,
                respuestaIndex = respuestaIndex,
                timestamp = timestamp
            });
        }

        if (logEnConsola)
            Debug.Log($"[EmotionDataManager] {(existente != null ? "Actualizada" : "Registrada")}: " +
                      $"[{escenaContextual}] {idPregunta} = {respuestaLabel}");
    }

    /// <summary>
    /// Guarda el estado completo de la sesión a un archivo JSON.
    /// Retorna la ruta absoluta del archivo guardado (o null si falló).
    /// </summary>
    public string GuardarAJson()
    {
        if (sesionActual == null || string.IsNullOrEmpty(sesionActual.idParticipante))
        {
            Debug.LogWarning("[EmotionDataManager] No hay sesión activa válida para guardar.");
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
            
            EnviarDatosAlServidor();

            if (logEnConsola)
            {
                Debug.Log($"[EmotionDataManager] JSON guardado en:\n{rutaCompleta}");
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

    /// <summary>
    /// Registra el nivel de dificultad de la sesión ("facil" | "dificil").
    /// </summary>
    public void SetDifficultyLevel(string level)
    {
        if (sesionActual == null || string.IsNullOrEmpty(sesionActual.idParticipante)) IniciarSesion(null);
        sesionActual.difficulty_level = level;
        if (logEnConsola)
            Debug.Log($"[EmotionDataManager] difficulty_level = {level}");
    }

    /// <summary>
    /// Marca la sesión como finalizada registrando la hora de cierre.
    /// </summary>
    public void FinalizarSesion()
    {
        if (sesionActual == null) return;
        sesionActual.fechaFin = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        LogEvent("session_completed", "cierre");
        if (logEnConsola)
            Debug.Log($"[EmotionDataManager] Sesión finalizada: {sesionActual.fechaFin}");
    }

    public SesionData GetSesionActual() => sesionActual;

    // ---- Estado de completado / flujo Fácil-Difícil / rehacer sesión ----

    public bool FacilCompletado => sesionActual != null && sesionActual.facilCompletado;
    public bool DificilCompletado => sesionActual != null && sesionActual.dificilCompletado;
    public bool RetryUsado => sesionActual != null && sesionActual.retryUsado;
    public string UltimoModoCompletado => sesionActual?.ultimoModoCompletado;

    /// <summary>
    /// Marca un modo ("facil" | "dificil") como completado en la sesión actual.
    /// </summary>
    public void MarcarModoCompletado(string modo)
    {
        if (sesionActual == null || string.IsNullOrEmpty(sesionActual.idParticipante)) IniciarSesion(null);

        sesionActual.ultimoModoCompletado = modo;
        if (modo == "facil") sesionActual.facilCompletado = true;
        else if (modo == "dificil") sesionActual.dificilCompletado = true;

        if (logEnConsola)
            Debug.Log($"[EmotionDataManager] Modo completado: {modo}");

        LogEvent("modo_completado", modo);
    }

    public void MarcarRetryUsado()
    {
        if (sesionActual == null) return;
        sesionActual.retryUsado = true;
        LogEvent("retry_clicked", "cierre");
    }

    public void IniciarNuevaSesion()
    {
        string hashUnico = Guid.NewGuid().ToString().Substring(0, 8);
        string idNuevo = "participante_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + hashUnico;

        sesionActual = new SesionData
        {
            idParticipante = idNuevo,
            fechaInicio = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            respuestas = new List<RespuestaData>(),
            eventos = new List<EventoData>()
        };

        visitasRetroalimentacion = 0;
        ultimaEscenaJugada = "";

        if (logEnConsola)
            Debug.Log($"[EmotionDataManager] Nueva experiencia iniciada: {sesionActual.idParticipante}");

        LogEvent("session_started", "nueva_experiencia");
    }

    // ======================================================================
    // Estructuras serializables (JsonUtility necesita [Serializable] + campos públicos)
    // ======================================================================

    /// <summary>
    /// Envía los datos de la sesión actual en formato JSON hacia la API en la nube.
    /// </summary>
    public void EnviarDatosAlServidor()
    {
        if (sesionActual == null) return;

        // Convertimos el objeto de sesión actual a un string JSON limpio
        string jsonPayload = JsonUtility.ToJson(sesionActual, prettyPrint: false);
        
        // Iniciamos la corrutina para hacer la petición web en segundo plano
        StartCoroutine(EnviarPostHttp(jsonPayload));
    }

    private IEnumerator EnviarPostHttp(string json)
    {
        // Sanitizar URL asegurando protocolo https://
        string urlDestino = apiUrl;
        if (!string.IsNullOrEmpty(urlDestino))
        {
            urlDestino = urlDestino.Trim();
            if (!urlDestino.StartsWith("http://") && !urlDestino.StartsWith("https://"))
            {
                urlDestino = "https://" + urlDestino;
            }
        }

        using (UnityWebRequest request = new UnityWebRequest(urlDestino, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            // Le especificamos al servidor que lo que enviamos es un JSON
            request.SetRequestHeader("Content-Type", "application/json");

            if (logEnConsola)
                Debug.Log($"[EmotionDataManager] Enviando JSON a la nube...");

            // Esperamos a que la petición termine
            yield return request.SendWebRequest();

            if (logEnConsola)
                Debug.Log("[EmotionDataManager] URL llamada: " + request.url);

            // Revisamos el resultado
            if (request.result == UnityWebRequest.Result.Success)
            {
                if (logEnConsola)
                {
                    Debug.Log("[EmotionDataManager] ¡Datos subidos con éxito a AWS S3!");
                    Debug.Log($"Respuesta del servidor: {request.downloadHandler.text}");
                }
            }
            else
            {
                Debug.LogError($"[EmotionDataManager] Error al subir datos: {request.error}");
            }
        }
    }

    [Serializable]
    public class SesionData
    {
        public string idParticipante;
        public string fechaInicio;
        public string fechaFin;
        public string difficulty_level;   // "facil" | "dificil"
        public bool facilCompletado;
        public bool dificilCompletado;
        public bool retryUsado;
        public string ultimoModoCompletado; // "facil" | "dificil"
        public List<RespuestaData> respuestas;
        public List<EventoData> eventos;
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

    [Serializable]
    public class EventoData
    {
        public string nombre;
        public string fase;
        public string payload;
        public string timestamp;
    }
}
