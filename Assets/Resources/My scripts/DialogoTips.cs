using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogoTips : MonoBehaviour
{
    public GameObject[] textPanels; // Paneles con cuadros de texto
    private int currentPanelIndex = 0; //Empieza con el primer cuadro de texto
    private bool flag = true; //true muestra paneles, false no muestra paneles

    void Update() // En cada frame ve si es true
    {
        if (flag)
        {
            textPanels[currentPanelIndex].SetActive(true);
        }
    }
    public void OnNextButton() 
    {
        textPanels[currentPanelIndex].SetActive(false);
        currentPanelIndex++;

        if (currentPanelIndex < textPanels.Length)
        {
            textPanels[currentPanelIndex].SetActive(true);
        }
    }

    public void OnSkipTipsButton()
    {
        textPanels[currentPanelIndex].SetActive(false);
        currentPanelIndex = 6; // Mostrar el panel que comienza la presentacion.
        textPanels[currentPanelIndex].SetActive(true); 

    }

    public void OnExitButton()
    {
        //Desactivar todos los paneles
        foreach (GameObject panel in textPanels)
        {
            panel.SetActive(false);
        }
        flag = false; // Cambiar el estado del flag para evitar que se muestre el primer panel de nuevo
    }

    public void GoToPanel(int panelIndex)
    {
        // Desactivar panel actual
        textPanels[currentPanelIndex].SetActive(false);

        // Cambiar al nuevo panel
        currentPanelIndex = panelIndex;

        // Activar el panel indicado
        if (currentPanelIndex < textPanels.Length)
        {
            textPanels[currentPanelIndex].SetActive(true);
        }
    }

}
