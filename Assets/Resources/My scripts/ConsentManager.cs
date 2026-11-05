using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Maneja la pantalla de consentimiento informado antes de iniciar la experiencia VR.
/// Adjunta este script al objeto "ConsentManager" en la escena ConsentScene.
/// </summary>
public class ConsentManager : MonoBehaviour
{
    [Header("Configuración de UI")]
    [Tooltip("El panel principal que contiene todo el UI del consentimiento")]
    public GameObject consentPanel;

    [Tooltip("Toggle/Checkbox que el usuario debe marcar para aceptar")]
    public Toggle acceptToggle;

    [Tooltip("Botón de 'Siguiente' (empieza deshabilitado)")]
    public Button nextButton;

    [Tooltip("Nombre exacto de la escena a cargar al aceptar")]
    public string waitingRoomSceneName = "WaitingRoom";

    [Header("Transición")]
    [Tooltip("Duración del fade out en segundos")]
    public float fadeDuration = 1.5f;

    [Tooltip("Canvas Group del panel para hacer fade (añade un CanvasGroup al consentPanel)")]
    public CanvasGroup panelCanvasGroup;

    [Header("Feedback Visual del Botón")]
    [Tooltip("Color del botón cuando está deshabilitado")]
    public Color buttonDisabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    [Tooltip("Color del botón cuando está habilitado")]
    public Color buttonEnabledColor = new Color(0.2f, 0.6f, 1f, 1f);

    private bool isTransitioning = false;

    void Start()
    {
        // Asegurarse de que el botón empiece deshabilitado
        SetNextButtonState(false);

        // Suscribir el evento del toggle
        if (acceptToggle != null)
            acceptToggle.onValueChanged.AddListener(OnToggleChanged);

        // Suscribir el evento del botón
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClicked);

        // Asegurarse de que el panel esté visible al inicio
        if (panelCanvasGroup != null)
            panelCanvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Se llama automáticamente cuando el usuario marca/desmarca el checkbox.
    /// </summary>
    void OnToggleChanged(bool isAccepted)
    {
        SetNextButtonState(isAccepted);

        // Una vez marcado, bloquear el checkbox para que quede "confirmado".
        // Esto evita que un click accidental (por ejemplo, el rayo del control
        // temblando al apuntar y presionar "Comenzar") lo desmarque sin que el
        // usuario lo haya tocado a propósito.
        if (isAccepted && acceptToggle != null)
            acceptToggle.interactable = false;
    }

    /// <summary>
    /// Habilita o deshabilita el botón Siguiente con feedback visual.
    /// </summary>
    void SetNextButtonState(bool enabled)
    {
        if (nextButton == null) return;

        nextButton.interactable = enabled;

        // Cambiar color del botón para dar feedback visual claro
        ColorBlock colors = nextButton.colors;
        colors.normalColor = enabled ? buttonEnabledColor : buttonDisabledColor;
        colors.disabledColor = buttonDisabledColor;
        nextButton.colors = colors;
    }

    /// <summary>
    /// Se llama al presionar el botón "Siguiente".
    /// </summary>
    void OnNextButtonClicked()
    {
        if (isTransitioning) return;
        if (acceptToggle == null || !acceptToggle.isOn) return;

        StartCoroutine(TransitionToWaitingRoom());
    }

    /// <summary>
    /// Hace fade out del panel y luego carga la escena WaitingRoom.
    /// </summary>
    IEnumerator TransitionToWaitingRoom()
    {
        isTransitioning = true;

        // Deshabilitar interacción durante la transición
        if (acceptToggle != null) acceptToggle.interactable = false;
        if (nextButton != null) nextButton.interactable = false;

        // Fade out del panel
        if (panelCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                panelCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
            panelCanvasGroup.alpha = 0f;
        }
        else
        {
            // Si no hay CanvasGroup, esperar un momento igual
            yield return new WaitForSeconds(fadeDuration);
        }

        // Cargar la escena de sala de espera
        SceneManager.LoadScene(waitingRoomSceneName);
    }

    void OnDestroy()
    {
        // Limpiar listeners para evitar memory leaks
        if (acceptToggle != null)
            acceptToggle.onValueChanged.RemoveListener(OnToggleChanged);
        if (nextButton != null)
            nextButton.onClick.RemoveListener(OnNextButtonClicked);
    }
}