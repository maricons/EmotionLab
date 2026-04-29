using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interrupciones : MonoBehaviour
{
    public Temporizador temporizador;
    public GameObject[] particulas;
    [Header("Prefabs que se apagarán")]
    public GameObject[] prefabs;
    [Header("Control de Música")]
    public AudioSource musicaDeFondo;
    public AudioSource musicaDeInterrupcion;
    [Header("UI")]
    public GameObject textoNotificacion;

    [Header("Configuración interrupción")]
    [Tooltip("Tiempo restante (en segundos) en el que se dispara la interrupción del generador.")]
    public float umbralInterrupcion = 110f;

    [Tooltip("Volumen de la música de fondo cuando NO hay interrupción (volumen 'normal').")]
    [Range(0f, 1f)] public float volumenFondoNormal = 0.1f;

    [Tooltip("Volumen de la música de interrupción cuando está activa.")]
    [Range(0f, 1f)] public float volumenInterrupcionActiva = 0.8f;

    private float tiempoActual;
    private bool interrupcionGenerador = false;

    // Start is called before the first frame update
    void Start()
    {
        // Música de interrupción: detenida por completo hasta que se necesite.
        // (Antes se ponía con volumen 0 pero seguía reproduciéndose, lo que
        //  causaba que se escuchara al entrar a la escena en Oficina.)
        if (musicaDeInterrupcion != null)
        {
            musicaDeInterrupcion.volume = 0f;
            musicaDeInterrupcion.Stop();
        }

        // Música de fondo: garantizamos que esté sonando al volumen normal.
        if (musicaDeFondo != null)
        {
            musicaDeFondo.volume = volumenFondoNormal;
            if (!musicaDeFondo.isPlaying) musicaDeFondo.Play();
        }

        // Nos aseguramos de que el texto de notificación esté oculto
        if (textoNotificacion != null)
        {
            textoNotificacion.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (temporizador == null) return;

        // No disparar la interrupción hasta que el participante haya pulsado
        // "Comenzar" y el temporizador esté efectivamente corriendo.
        // (Esto evita que en Oficina se rompa el generador antes de empezar
        //  cuando tiempoInicial es <= umbralInterrupcion.)
        if (!temporizador.getTemporizadorActivo()) return;

        tiempoActual = temporizador.getTiempoRestante();

        if (!interrupcionGenerador && tiempoActual <= umbralInterrupcion)
        {
            InterrumpirGenerador();
            interrupcionGenerador = true;
        }
    }

    public void InterrumpirGenerador()
    {
        if (prefabs != null)
        {
            foreach (GameObject prefab in prefabs)
            {
                if (prefab != null) prefab.SetActive(false);
            }
        }

        if (particulas != null)
        {
            foreach (GameObject particula in particulas)
            {
                if (particula != null) particula.SetActive(true);
            }
        }
        // Mostrar el texto de notificación
        if (textoNotificacion != null)
        {
            textoNotificacion.SetActive(true);
        }

        // Bajar la música de fondo y subir la de interrupción
        if (musicaDeFondo != null)
        {
            musicaDeFondo.volume = 0f;
        }

        if (musicaDeInterrupcion != null)
        {
            musicaDeInterrupcion.volume = volumenInterrupcionActiva;
            if (!musicaDeInterrupcion.isPlaying) musicaDeInterrupcion.Play();
        }
    }
    // Necesitarás llamar a esta función cuando el evento TERMINE
    public void TerminarInterrupcionGenerador()
    {
        // Revertir todo
        if (prefabs != null)
        {
            foreach (GameObject prefab in prefabs)
            {
                if (prefab != null) prefab.SetActive(true);
            }
        }
        if (particulas != null)
        {
            foreach (GameObject particula in particulas)
            {
                if (particula != null) particula.SetActive(false);
            }
        }
        // Ocultar el texto de notificación
        if (textoNotificacion != null)
        {
            textoNotificacion.SetActive(false);
        }
        // Bajar la música de interrupción y subir la de fondo
        if (musicaDeFondo != null)
        {
            musicaDeFondo.volume = volumenFondoNormal; // Vuelve al volumen normal
        }

        if (musicaDeInterrupcion != null)
        {
            musicaDeInterrupcion.volume = 0f;
            musicaDeInterrupcion.Stop(); // Detener para no consumir recursos
        }
    }
    // Esta función PÚBLICA solo oculta el texto de notificación
    public void OcultarNotificacion()
    {
        if (textoNotificacion != null)
        {
            textoNotificacion.SetActive(false);
        }
    }
    // Esta función PÚBLICA solo muestra el texto de notificación
    public void MostrarNotificacion()
    {
        if (textoNotificacion != null)
        {
            textoNotificacion.SetActive(true);
        }
    }  
}
