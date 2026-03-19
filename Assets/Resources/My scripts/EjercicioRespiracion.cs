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

    [Header("UI opcional")] //Texto Inhala, Exhala
    public TextMeshProUGUI instructionText;
    //public GameObject panelText;

        void Start()
    {
         baseScale = balloonObject.transform.localScale;

        // Ocultar todo al iniciar
        balloonObject.SetActive(false);
        //panelText.SetActive(false);

        if (instructionText != null)
            instructionText.gameObject.SetActive(false);

        /*if (panelText!= null)
            panelText.gameObject.SetActive(false);*/
    }


    public void StartExercise()
    {
        if (!isRunning)
        {
            StartCoroutine(BreathingRoutine());
        }
    }
    
    private IEnumerator BreathingRoutine()
    {
        isRunning = true;
        balloonObject.SetActive(true);
        //panelText.SetActive(true);
        if (instructionText != null) instructionText.gameObject.SetActive(true);
        //if (panelText != null) panelText.gameObject.SetActive(true);

        for (int i = 0; i < cycles; i++)
        {
            // Inhalar
            if (instructionText != null) instructionText.text = "Inhala en 4s...";
            yield return StartCoroutine(ScaleBalloon(Vector3.one * scaleMultiplier, inhaleDuration));

            // Sostener
            if (instructionText != null) instructionText.text = "Sostén en 7s...";
            yield return new WaitForSeconds(holdDuration);

            // Exhalar
            if (instructionText != null) instructionText.text = "Exhala en 8s...";
            yield return StartCoroutine(ScaleBalloon(baseScale, exhaleDuration));
        }
        
        // Final del ejercicio
        if (instructionText != null) instructionText.text = "Excelente trabajo!";
        yield return new WaitForSeconds(1.5f);

        balloonObject.SetActive(false);
        //panelText.SetActive(false);
        if (instructionText != null) instructionText.gameObject.SetActive(false);
        //if (panelText != null) panelText.gameObject.SetActive(false);
        isRunning = false;

        // Al terminar el ejercicio, decirle al script de diálogo que avance
        dialogoTips.GoToPanel(nextPanelIndexAfterExercise);
    }

    private IEnumerator ScaleBalloon(Vector3 targetScale, float duration)
    {
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
