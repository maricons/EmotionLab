using UnityEngine;
using UnityEngine.UI;

public class DialogoPreparacion : MonoBehaviour
{
    public GameObject[] textPanels; // Paneles con cuadros de texto
    private int currentPanelIndex = 0;

    public GameObject robot; // Asignar desde el editor

    private bool flag = false;
    public Transform targetPosition;  // Asigna la posición destino desde el editor


    void Update()
    {
        // Validar referencias antes de calcular distancia
        if (robot == null || targetPosition == null) return;

        //ver si el robot está cerca de la posicion deseada
        if (!flag && Vector3.Distance(robot.transform.position, targetPosition.position) < 1)
        {
            flag = true;
        }


        if (flag == true && currentPanelIndex == 0)
        {
            if (textPanels != null && currentPanelIndex < textPanels.Length
                && textPanels[currentPanelIndex] != null)
            {
                textPanels[currentPanelIndex].SetActive(true);
            }
            flag = false; // Resetear la bandera para que no se active de nuevo
        }
    }


    public void OnNextButton()
    {
        if (textPanels != null && currentPanelIndex >= 0 && currentPanelIndex < textPanels.Length
            && textPanels[currentPanelIndex] != null)
        {
            textPanels[currentPanelIndex].SetActive(false);
        }

        currentPanelIndex++;

        if (textPanels != null && currentPanelIndex < textPanels.Length
            && textPanels[currentPanelIndex] != null)
        {
            textPanels[currentPanelIndex].SetActive(true);
        }

    }

    public void OnSkipIntroButton()
    {
        if (textPanels == null) return;
        foreach (GameObject panel in textPanels)
        {
            if (panel != null) panel.SetActive(false);
        }

    }
}
