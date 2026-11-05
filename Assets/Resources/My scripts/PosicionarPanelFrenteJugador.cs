using System.Collections;
using UnityEngine;

/// <summary>
/// Mantiene el panel de consentimiento (Canvas) siempre frente al jugador,
/// siguiendo hacia dónde mira la cámara VR en todo momento (no solo al
/// iniciar la escena).
///
/// Por qué hace falta: la posición del Canvas en la escena es fija en el
/// mundo (m_LocalPosition), pero hacia dónde mira el jugador depende de la
/// orientación real calibrada del dispositivo (Guardian / recentrado), que
/// puede cambiar en cualquier momento. Este script recalcula posición y
/// rotación del panel en cada frame en base a la cámara real, con una
/// transición suave (Lerp/Slerp) para evitar saltos bruscos que puedan
/// resultar incómodos en VR.
///
/// Adjuntar este script al GameObject "Canvas" de la escena Consentimiento.
/// </summary>
public class PosicionarPanelFrenteJugador : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Cámara del jugador (Main Camera del XR Origin). Si se deja vacío, se usa Camera.main")]
    [SerializeField] private Transform camaraJugador;

    [Header("Configuración")]
    [Tooltip("Distancia (en metros) a la que aparecerá el panel frente al jugador")]
    [SerializeField] private float distancia = 3f;

    [Tooltip("Altura fija (en metros, eje Y global) a la que se ubica el panel")]
    [SerializeField] private float altura = 1.5f;

    [Tooltip("Velocidad con la que el panel sigue la mirada. Mayor valor = sigue más rápido/rígido, menor valor = más suave")]
    [SerializeField] private float velocidadSeguimiento = 3f;

    private Transform camara;

    IEnumerator Start()
    {
        // Esperar un frame para asegurarse de que la pose de la cámara XR
        // ya esté aplicada (en el primer frame puede no estar lista todavía).
        yield return null;

        camara = camaraJugador != null
            ? camaraJugador
            : (Camera.main != null ? Camera.main.transform : null);

        if (camara == null)
        {
            Debug.LogWarning("[PosicionarPanelFrenteJugador] No se encontró la cámara del jugador.");
            enabled = false;
            yield break;
        }

        // Ubicar el panel de inmediato la primera vez, sin animación de por medio.
        Vector3 posicionInicial = CalcularPosicionObjetivo(out Quaternion rotacionInicial);
        transform.SetPositionAndRotation(posicionInicial, rotacionInicial);
    }

    void LateUpdate()
    {
        if (camara == null) return;

        Vector3 posicionObjetivo = CalcularPosicionObjetivo(out Quaternion rotacionObjetivo);

        transform.position = Vector3.Lerp(transform.position, posicionObjetivo, Time.deltaTime * velocidadSeguimiento);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, Time.deltaTime * velocidadSeguimiento);
    }

    /// <summary>
    /// Calcula dónde debería estar el panel: frente a la cámara, a la
    /// distancia y altura configuradas, mirando hacia el jugador.
    /// </summary>
    private Vector3 CalcularPosicionObjetivo(out Quaternion rotacion)
    {
        // Dirección horizontal hacia donde mira el jugador (ignorando la
        // inclinación de la cabeza hacia arriba/abajo).
        Vector3 direccion = camara.forward;
        direccion.y = 0f;
        if (direccion.sqrMagnitude < 0.0001f)
            direccion = camara.up;
        direccion.Normalize();

        Vector3 posicion = camara.position + direccion * distancia;
        posicion.y = altura;

        rotacion = Quaternion.LookRotation(direccion, Vector3.up);
        return posicion;
    }
}
