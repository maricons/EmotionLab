using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections; // Necesario para las corrutinas


/// <summary>
/// Singleton que persiste entre escenas y almacena las respuestas de los
/// formularios emocionales del participante. Guarda todo a un JSON en disco.
///
/// Ruta del archivo: Application.persistentDataPath/EmotionLabSesiones/
///   - En PC:   C:\Users\USUARIO\AppData\LocalLow\[Company]\EmotionLab\...
///   - En Quest: /storage/emulated/0/Android/data/[packageName]/files/...
///
/// Uso tÃ­pico:
///   EmotionDataManager.Instance.IniciarSesion("participante_001");
///   EmotionDataManager.Instance.RegistrarRespuesta(
///       "waiting_room", "sentir_hoy", "Â¿CÃ³mo te sientes hoy?", "bien", 3);
///   EmotionDataManager.Instance.GuardarAJson();
/// </summary>
public class EmotionDataManager : MonoBehaviour
{
    public static EmotionDataManager Instance { get; private set; }

    [Header("ConfiguraciÃ³n")]
    [Tooltip("Nombre de carpeta donde se guardan los JSON de sesiÃ³n.")]
    public string carpetaSesiones = "EmotionLabSesiones";
    [SerializeField]
    public string apiUrl = "https://cntbcwfel0.execute-api.us-east-1.amazonaws.com/prod/data";

    [Tooltip("Si estÃ¡ activo, tambiÃ©n imprime el JSON en la consola al guardar.")]
    public bool logEnConsola = true;

    [Tooltip("Guarda a disco automÃ¡ticamente al cambiar de escena.")]
    public bool guardarAlCambiarEscena = true;

    [Tooltip("Guarda a disco automÃ¡ticamente al cerrar la aplicaciÃ³n o pausarla (Quest).")]
    public bool guardarAlCerrarApp = true;

    // ---- Datos de la sesiÃ³n actual ----
    public SesionData sesionActual;

