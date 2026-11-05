using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    public void IrACierre(string Cierre)
    {
        SceneManager.LoadScene("Cierre");
    }

    public void IrARetroalimentación(string Retroalimentacion)
    {
        // Este botón ("si_Button" en Salón) es la salida real del modo Difícil.
        // Sin esto, DialogoCierre nunca ve "dificil" en UltimoModoCompletado y
        // el Panel Puente Dificil se vuelve a mostrar en cada vuelta (bucle).
        EmotionDataManager.Instance?.MarcarModoCompletado("dificil");
        SceneManager.LoadScene("Retroalimentacion");
    }

    public void IrAWaitingRoom(string WaitingRoom)
    {
        SceneManager.LoadScene("WaitingRoom");
    }

}