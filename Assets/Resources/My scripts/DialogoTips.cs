using UnityEngine;

/// <summary>
/// Diálogo de tips/consejos. Se diferencia de los demás en que:
/// - En cada frame mantiene visible el panel actual (mientras autoMostrar = true).
/// - Tiene un botón Skip Tips que salta a un índice específico (no al final).
/// - Tiene un botón Exit que cierra todo y desactiva el auto-mostrado.
///
/// La lógica de navegación (textPanels, currentPanelIndex, OnNextButton,
/// GoToPanel) vive en <see cref="DialogoBase"/>.
/// </summary>
public class DialogoTips : DialogoBase
{
    [Header("Comportamiento Tips")]
    [Tooltip("Mientras esté activo, el panel actual se mantiene visible cada frame. " +
             "OnExitButton lo desactiva.")]
    public bool autoMostrarPanelActual = true;

    [Tooltip("Índice del panel al que salta 'Skip Tips' (panel que comienza la presentación).")]
    public int skipTipsTargetIndex = 6;

    void Update()
    {
        if (autoMostrarPanelActual) ShowCurrentPanel();
    }

    /// <summary>Salta al panel configurado en skipTipsTargetIndex (con clamp).</summary>
    public void OnSkipTipsButton()
    {
        if (textPanels == null || textPanels.Length == 0)
        {
            Debug.LogWarning("[DialogoTips] textPanels vacío en OnSkipTipsButton.");
            return;
        }

        GoToPanel(skipTipsTargetIndex);
    }

    /// <summary>Cierra todos los paneles y detiene el auto-mostrado.</summary>
    public void OnExitButton()
    {
        HideAllPanels();
        autoMostrarPanelActual = false;
    }
}
