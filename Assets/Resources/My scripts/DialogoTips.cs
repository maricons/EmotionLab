using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogoTips : MonoBehaviour
{
    public GameObject[] textPanels; // Paneles con cuadros de texto
    private int currentPanelIndex = 0; //Empieza con el primer cuadro de texto
    private bool flag = true; //true muestra paneles, false no muestra paneles

    [Tooltip("Índice del panel donde 'Skip Tips' debe saltar (panel que comienza la presentación).")]
    public int skipTipsTargetIndex = 6;

    void Update() // En cada frame ve si es true
    {
        if (flag)
        {
            if (textPanels != null && currentPanelIndex >= 0 && currentPanelIndex < textPanels.Length
                && textPanels[currentPanelIndex] != null)
            {
                textPanels[currentPanelIndex].SetActive(true);
            }
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

    public void OnSkipTipsButton()
    {
        if (textPanels == null || textPanels.Length == 0)
        {
            Debug.LogWarning("[DialogoTips] textPanels vacío en OnSkipTipsButton.");
            return;
        }

        if (currentPanelIndex >= 0 && currentPanelIndex < textPanels.Length
            && textPanels[currentPanelIndex] != null)
        {
            textPanels[currentPanelIndex].SetActive(false);
        }

        // Clamp al rango válido por si el array tiene menos paneles de los esperados
        currentPanelIndex = Mathf.Clamp(skipTipsTargetIndex, 0, textPanels.Length - 1);

        if (textPanels[currentPanelIndex] != null)
            textPanels[currentPanelIndex].SetActive(true);
    }

    public void OnExitButton()
    {
        //Desactivar todos los paneles
        if (textPanels != null)
        {
            foreach (GameObject panel in textPanels)
            {
                if (panel != null) panel.SetActive(false);
            }
        }
        flag = false; // Cambiar el estado del flag para evitar que se muestre el primer panel de nuevo
    }

    public void GoToPanel(int panelIndex)
    {
        if (textPanels == null || textPanels.Length == 0) return;

        // Desactivar panel actual
        if (currentPanelIndex >= 0 && currentPanelIndex < textPanels.Length
            && textPanels[currentPanelIndex] != null)
        {
            textPanels[currentPanelIndex].SetActive(false);
        }

        // Cambiar al nuevo panel (clamp por seguridad)
        currentPanelIndex = Mathf.Clamp(panelIndex, 0, textPanels.Length - 1);

        // Activar el panel indicado
        if (textPanels[currentPanelIndex] != null)
            textPanels[currentPanelIndex].SetActive(true);
    }

}
