using UnityEngine;

/// <summary>
/// Diálogo de bienvenida del Waiting Room. Al terminar la secuencia
/// (último panel + Next, o Skip), arranca al robot.
///
/// La lógica de navegación (textPanels, currentPanelIndex, OnNextButton,
/// OnSkipIntroButton) vive en <see cref="DialogoBase"/>.
/// </summary>
public class DialogoBienvenida : DialogoBase
{
    [Header("Acción al terminar la secuencia")]
    [Tooltip("Robot que empezará a moverse cuando termine la intro. Asignar desde el Inspector.")]
    public RobotMover robotMover;

    protected override void OnSequenceFinished()
    {
        if (robotMover != null)
            robotMover.StartMoving();
        else
            Debug.LogWarning("[DialogoBienvenida] robotMover no asignado en el Inspector.");
    }
}
