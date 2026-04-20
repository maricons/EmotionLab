using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Maneja los botones de emociones de una pregunta del formulario.
/// Al seleccionar una opción, si está activo el auto-guardado,
/// registra la respuesta en EmotionDataManager (que persiste entre escenas
/// y guarda a JSON en disco).
/// </summary>
public class PreguntasFormulario : MonoBehaviour
{
    [Header("Identificación de la pregunta (para el JSON)")]
    [Tooltip("Texto de la pregunta tal cual aparece en el formulario. Ej: '¿Cómo te sientes hoy?'")]
    public string textoPregunta = "";

    [Tooltip("ID corto de la pregunta (sin espacios). Ej: 'sentir_hoy', 'nerviosismo', 'preocupacion'")]
    public string idPregunta = "";

    [Tooltip("Contexto/escena para guardar con la respuesta. Ej: 'waiting_room_inicial', 'cierre_final'.")]
    public string contextoEscena = "waiting_room_inicial";

    [Header("Auto-guardado")]
    [Tooltip("Si está activo, cada vez que el jugador selecciona una opción se registra automáticamente en EmotionDataManager.")]
    public bool autoGuardarAlSeleccionar = true;

    [Tooltip("Si está activo, además escribe el JSON a disco cada vez que se selecciona (más seguro, más I/O).")]
    public bool escribirDiscoEnCadaSeleccion = false;

    [Header("Botones de emociones para esta pregunta")]
    public Button[] optionButtons;

    [Tooltip("Etiqueta de cada opción, en el mismo orden que optionButtons. Ej: '9', '8', '7' ... '1'.")]
    public string[] optionLabels;

    [Header("Sprite para marcar selección")]
    public Sprite selectedImagePrefab;

    private int selectedIndex = -1;

    void Start()
    {
        foreach (Button btn in optionButtons)
        {
            if (btn == null) continue;
            btn.onClick.RemoveAllListeners();
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] == null) continue;
            int index = i;
            optionButtons[i].onClick.AddListener(() => SelectOption(index));
        }
    }

    public void SelectOption(int index)
    {
        if (index < 0 || index >= optionButtons.Length) return;

        // Desactivar todas las marcas de selección
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] == null) continue;
            Transform selectedImage = optionButtons[i].transform.Find("SelectedImage");
            if (selectedImage != null)
                selectedImage.gameObject.SetActive(false);
        }

        selectedIndex = index;

        // Marcar el botón seleccionado
        Transform target = optionButtons[selectedIndex].transform.Find("SelectedImage");
        if (target != null)
            target.gameObject.SetActive(true);

        // Auto-guardado en EmotionDataManager
        if (autoGuardarAlSeleccionar)
            GuardarRespuestaEnManager();
    }

    public int GetSelectedIndex()
    {
        return selectedIndex;
    }

    /// <summary>
    /// Devuelve el nombre del GameObject del botón seleccionado (fallback si no hay labels).
    /// </summary>
    public string GetSelectedOptionName()
    {
        if (selectedIndex < 0 || selectedIndex >= optionButtons.Length) return null;
        if (optionButtons[selectedIndex] == null) return null;
        return optionButtons[selectedIndex].gameObject.name;
    }

    /// <summary>
    /// Devuelve la etiqueta semántica (optionLabels) del botón seleccionado.
    /// Si no hay etiqueta configurada, cae al nombre del GameObject.
    /// </summary>
    public string GetSelectedLabel()
    {
        if (selectedIndex < 0) return null;

        if (optionLabels != null
            && selectedIndex < optionLabels.Length
            && !string.IsNullOrEmpty(optionLabels[selectedIndex]))
        {
            return optionLabels[selectedIndex];
        }
        return GetSelectedOptionName();
    }

    public bool HasAnswer() => selectedIndex >= 0;

    private void GuardarRespuestaEnManager()
    {
        var manager = EmotionDataManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning($"[PreguntasFormulario] No hay EmotionDataManager en escena. " +
                             $"Respuesta '{idPregunta}' no registrada.");
            return;
        }

        if (string.IsNullOrEmpty(idPregunta))
        {
            Debug.LogWarning($"[PreguntasFormulario] '{gameObject.name}' no tiene 'idPregunta' configurado. No se guarda.");
            return;
        }

        manager.RegistrarOActualizarRespuesta(
            escena: contextoEscena,
            idPregunta: idPregunta,
            textoPregunta: textoPregunta,
            respuestaLabel: GetSelectedLabel(),
            respuestaIndex: selectedIndex
        );

        if (escribirDiscoEnCadaSeleccion)
            manager.GuardarAJson();
    }
}
