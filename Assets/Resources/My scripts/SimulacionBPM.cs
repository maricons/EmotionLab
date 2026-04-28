using System.Collections;
using TMPro;
using UnityEngine;

public class SimulacionBPM : MonoBehaviour
{
    public TMP_Text displayText;
    public Temporizador temporizador;
    private bool flag = false;
    private Coroutine bpmCoroutine;

    void Update()
    {
        if (temporizador == null) return;

        if (!flag && temporizador.getTemporizadorActivo())
        {
            flag = true;
            bpmCoroutine = StartCoroutine(ActualizarBPM());
        }
    }

    private IEnumerator ActualizarBPM()
    {
        while (flag)
        {
            int bpm = Random.Range(60, 180);
            if (displayText != null)
                displayText.text = bpm.ToString();
            yield return new WaitForSeconds(0.5f); // Proporciona un intervalo de actualización
        }
    }
}
