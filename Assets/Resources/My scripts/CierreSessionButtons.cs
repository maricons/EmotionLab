using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Maneja los 3 botones de la escena Cierre: "Volver a Empezar" (mismo SessionID,
/// solo puede usarse una vez por sesión), "Nueva Experiencia" (SessionID nuevo) y
/// "Salir" (cierra la aplicación).
/// </summary>
public class CierreSessionButtons : MonoBehaviour
{
    [Tooltip("GameObject del botón 'Volver a Empezar'. Se oculta si ya se usó una vez en esta sesión.")]
    public GameObject botonVolverAEmpezar;

    [Tooltip("Nombre exacto de la escena Waiting Room. Debe estar añadida en File > Build Settings.")]
    public string escenaWaitingRoom = "Waiting Room";

    private void Start()
    {
        if (botonVolverAEmpezar != null && EmotionDataManager.Instance != null && EmotionDataManager.Instance.RetryUsado)
        {
            botonVolverAEmpezar.SetActive(false);
        }
    }

    /// <summary>Botón "Volver a Empezar": misma sesión, mismo SessionID. Solo una vez por sesión.</summary>
    public void OnVolverAEmpezar()
    {
        EmotionDataManager.Instance?.MarcarRetryUsado();
        SceneManager.LoadScene(escenaWaitingRoom);
    }

    /// <summary>Botón "Nueva Experiencia": genera un SessionID nuevo y reinicia el estado de completado.</summary>
    public void OnNuevaExperiencia()
    {
        EmotionDataManager.Instance?.IniciarNuevaSesion();
        SceneManager.LoadScene(escenaWaitingRoom);
    }

    /// <summary>Botón "Salir": cierra la aplicación.</summary>
    public void OnSalir()
    {
        // TODO(equipo de datos / Sprint 4 - LocalStorageWriter): antes de cerrar,
        // guardar/exportar el resumen final de la sesión (CSV agregado, KPIs
        // calculados). EmotionDataManager ya autoguarda el JSON crudo en
        // OnApplicationQuit(), pero el resumen agregado queda pendiente.
        Application.Quit();
    }
}
