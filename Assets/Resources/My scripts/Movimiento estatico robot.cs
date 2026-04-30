using UnityEngine;

/// <summary>
/// Hace que el robot levite suavemente sobre el eje Y.
/// PENSADO PARA APLICARSE AL MISMO GameObject que tiene RobotMover
/// (típicamente Robot_Guardian / Labby), donde está la malla visible.
///
/// Usa LateUpdate para correr DESPUÉS de RobotMover (que escribe en Update),
/// así el bobbing siempre tiene la última palabra sobre la Y. No toca X/Z
/// (los maneja RobotMover) ni la rotación (la maneja RobotMover con LookAt).
/// </summary>
public class RobotVolador : MonoBehaviour
{
    [Header("Levitación vertical")]
    [Tooltip("Altura máxima del balanceo arriba/abajo, en unidades.")]
    public float amplitud = 0.15f;

    [Tooltip("Velocidad del ciclo de levitación.")]
    public float velocidad = 1.5f;

    [Header("Inclinación opcional")]
    [Tooltip("Grados máximos de inclinación oscilante en X. 0 = sin inclinación.")]
    public float inclinacionGrados = 0f;

    [Tooltip("Velocidad de la oscilación de inclinación (relativa a 'velocidad').")]
    public float velocidadInclinacion = 0.5f;

    private float baseY;
    private float offsetAleatorio;

    void Start()
    {
        // Y mundial inicial alrededor de la cual oscila el robot.
        baseY = transform.position.y;

        // Variación entre robots para que no leviten en sincro.
        offsetAleatorio = Random.Range(0f, 2f * Mathf.PI);
    }

    void LateUpdate()
    {
        // Aplicar la levitación DESPUÉS de RobotMover (que escribe en Update).
        // Solo tocamos Y; X y Z los gestiona RobotMover.
        float deltaY = Mathf.Sin((Time.time + offsetAleatorio) * velocidad) * amplitud;

        Vector3 pos = transform.position;
        pos.y = baseY + deltaY;
        transform.position = pos;

        // Inclinación suave aditiva sobre la rotación que ya escribió RobotMover.
        if (inclinacionGrados > 0f)
        {
            float inclinacion = Mathf.Sin(Time.time * velocidad * velocidadInclinacion) * inclinacionGrados;
            transform.rotation = transform.rotation * Quaternion.Euler(inclinacion, 0f, 0f);
        }
    }
}
