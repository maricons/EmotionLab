using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class signAnimated : MonoBehaviour
{
    // Start is called before the first frame update
    public float Timer = 5f;
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        Timer -= Time.deltaTime;
        if (Timer <= 0)
        {
           gameObject.SetActive(false);
        }
    }
}
