using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class FuncionalidadPato : MonoBehaviour
{
    public float audioCoolDown = 1f;
    public AudioClip sonidoPato1;
    public AudioClip sonidoPato2;
    public AudioClip sonidoPato3;
    public AudioClip sonidoPato4;


    private float Timer = 0;
    private int lastPlayed = 0;
    private UnityEngine.Vector3 posIni;
    private AudioSource aSource;

    // Start is called before the first frame update
    void Start()
    {
        posIni = gameObject.transform.position;
        aSource = gameObject.GetComponent<AudioSource>();
        //Timer = 0f;
        //print(posIni);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) {
            gameObject.transform.position = posIni;
        }
        
        if (Timer>0)
        {
            Timer -= Time.deltaTime;
        }
    }

    AudioClip sonidoAleatorio()
    {
        int num = 0;
        while (num == lastPlayed)
        {
            num = UnityEngine.Random.Range(0,4);
        }

        lastPlayed = num;
        


        if (num == 0)
        {
            return sonidoPato1;
        } else if (num == 1)
        {
            return sonidoPato2;
        } else if (num == 2)
        {
            return sonidoPato3;
        } else if (num == 3)
        {
            return sonidoPato4;
        }

        return sonidoPato1;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (Timer<=0)
        {
            aSource.PlayOneShot(sonidoAleatorio());
            Timer = audioCoolDown;
        }
        
    }
}
