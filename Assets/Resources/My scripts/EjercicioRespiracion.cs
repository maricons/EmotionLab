using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EjercicioRespiracion : MonoBehaviour
{
    public GameObject balloonObject;        // 🎈 El globo o balón
    public DialogoTips dialogoTips;         // 🔗 Referencia al script de diálogos
    public int nextPanelIndexAfterExercise; // Panel al que debe ir después del ejercicio

    public int cycles = 3;                  // Número de respiraciones
    public float inhaleDuration = 4f;       // Duración inhalar
    public float holdDuration = 7f;         // Duración sostener
    public float exhaleDuration = 8f;       // Duración exhalar
    public float scaleMultiplier = 1f;    // Tamaño máximo del globo al inhalar
    private bool isRunning = false;
    private Vector3 baseScale;

    // Referencia a la cuenta atrás en curso. Permite detenerla antes de
    // iniciar la siguiente fase y evitar cuentas regresivas en paralelo.
    private Coroutine cuentaAtrasCoroutine;

    [Header("UI opcional")] //Texto Inhala, Exhala
    public TextMeshProUGUI instructionText;
    //public GameObject panelText;

        void Start()
    {
        if (balloonObject != null)
        {
            baseScale = balloonObject.transform.localScale;
            balloonObject.SetActive(false);
        }

        if (instructionText != null)
            instructionText.gameObject.SetActive(false);
    }


    public void StartExercise()
    {
        if (!isRunning)
        {
            StartCoroutine(BreathingRoutine());
        }
    }

    private IEnumerator MostrarCuentaAtras(string accion, float duracion)
    {
        int tiempo = Mathf.CeilToInt(duracion);

        while (tiempo > 0)
        {
            if (instructionText != null)
                instructionText.text = accion + "... " + tiempo;

            yield return new WaitForSeconds(1f);
            tiempo--;
        }
    }

    /// <summary>
    /// Lanza la cuenta atrás de una fase. Si había una cuenta atrás de la
    /// fase anterior aún corriendo, la detiene primero para evitar que
    /// dos textos se pisen en pantalla.
    /// </summary>
    private void IniciarCuentaAtras(string accion, float duracion)
    {
        if (cuentaAtrasCoroutine != null)
        {
            StopCoroutine(cuentaAtrasCoroutine);
            cuentaAtrasCoroutine = null;
        }
        cuentaAtrasCoroutine = StartCoroutine(MostrarCuentaAtras(accion, duracion));
    }

    private IEnumerator BreathingRoutine()
    {
        isRunning = true;

        if (balloonObject != null) balloonObject.SetActive(true);
        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(true);
            instructionText.text = "¡Comencemos!";
        }

        yield return new WaitForSeconds(2f);

        for (int i = 0; i < cycles; i++)
        {
            // Inhalar
            IniciarCuentaAtras("Inhala", inhaleDuration);
            yield return StartCoroutine(ScaleBalloon(Vector3.one * scaleMultiplier, inhaleDuration));

            // Sostener
            IniciarCuentaAtras("Sostén", holdDuration);
            yield return new WaitForSeconds(holdDuration);

            // Exhalar
            IniciarCuentaAtras("Exhala", exhaleDuration);
            yield return StartCoroutine(ScaleBalloon(baseScale, exhaleDuration));
        }

        // Asegurar que ninguna cuenta atrás quede colgada al terminar
        if (cuentaAtrasCoroutine != null)
        {
            StopCoroutine(cuentaAtrasCoroutine);
            cuentaAtrasCoroutine = null;
        }

        // Final del ejercicio
        if (instructionText != null) instructionText.text = "Excelente trabajo!";
        yield return new WaitForSeconds(1.5f);

        if (balloonObject != null) balloonObject.SetActive(false);
        if (instructionText != null) instructionText.gameObject.SetActive(false);
        isRunning = false;

        // Al terminar el ejercicio, decirle al script de diálogo que avance
        if (dialogoTips != null)
            dialogoTips.GoToPanel(nextPanelIndexAfterExercise);
        else
            Debug.LogWarning("[EjercicioRespiracion] dialogoTips no asignado.");
    }

    private IEnumerator ScaleBalloon(Vector3 targetScale, float duration)
    {
        if (balloonObject == null) yield break;

        Vector3 initialScale = balloonObject.transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            balloonObject.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            yield return null;
        }
    }
}
