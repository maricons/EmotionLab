using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSelector : MonoBehaviour
{
    public static string presentacionSeleccionada;
    public static string escenaSeleccionada;

    [Header("Formulario del Clipboard")]
    [Tooltip("Preguntas del formulario del Clipboard (Waiting Room). El botón 'Empezar' " +
             "permanece deshabilitado hasta que todas tengan una respuesta seleccionada.")]
    [SerializeField] private PreguntasFormulario[] preguntasClipboard;
    [Tooltip("Botón 'Empezar' del board. Se habilita/deshabilita según el estado del formulario.")]
    [SerializeField] private Button botonEmpezar;

    [Header("Pizarra de selección")]
    [Tooltip("GameObject 'Presentaciones' (tablero de selección de presentación). Permanece oculto " +
             "hasta que se llame a MostrarPizarra().")]
    [SerializeField] private GameObject panelPresentaciones;
    [Tooltip("GameObject 'Facil' (contenedor de Button_1 en Escenarios). Permanece oculto " +
             "hasta que se llame a MostrarPizarra().")]
    [SerializeField] private GameObject cajaFacil;
    [Tooltip("GameObject 'Dificil' (contenedor de Button_2 en Escenarios). Se revela solo si el modo " +
             "Fácil ya se completó en esta sesión; se oculta por completo en caso contrario.")]
    [SerializeField] private GameObject cajaDificil;

    [Header("Highlight de selección faltante")]
    [Tooltip("Tablero de selección de presentación. Se resalta si se presiona 'Iniciar' sin elegir una.")]
    [SerializeField] private PreguntasPizarra tableroPresentaciones;
    [Tooltip("Tablero de selección de escena (Fácil/Difícil). Se resalta si se presiona 'Iniciar' sin elegir una.")]
    [SerializeField] private PreguntasPizarra tableroEscenarios;

    void Start()
    {
        foreach (PreguntasFormulario pregunta in preguntasClipboard)
        {
            if (pregunta != null)
                pregunta.OnRespuestaSeleccionada += ActualizarBotonEmpezar;
        }

        ActualizarBotonEmpezar();
    }

    void OnDestroy()
    {
        foreach (PreguntasFormulario pregunta in preguntasClipboard)
        {
            if (pregunta != null)
                pregunta.OnRespuestaSeleccionada -= ActualizarBotonEmpezar;
        }
    }

    private bool TodasLasPreguntasRespondidas()
    {
        if (preguntasClipboard == null || preguntasClipboard.Length == 0)
        {
            Debug.LogWarning("MenuSelector: 'preguntasClipboard' no está configurado. " +
                              "El botón Empezar no exigirá responder el formulario.");
            return true;
        }

        foreach (PreguntasFormulario pregunta in preguntasClipboard)
        {
            if (pregunta != null && !pregunta.HasAnswer())
                return false;
        }
        return true;
    }

    private void ActualizarBotonEmpezar()
    {
        if (botonEmpezar != null)
            botonEmpezar.interactable = TodasLasPreguntasRespondidas();
    }

    /// <summary>
    /// Revela el tablero de selección de presentación. Se llama desde
    /// DialogoPreparacion cuando Labbu muestra el panel "Elección presentación",
    /// para que aparezca junto con esa parte de la explicación (no antes).
    /// </summary>
    public void MostrarPresentaciones()
    {
        if (panelPresentaciones != null)
            panelPresentaciones.SetActive(true);
    }

    /// <summary>
    /// Revela el tablero de selección de escena (Fácil/Difícil). Se llama desde
    /// DialogoPreparacion cuando Labbu muestra el panel "Elección lugar", para
    /// que aparezca junto con esa parte de la explicación (no antes).
    /// </summary>
    public void MostrarEscenarios()
    {
        if (cajaFacil != null)
            cajaFacil.SetActive(true);

        if (cajaDificil != null)
        {
            bool desbloqueado = EmotionDataManager.Instance != null && EmotionDataManager.Instance.FacilCompletado;
            cajaDificil.SetActive(desbloqueado);
        }
    }

    // Llamado por los botones de selección de presentación
    public void SeleccionarPresentacion(string nombrePresentacion)
    {
        presentacionSeleccionada = nombrePresentacion;
        PlayerPrefs.SetString("PresentacionSeleccionada", nombrePresentacion);
        Debug.Log("Presentación seleccionada: " + nombrePresentacion);
    }

    // Llamado por los botones de selección de escena
    public void SeleccionarEscena(string nombreEscena)
    {
        bool esDificil = nombreEscena == "Salón";
        bool dificilDesbloqueado = EmotionDataManager.Instance != null && EmotionDataManager.Instance.FacilCompletado;

        if (esDificil && !dificilDesbloqueado)
        {
            // La caja "Dificil" debería estar oculta (ver Start()) mientras esto sea falso;
            // este chequeo es solo una red de seguridad por si igual se llega a llamar.
            Debug.LogWarning("Modo Difícil bloqueado: primero debe completarse el modo Fácil.");
            return;
        }

        escenaSeleccionada = nombreEscena;
        PlayerPrefs.SetString("EscenaSeleccionada", nombreEscena);
        Debug.Log("Escena seleccionada: " + nombreEscena);

        // Registrar dificultad en EmotionDataManager para el JSON de sesión
        string dificultad = esDificil ? "dificil" : "facil";
        PlayerPrefs.SetString("DificultadSeleccionada", dificultad);
        EmotionDataManager.Instance?.SetDifficultyLevel(dificultad);
    }

    // Llamado por el botón "Iniciar"
    public void Iniciar()
    {
        if (!TodasLasPreguntasRespondidas())
        {
            Debug.LogWarning("Debes responder el formulario del Clipboard antes de iniciar.");
            return;
        }

        bool faltaPresentacion = string.IsNullOrEmpty(presentacionSeleccionada);
        bool faltaEscena = string.IsNullOrEmpty(escenaSeleccionada);

        if (faltaPresentacion || faltaEscena)
        {
            Debug.LogWarning("Debes seleccionar una presentación y una escena antes de iniciar.");

            if (faltaPresentacion && tableroPresentaciones != null)
                tableroPresentaciones.ResaltarOpciones();
            if (faltaEscena && tableroEscenarios != null)
                tableroEscenarios.ResaltarOpciones();

            return;
        }

        SceneManager.LoadScene(escenaSeleccionada);
    }
}

