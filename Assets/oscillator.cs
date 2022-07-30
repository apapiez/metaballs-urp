using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class oscillator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //oscilate the object
        transform.position = new Vector3(Mathf.Sin(Time.time * 2), 0, 0);
        



        
    }
}
