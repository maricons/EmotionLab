using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class FormularioCierre : MonoBehaviour
{
    [Header("Preguntas del formulario")]
    [Tooltip("Evento que se dispara al responder todas las preguntas")]
    public UnityEvent onFormularioCompletado;
    
    //Guardamos las preguntas de forma interna para no arrastrar nada
    private PreguntasFormulario[] preguntasEncontradas;
    private bool formularioYaFinalizado = false;

    private void Start()
    {
        // El script busca automáticamente todas las preguntas que estén dentro del Clipboard
        preguntasEncontradas = GetComponentsInChildren<PreguntasFormulario>(true);

        if (preguntasEncontradas.Length == 0)
        {
            Debug.LogWarning("[FormularioCierreManager] ¡Ojo! No se encontró ningún script 'PreguntasFormulario' hijo de este objeto.");
            return;
        }

        // Nos suscribimos a los botones de todas las preguntas detectadas
        foreach (PreguntasFormulario pregunta in preguntasEncontradas)
        {
            if (pregunta == null) continue;

            for (int i = 0; i < pregunta.optionButtons.Length; i++)
            {
                if (pregunta.optionButtons[i] == null) continue;
                int indexOpcion = i;// Copia local para usar dentro del listener
                PreguntasFormulario preguntaActual = pregunta;
                // Escuchamos los clics de las caritas en tiempo real
                pregunta.optionButtons[i].onClick.AddListener(VerificarFormulario);
            }
        }
    }
    /// <summary>
    /// Se ejecuta cada vez que el estudiante hace clic en una respuesta del formulario de cierre.
    /// </summary>
    private void OnOpcionSeleccionada(PreguntasFormulario pregunta, int indexOpcion)
    {
        // 1. Registramos o actualizamos la respuesta en el EmotionDataManager
        if (EmotionDataManager.Instance != null && pregunta != null)
        {
            string nombreEscena = SceneManager.GetActiveScene().name;
            ///string idPregunta = !string.IsNullOrEmpty(pregunta.questionId) ? pregunta.questionId : pregunta.gameObject.name;
            ///string textoPregunta = pregunta.questionText != null ? pregunta.questionText.text : "Pregunta_Cierre";
            ///string labelRespuesta = $"opcion_{indexOpcion + 1}";
            
            // Nombre de la escena activa sin requerir namespaces externos
            ///string nombreEscena = gameObject.scene.name;
            // ID de la pregunta: usamos el nombre del GameObject si no hay campo específico disponible
            string idPregunta = pregunta.gameObject.name;
            // Nombre descriptivo de la pregunta tomando el nombre del GameObject de la pregunta
            string textoPregunta = pregunta.gameObject.name;
            string labelRespuesta = $"opcion_{indexOpcion + 1}";

            EmotionDataManager.Instance.RegistrarOActualizarRespuesta(
                nombreEscena,
                idPregunta,
                textoPregunta,
                labelRespuesta,
                indexOpcion
            );
        }

        // 2. Comprobamos si con este clic ya se completó todo el formulario
        VerificarFormulario();
    }

    /// <summary>
    /// Comprueba si absolutamente todas las preguntas del portapapeles tienen una respuesta seleccionada.
    /// </summary>
    public void VerificarFormulario()
    {
        if (formularioYaFinalizado) return;

        // Validamos una por una
        foreach (PreguntasFormulario pregunta in preguntasEncontradas)
        {
            if (pregunta == null) continue;

            if (!pregunta.HasAnswer())
            {
                // Si falta aunque sea una sola carita por marcar, frena la verificación
                return; 
            }
        }

        // Si el código pasa el bucle, significa que completaron todo el formulario
        formularioYaFinalizado = true;

        // Finalizamos la sesión, guardamos el JSON acumulado y lo subimos a la nube
        if (EmotionDataManager.Instance != null)
        {
            EmotionDataManager.Instance.FinalizarSesion();
            EmotionDataManager.Instance.GuardarAJson();
        }

        // Avisamos al DialogoCierre para que apague el portapapeles y libere el botón
        if (onFormularioCompletado != null)
        {
            onFormularioCompletado.Invoke();
        }
    }
    
}