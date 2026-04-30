using UnityEngine;

public class RobotMover : MonoBehaviour
{
    [Header("Movimiento")]
    public Transform targetPosition;   // Posición final del robot
    public float moveSpeed = 2f;

    [Tooltip("Distancia bajo la cual se considera que el robot 'llegó' al target.")]
    public float distanciaLlegada = 0.5f;

    [Header("Rotación hacia el jugador")]
    [Tooltip("Asignar la cámara con head tracking del XR Origin: " +
             "XR Origin > Camera Offset > Main Camera.")]
    public Transform lookTarget;

    public float rotationSpeed = 2f;

    [Tooltip("Si está activo, el robot SIEMPRE mira al jugador (incluso al llegar al target). " +
             "Si está desactivado, solo mira mientras se desplaza.")]
    public bool mirarSiempre = true;

    [Header("Compensación del modelo (si el FBX no apunta a +Z)")]
    [Tooltip("Grados extra a sumar en Y a la rotación calculada por LookRotation. " +
             "Si Labby te da la espalda con el LookAt correcto, prueba 180. " +
             "Si te muestra un costado, prueba 90 o -90.")]
    public float offsetRotacionY = 0f;

    private bool shouldMove = false;

    void Update()
    {
        // ----- Movimiento -----
        if (shouldMove && targetPosition != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition.position,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPosition.position) < distanciaLlegada)
            {
                shouldMove = false;
            }
        }

        // ----- Rotación hacia el jugador -----
        // Fuera del if(shouldMove): así sigue mirando al jugador después de llegar.
        if (lookTarget != null && (shouldMove || mirarSiempre))
        {
            Vector3 direction = lookTarget.position - transform.position;
            direction.y = 0f; // ignorar diferencias de altura
            if (direction.sqrMagnitude < 0.0001f) return; // muy cerca, evitar NaN

            Quaternion lookRotation = Quaternion.LookRotation(direction.normalized)
                                      * Quaternion.Euler(0f, offsetRotacionY, 0f);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    public void StartMoving()
    {
        shouldMove = true;
    }
}
