using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PreguntasPizarra : MonoBehaviour
{
    public Button[] optionButtons;

    private int selectedIndex = -1;
    public Color selectedColor;
    public Color defaultColor = Color.white;

    [Header("Highlight de ayuda")]
    [Tooltip("Ciclos de parpadeo que hace ResaltarOpciones() para ayudar al usuario a encontrar este tablero.")]
    public int ciclosResaltado = 3;
    [Tooltip("Duración de cada mitad de ciclo (encendido/apagado) del parpadeo, en segundos.")]
    public float duracionMedioCiclo = 0.25f;

    private Coroutine resaltadoCoroutine;

    public void SelectOption(int index)
    {
        selectedIndex = index;
        AplicarColorSegunSeleccion();
    }

    /// <summary>
    /// Hace parpadear las opciones de este tablero unos ciclos para llamar la
    /// atención del usuario (ej. al presionar "Iniciar" sin haber seleccionado
    /// nada en este tablero).
    /// </summary>
    public void ResaltarOpciones()
    {
        if (resaltadoCoroutine != null)
            StopCoroutine(resaltadoCoroutine);
        resaltadoCoroutine = StartCoroutine(PulsarOpciones());
    }

    private IEnumerator PulsarOpciones()
    {
        for (int ciclo = 0; ciclo < ciclosResaltado; ciclo++)
        {
            SetColorTodos(selectedColor);
            yield return new WaitForSeconds(duracionMedioCiclo);
            SetColorTodos(defaultColor);
            yield return new WaitForSeconds(duracionMedioCiclo);
        }

        AplicarColorSegunSeleccion();
        resaltadoCoroutine = null;
    }

    private void AplicarColorSegunSeleccion()
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            var image = optionButtons[i].GetComponent<Image>();
            if (image != null)
            {
                image.color = (i == selectedIndex) ? selectedColor : defaultColor;
            }
        }
    }

    private void SetColorTodos(Color color)
    {
        foreach (var btn in optionButtons)
        {
            var image = btn.GetComponent<Image>();
            if (image != null) image.color = color;
        }
    }
}
