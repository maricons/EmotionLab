using UnityEngine;
using UnityEngine.UI;

public class DialogoBienvenida : MonoBehaviour
{
    public GameObject[] textPanels; // Paneles con cuadros de texto
    private int currentPanelIndex = 0;

    public RobotMover robotMover; // Asignar desde el editor

    public void OnNextButton()
    {
        // Validar índice antes de desactivar el panel actual
        if (textPanels != null && currentPanelIndex >= 0 && currentPanelIndex < textPanels.Length
            && textPanels[currentPanelIndex] != null)
        {
            textPanels[currentPanelIndex].SetActive(false);
        }

        currentPanelIndex++;

        if (textPanels != null && currentPanelIndex < textPanels.Length)
        {
            if (textPanels[currentPanelIndex] != null)
                textPanels[currentPanelIndex].SetActive(true);
        }
        else
        {
            // Fin de la intro, mover robot
            if (robotMover != null)
                robotMover.StartMoving();
            else
                Debug.LogWarning("[DialogoBienvenida] robotMover no asignado en el Inspector.");
        }
    }

    public void OnSkipIntroButton()
    {
        if (textPanels != null)
        {
            foreach (GameObject panel in textPanels)
            {
                if (panel != null) panel.SetActive(false);
            }
        }

        if (robotMover != null)
            robotMover.StartMoving();
        else
            Debug.LogWarning("[DialogoBienvenida] robotMover no asignado en el Inspector.");
    }
}
