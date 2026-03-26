using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scriptContadorDucks : MonoBehaviour
{
    public int cDucks = 0;
    // Start is called before the first frame update
    private GameObject gDuck;
    public GameObject giantDuck;
    public GameObject mensajeFinal;
    void Start()
    {
        gDuck = GameObject.Find("GiantDuck");

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
        }
    }

}
