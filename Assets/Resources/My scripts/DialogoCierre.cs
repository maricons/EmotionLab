using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Diálogo de cierre para que conteste las ultimas preguntas
///
/// La lógica de navegación (textPanels, currentPanelIndex, OnNextButton,
/// OnSkipIntroButton) vive en <see cref="DialogoBase"/>.
/// </summary>
public class DialogoCierre : DialogoBase
{
    [Header("Referencias del cierre")]
    [Tooltip("Objeto Clipboard")]
    public GameObject clipboard;
    public Button btnVamosAlCierre;
    public string escenaSiguiente = "Cierre";

    [Header("Puente Fácil → Difícil")]
    [Tooltip("Escena del modo Difícil a la que ir cuando el modo recién completado fue 'facil'. " +
             "Si el modo recién completado fue 'dificil', se ignora y se usa 'escenaSiguiente'.")]
    public string escenaDificil = "Salón";
    [Tooltip("Panel con el mensaje de Labbu explicando que sigue el modo Difícil, con un botón que " +
             "llama a OnContinuarADificil(). Se muestra en vez de navegar cuando el modo recién " +
             "completado fue 'facil'.")]
    public GameObject panelPuenteDificil;

    private void Start()
    {
        InputSystem.Update();

        if (clipboard != null)
            clipboard.SetActive(false);

        if (btnVamosAlCierre != null)
            btnVamosAlCierre.interactable = false;

        currentPanelIndex = 0;
        HideAllPanels();

        if (panelPuenteDificil != null)
            panelPuenteDificil.SetActive(false);

        // El panel 0 ("Capa Bienvenida") queda oculto a propósito: lo revela
        // EjercicioRespiracion al terminar (dialogoTips.GoToPanel(0)), disparado
        // por el botón "inicio_Button" de esta escena, no automáticamente aquí.
    }

    ///<summary>
    /// Sobreescribimos el avance de diálogo para mostrar el clipboard al llegar al último panel, y habilitar el botón para avanzar a la escena de cierre.
    /// </summary>

    public override void OnNextButton()
    {
        //Antes de avanzar al siguiente panel, verificamos si estamos en el último panel.
        if (currentPanelIndex == 2)
        {
            // Si es el último panel, mostramos el clipboard y habilitamos el botón para avanzar a la escena de cierre.
            if (clipboard != null)
                clipboard.SetActive(true);
        }
        else
        {
            // Si no es el último panel, avanzamos normalmente.
            base.OnNextButton();
        }
    }

    ///<summary>
    /// Funcion que llamara el formulario al responder la última pregunta.
    ///</summary>
    public void OnFormularioFinalizado()
    {
        //1. Desaparece el clipboard
        if (clipboard != null)
            clipboard.SetActive(false);

        //2. Se habilita boton "Vamos al cierre"
        if (btnVamosAlCierre != null)
            btnVamosAlCierre.interactable = true;
    }

    /// <summary>
    /// Se ejecuta cuando se pulsa el botón "Vamos al cierre" (ya que es el último panel de la secuencia).
    /// </summary>
    
    protected override void OnSequenceFinished()
    {
        string modo = EmotionDataManager.Instance?.UltimoModoCompletado;

        // Si el modo recién completado fue Fácil, en vez de navegar directo a
        // Cierre mostramos el puente hacia el modo Difícil.
        if (modo == "facil" && panelPuenteDificil != null)
        {
            HideAllPanels();
            panelPuenteDificil.SetActive(true);
            return;
        }

        // Modo Difícil recién completado (o no hay puente configurado): cerrar normalmente.
        if (!string.IsNullOrEmpty(escenaSiguiente))
            SceneManager.LoadScene(escenaSiguiente);
        else
            Debug.LogWarning("[DialogoCierre] Escena siguiente no asignada en el Inspector.");
    }

    /// <summary>
    /// Botón del panel puente hacia el modo Difícil (Salón).
    /// </summary>
    public void OnContinuarADificil()
    {
        if (!string.IsNullOrEmpty(escenaDificil))
            SceneManager.LoadScene(escenaDificil);
        else
            Debug.LogWarning("[DialogoCierre] 'escenaDificil' no asignada en el Inspector.");
    }
}
