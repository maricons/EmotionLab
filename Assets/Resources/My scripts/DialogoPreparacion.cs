using UnityEngine;

/// <summary>
/// Diálogo que aparece cuando el robot llega cerca de un punto destino.
/// Espera en Update() a que el robot se acerque a 'targetPosition' y
/// entonces muestra el primer panel.
///
/// La lógica de navegación (textPanels, currentPanelIndex, OnNextButton,
/// OnSkipIntroButton) vive en <see cref="DialogoBase"/>.
/// </summary>
public class DialogoPreparacion : DialogoBase
{
    [Header("Disparador por proximidad del robot")]
    [Tooltip("Robot que debe acercarse a 'targetPosition' para activar el primer panel.")]
    public GameObject robot;

    [Tooltip("Posición destino. Cuando el robot está a <1 unidad, se muestra el panel inicial.")]
    public Transform targetPosition;

    [Tooltip("Distancia (en unidades) por debajo de la cual se considera que el robot 'llegó'.")]
    public float distanciaActivacion = 1f;

    private bool primerPanelMostrado = false;

    void Update()
    {
        if (primerPanelMostrado) return;
        if (robot == null || targetPosition == null) return;

        if (Vector3.Distance(robot.transform.position, targetPosition.position) < distanciaActivacion)
        {
            primerPanelMostrado = true;
            ShowCurrentPanel();
        }
    }

    /// <summary>
    /// Override: en este diálogo, Skip solo cierra paneles sin disparar
    /// nada al final (a diferencia de DialogoBienvenida).
    /// </summary>
    protected override void OnSkipFinished()
    {
        // Intencionalmente vacío: Skip aquí solo oculta los paneles,
        // no desencadena ninguna acción posterior.
    }
}
