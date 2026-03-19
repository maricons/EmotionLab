using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptDucks : MonoBehaviour
{
    
    //private int cDucks = 0;
    private GameObject sDuck;
    // Start is called before the first frame update
    void Start()
    {
        sDuck = GameObject.Find("SampleDuck");
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject newDuck = Instantiate(sDuck);
            newDuck.GetComponent<Rigidbody>().useGravity = true;
            newDuck.transform.Translate(new (0.0f,-0.5f,0.0f));
            //cDucks += 1;
            //print(cDucks);
        }
        
    }
}
