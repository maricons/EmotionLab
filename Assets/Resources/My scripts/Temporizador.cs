using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Temporizador : MonoBehaviour
{
    [Header("Configuración")]
    public TMP_Text displayText;
    public float tiempoInicial = 60f; // Tiempo en segundos
    public bool autoIniciar = false;

    [Header("Formato")]
    public bool mostrarHoras = false;
    public bool mostrarMilisegundos = false;
    public GameObject robot; // Asignar desde el editor
    public GameObject[] textPanels;
    private int currentPanelIndex = 0;

    [Header("Transición de escena")]
    [Tooltip("Nombre exacto de la escena de cierre. Debe estar añadida en File > Build Settings.")]
    public string escenaSiguiente = "Cierre";

    [Tooltip("Si está activo, al expirar el tiempo se cambia automáticamente a 'escenaSiguiente' " +
             "tras 'delayAntesDeTransicion' segundos. " +
             "RECOMENDADO: dejarlo en FALSE y conectar IrAEscenaSiguiente() al botón 'Comenzar' " +
             "del panel final desde el Inspector (OnClick). Es más determinista; el Invoke se " +
             "cancela si algo desactiva el GameObject del Temporizador.")]
    public bool transicionAutomaticaAlExpirar = false;

    [Tooltip("Solo se usa si 'transicionAutomaticaAlExpirar' está activo.")]
    public float delayAntesDeTransicion = 30f;

    private float tiempoRestante;
    private bool temporizadorActivo = false;
    private bool transicionLanzada = false;

    void Start()
    {
        tiempoRestante = tiempoInicial;
        ActualizarDisplay();

        if (autoIniciar)
            IniciarTemporizador();
    }

    void Update()
    {
        if (temporizadorActivo && tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;

            // Clamp ANTES de pintar para evitar mostrar "-01:-01"
            // cuando deltaTime hace que tiempoRestante quede negativo.
            if (tiempoRestante <= 0)
            {
                tiempoRestante = 0;
                temporizadorActivo = false;
                ActualizarDisplay();
                TemporizadorCompletado();
            }
            else
            {
                ActualizarDisplay();
            }
        }
        else if (tiempoRestante <= 0)
        {
            PausarTemporizador();

            if (textPanels != null && currentPanelIndex >= 0 && currentPanelIndex < textPanels.Length
                && textPanels[currentPanelIndex] != null)
            {
                textPanels[currentPanelIndex].SetActive(true);
            }

            if (robot != null)
                robot.SetActive(true);
        }
    }

    private void ActualizarDisplay()
    {
        if (displayText == null) return;

        if (mostrarHoras)
            displayText.text = FormatearConHoras(tiempoRestante);
        else if (mostrarMilisegundos)
            displayText.text = FormatearConMilisegundos(tiempoRestante);
        else
            displayText.text = FormatearTiempoBasico(tiempoRestante);
    }

    private string FormatearTiempoBasico(float tiempo)
    {
        int minutos = Mathf.FloorToInt(tiempo / 60);
        int segundos = Mathf.FloorToInt(tiempo % 60);
        return string.Format("{0:00}:{1:00}", minutos, segundos);
    }

    private string FormatearConMilisegundos(float tiempo)
    {
        int minutos = Mathf.FloorToInt(tiempo / 60);
        int segundos = Mathf.FloorToInt(tiempo % 60);
        int milisegundos = Mathf.FloorToInt((tiempo * 1000) % 1000);
        return string.Format("{0:00}:{1:00}.{2:000}", minutos, segundos, milisegundos);
    }

    private string FormatearConHoras(float tiempo)
    {
        int horas = Mathf.FloorToInt(tiempo / 3600);
        int minutos = Mathf.FloorToInt((tiempo % 3600) / 60);
        int segundos = Mathf.FloorToInt(tiempo % 60);
        return string.Format("{0:00}:{1:00}:{2:00}", horas, minutos, segundos);
    }

    // Métodos públicos para controlar el temporizador
    public void IniciarTemporizador()
    {
        temporizadorActivo = true;
    }

    public void PausarTemporizador()
    {
        temporizadorActivo = false;
    }

    public void ReiniciarTemporizador()
    {
        tiempoRestante = tiempoInicial;
        ActualizarDisplay();
    }

    public void ConfigurarTiempo(float nuevoTiempo)
    {
        tiempoInicial = nuevoTiempo;
        ReiniciarTemporizador();
    }

    // Evento cuando el temporizador llega a cero
    private void TemporizadorCompletado()
    {
        Debug.Log("¡Tiempo completado!");

        // Solo transiciona automáticamente si el flag está activo.
        // En el flujo normal el participante interactúa con el robot
        // (ejercicio de respiración u otro), y la transición se dispara
        // al final de ese flujo llamando a IrAEscenaSiguiente() desde un botón.
        if (transicionAutomaticaAlExpirar
            && !transicionLanzada
            && !string.IsNullOrEmpty(escenaSiguiente))
        {
            transicionLanzada = true;
            Invoke(nameof(IrAEscenaSiguiente), Mathf.Max(0f, delayAntesDeTransicion));
        }
    }

    /// <summary>
    /// Carga la escena configurada en 'escenaSiguiente'.
    /// Pública para poder conectarla desde un botón del Inspector (OnClick),
    /// desde el final del ejercicio de respiración, o desde cualquier otro
    /// punto del flujo del robot.
    /// </summary>
    public void IrAEscenaSiguiente()
    {
        if (transicionLanzada) return; // Evitar dobles transiciones
        transicionLanzada = true;

        if (string.IsNullOrEmpty(escenaSiguiente))
        {
            Debug.LogWarning("[Temporizador] 'escenaSiguiente' está vacío. No se hace transición.");
            return;
        }

        // EmotionDataManager hace auto-guardado a JSON al descargar la escena.
        Debug.Log($"[Temporizador] Cargando escena siguiente: '{escenaSiguiente}'");
        SceneManager.LoadScene(escenaSiguiente);
    }

    public bool getTemporizadorActivo()
    {
        return temporizadorActivo;
    }
    
    public float getTiempoRestante()
    {
        return tiempoRestante;
    }
}