    void Awake()
    {
        // ImplementaciÃ³n singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Si nadie iniciÃ³ sesiÃ³n aÃºn, la iniciamos de forma automÃ¡tica
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
    /// Inicia una nueva sesiÃ³n. Llamar al principio del experimento.
    /// Si idParticipante es null/vacÃ­o se genera uno automÃ¡tico.
    /// </summary>
    public void IniciarSesion(string idParticipante)
    {
        // Si ya hay una sesiÃ³n activa y con ID vÃ¡lido, no la reiniciamos para no perder respuestas
        if (sesionActual != null && !string.IsNullOrEmpty(sesionActual.idParticipante) && string.IsNullOrEmpty(idParticipante))
        {
            if (logEnConsola)
                Debug.Log($"[EmotionDataManager] SesiÃ³n existente conservada: {sesionActual.idParticipante}");
            return;
        }

        // Generamos un cÃ³digo alfanumÃ©rico Ãºnico de 8 caracteres al final (GUID)
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

        if (logEnConsola)
            Debug.Log($"[EmotionDataManager] SesiÃ³n iniciada CON Ã‰XITO: {sesionActual.idParticipante}");

        LogEvent("session_started", "inicio");
    }

    /// <summary>
    /// Punto Ãºnico de entrada para registrar eventos KPI puntuales
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
    /// Registra una respuesta NUEVA en la sesiÃ³n actual (siempre aÃ±ade).
    /// Usar para registros tipo log/timeline.
    /// </summary>
    public void RegistrarRespuesta(string escena, string idPregunta, string textoPregunta,
                                   string respuestaLabel, int respuestaIndex)
    {
        if (sesionActual == null || string.IsNullOrEmpty(sesionActual.idParticipante)) IniciarSesion(null);

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
        if (sesionActual == null || string.IsNullOrEmpty(sesionActual.idParticipante)) IniciarSesion(null);
        if (sesionActual.respuestas == null)
            sesionActual.respuestas = new List<RespuestaData>();

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
            existente.textoPregunta = textoPregunta; // por si se editÃ³
            existente.timestamp = timestamp; // timestamp de la Ãºltima modificaciÃ³n
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
    /// Guarda el estado completo de la sesiÃ³n a un archivo JSON.
    /// Retorna la ruta absoluta del archivo guardado (o null si fallÃ³).
    /// </summary>
    public string GuardarAJson()
    {
        if (sesionActual == null || string.IsNullOrEmpty(sesionActual.idParticipante))
            {
                Debug.LogWarning("[EmotionDataManager] No hay sesiÃ³n activa vÃ¡lida para guardar.");
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
    /// Devuelve la ruta completa donde se guardan los JSON (Ãºtil para mostrÃ¡rsela al investigador).
    /// </summary>
    public string GetRutaCarpeta()
    {
        return Path.Combine(Application.persistentDataPath, carpetaSesiones);
    }

    /// <summary>
    /// Registra el nivel de dificultad de la sesiÃ³n ("facil" | "dificil").
    /// Llamar desde MenuSelector al seleccionar la escena.
    /// </summary>
    public void SetDifficultyLevel(string level)
    {
        if (sesionActual == null || string.IsNullOrEmpty(sesionActual.idParticipante)) IniciarSesion(null);
        sesionActual.difficulty_level = level;
        if (logEnConsola)
            Debug.Log($"[EmotionDataManager] difficulty_level = {level}");
    }

    /// <summary>
    /// Marca la sesiÃ³n como finalizada registrando la hora de cierre.
    /// Llamar desde FormularioCierre al completar el formulario.
    /// </summary>

    public void FinalizarSesion()
    {
        if (sesionActual == null) return;
        sesionActual.fechaFin = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        LogEvent("session_completed", "cierre");
        if (logEnConsola)
            Debug.Log($"[EmotionDataManager] SesiÃ³n finalizada: {sesionActual.fechaFin}");
    }

    public SesionData GetSesionActual() => sesionActual;

    // ---- Estado de completado / flujo FÃ¡cil-DifÃ­cil / rehacer sesiÃ³n ----

    public bool FacilCompletado => sesionActual != null && sesionActual.facilCompletado;
    public bool DificilCompletado => sesionActual != null && sesionActual.dificilCompletado;
    public bool RetryUsado => sesionActual != null && sesionActual.retryUsado;
    public string UltimoModoCompletado => sesionActual?.ultimoModoCompletado;

    /// <summary>
    /// Marca un modo ("facil" | "dificil") como completado en la sesiÃ³n actual.
    /// Llamar al finalizar Oficina (FÃ¡cil) o SalÃ³n (DifÃ­cil). RetroalimentaciÃ³n
    /// usa "ultimoModoCompletado" para decidir si debe encadenar al modo DifÃ­cil
    /// o continuar hacia Cierre.
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

    /// <summary>
    /// Marca que ya se usÃ³ el (Ãºnico) "Volver a Empezar" de esta sesiÃ³n.
    /// Llamar desde el botÃ³n "Volver a Empezar" en Cierre, antes de recargar
    /// Waiting Room.
    /// </summary>
    public void MarcarRetryUsado()
    {
        if (sesionActual == null) return;
        sesionActual.retryUsado = true;
        LogEvent("retry_clicked", "cierre");
    }

    /// <summary>
    /// Genera una sesiÃ³n completamente nueva (nuevo SessionID), descartando el
    /// estado de completado de la sesiÃ³n anterior. Llamar desde el botÃ³n
    /// "Nueva Experiencia" en Cierre, antes de recargar Waiting Room.
    /// </summary>
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

        if (logEnConsola)
            Debug.Log($"[EmotionDataManager] Nueva experiencia iniciada: {sesionActual.idParticipante}");

        LogEvent("session_started", "nueva_experiencia");
    }

    // ======================================================================
    // Estructuras serializables (JsonUtility necesita [Serializable] + campos pÃºblicos)
    // ======================================================================

    /// <summary>
    /// EnvÃ­a los datos de la sesiÃ³n actual en formato JSON hacia la API de simulaciÃ³n (Beeceptor).
    /// </summary>
    public void EnviarDatosAlServidor()
    {
        if (sesionActual == null) return;

        // Convertimos el objeto de sesiÃ³n actual a un string JSON limpio
        string jsonPayload = JsonUtility.ToJson(sesionActual, prettyPrint: false);
        
        // Iniciamos la corrutina para hacer la peticiÃ³n web en segundo plano
        StartCoroutine(EnviarPostHttp(jsonPayload));
    }

    private IEnumerator EnviarPostHttp(string json)
    {
        // Creamos la peticiÃ³n POST apuntando a tu URL de Beeceptor
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

            // Esperamos a que la peticiÃ³n termine
            yield return request.SendWebRequest();

            Debug.Log("[EmotionDataManager] URL REAL que se estÃ¡ llamando: " + request.url);

            // Revisamos el resultado
            if (request.result == UnityWebRequest.Result.Success)
            {
                if (logEnConsola)
                {
                    Debug.Log("[EmotionDataManager] Â¡Datos subidos con Ã©xito a Beeceptor!");
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
