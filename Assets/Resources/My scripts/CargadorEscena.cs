using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Componente simple para cargar una escena desde un botÃ³n del Inspector (OnClick).
/// Pensado para el botÃ³n 'Comenzar' que aparece junto al robot al expirar el
/// temporizador en SalÃ³n / Oficina, para pasar a la escena de Cierre.
///
/// Uso:
///   1. AÃ±adir este componente al GameObject del botÃ³n.
///   2. Configurar 'nombreEscena' en el Inspector (ej: "Cierre"). Debe estar
///      aÃ±adida en File > Build Settings.
///   3. En el botÃ³n -> OnClick() -> arrastrar el GameObject -> seleccionar
///      CargadorEscena.CargarEscena().
/// </summary>
public class CargadorEscena : MonoBehaviour
{
    [Tooltip("Nombre exacto de la escena a cargar. Debe estar aÃ±adida en File > Build Settings.")]
    public string nombreEscena = "Cierre";

    [Tooltip("Si no está vacío ('facil' | 'dificil'), al presionar este botón marca ese modo como " +
             "completado en EmotionDataManager antes de cargar la escena. Dejar vacío si este botón " +
             "no representa el fin de un modo Fácil/Difícil.")]
    public string modoCompletadoAlFinalizar = "";

    /// <summary>
    /// Carga la escena configurada en 'nombreEscena'.
    /// </summary>
    public void CargarEscena()
    {
        if (!string.IsNullOrEmpty(modoCompletadoAlFinalizar))
            EmotionDataManager.Instance?.MarcarModoCompletado(modoCompletadoAlFinalizar);

        if (string.IsNullOrEmpty(nombreEscena))
        {
            Debug.LogWarning("[CargadorEscena] 'nombreEscena' estÃ¡ vacÃ­o. No se hace transiciÃ³n.");
            return;
        }

        // Marcar modo completado si venimos de Oficina o Salón
        string escenaActual = SceneManager.GetActiveScene().name;
        if (escenaActual.IndexOf("Oficina", System.StringComparison.OrdinalIgnoreCase) >= 0)
            EmotionDataManager.Instance?.MarcarModoCompletado("facil");
        else if (escenaActual.IndexOf("Sal", System.StringComparison.OrdinalIgnoreCase) >= 0)
            EmotionDataManager.Instance?.MarcarModoCompletado("dificil");

        // EmotionDataManager hace auto-guardado a JSON al descargar la escena.
        Debug.Log($"[CargadorEscena] Cargando escena: '{nombreEscena}'");
        SceneManager.LoadScene(nombreEscena);
    }
}
