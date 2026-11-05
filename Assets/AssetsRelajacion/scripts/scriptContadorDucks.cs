using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class scriptContadorDucks : MonoBehaviour
{
    public int cDucks = 0;
    // Start is called before the first frame update
    private GameObject gDuck;
    public GameObject giantDuck;
    public GameObject mensajeFinal;

    [Header("UI Contador")]
    [Tooltip("Texto (TMP) que muestra 'X/8 Patos'. Se actualiza automáticamente.")]
    public TextMeshProUGUI contadorTexto;

    void Start()
    {
        gDuck = GameObject.Find("GiantDuck");
        ActualizarContadorUI();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.name == "Duck")
        {
            cDucks += 1;
            print(cDucks);
            ActualizarContadorUI();
        }
        if (cDucks == 8)
        {
            giantDuck.SetActive(true);
            mensajeFinal.SetActive(true);
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (other.name == "Duck")
        {
            cDucks -= 1;
            print(cDucks);
            ActualizarContadorUI();
        }
    }

    /// <summary>
    /// Actualiza el texto del contador con el formato "X/8 Patos".
    /// </summary>
    void ActualizarContadorUI()
    {
        if (contadorTexto != null)
            contadorTexto.text = $"{cDucks}/8 Patos";
    }

}
