using UnityEngine;

/// <summary>
/// Clase base para los diálogos del proyecto. Contiene la lógica común de
/// navegación entre paneles (textPanels[]) que antes estaba duplicada en
/// DialogoBienvenida, DialogoPreparacion y DialogoTips.
///
/// Cada subclase extiende y añade lo específico (acción al terminar la
/// secuencia, lógica de Update, comportamientos extra de skip, etc.)
/// mediante los hooks virtuales <see cref="OnSequenceFinished"/> y
/// <see cref="OnSkipFinished"/>.
///
/// Las subclases mantienen sus nombres originales para no romper las
/// referencias serializadas en las escenas.
/// </summary>
public abstract class DialogoBase : MonoBehaviour
{
    [Header("Paneles de texto")]
    [Tooltip("Paneles secuenciales del diálogo. Se muestra uno a la vez.")]
    public GameObject[] textPanels;

    /// <summary>Índice del panel actualmente activo. Las subclases pueden leerlo.</summary>
    protected int currentPanelIndex = 0;

    // ======================================================================
    // API pública que se conecta desde los botones del Inspector (OnClick)
    // ======================================================================

    /// <summary>Avanza al siguiente panel. Si era el último, dispara OnSequenceFinished.</summary>
    public virtual void OnNextButton()
    {
        HideCurrentPanel();
        currentPanelIndex++;

        if (HasPanelAt(currentPanelIndex))
        {
            ShowCurrentPanel();
        }
        else
        {
            OnSequenceFinished();
        }
    }

    /// <summary>
    /// Cierra todos los paneles y dispara OnSkipFinished (que por defecto
    /// llama a OnSequenceFinished).
    /// </summary>
    public virtual void OnSkipIntroButton()
    {
        HideAllPanels();
        OnSkipFinished();
    }

    /// <summary>Salta a un panel específico. Útil para diálogos no lineales (DialogoTips).</summary>
    public virtual void GoToPanel(int panelIndex)
    {
        if (textPanels == null || textPanels.Length == 0) return;

        HideCurrentPanel();
        currentPanelIndex = Mathf.Clamp(panelIndex, 0, textPanels.Length - 1);
        ShowCurrentPanel();
    }

    // ======================================================================
    // Hooks virtuales para subclases
    // ======================================================================

    /// <summary>Se llama cuando la secuencia termina (último panel + Next).</summary>
    protected virtual void OnSequenceFinished() { }

    /// <summary>
    /// Se llama tras pulsar Skip. Por defecto delega en OnSequenceFinished
    /// porque saltar suele equivaler a "terminar la secuencia".
    /// Las subclases pueden overridearlo si Skip no debe disparar nada.
    /// </summary>
    protected virtual void OnSkipFinished() { OnSequenceFinished(); }

    // ======================================================================
    // Helpers protegidos (con bounds checks + null checks)
    // ======================================================================

    protected bool HasPanelAt(int index)
    {
        return textPanels != null
            && index >= 0
            && index < textPanels.Length
            && textPanels[index] != null;
    }

    protected void ShowCurrentPanel()
    {
        if (HasPanelAt(currentPanelIndex))
            textPanels[currentPanelIndex].SetActive(true);
    }

    protected void HideCurrentPanel()
    {
        if (HasPanelAt(currentPanelIndex))
            textPanels[currentPanelIndex].SetActive(false);
    }

    protected void HideAllPanels()
    {
        if (textPanels == null) return;
        foreach (GameObject panel in textPanels)
        {
            if (panel != null) panel.SetActive(false);
        }
    }
}
