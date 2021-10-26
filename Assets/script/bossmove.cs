using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bossmove : MonoBehaviour
{
    float timeCounter =0;
    float speed;
    float width;
    float height;

    // Start is called before the first frame update
    void Start()
    {
        speed = 3;
        width = 1;
        height =1;    
    }

    // Update is called once per frame
    void Update()
    {
        timeCounter += Time.deltaTime*speed;
        float x = Mathf.Cos (timeCounter)*width;
        float y = Mathf.Sin (timeCounter)*height;
        float z = 0;
        transform.position = new Vector3(x+5,y+2,z);
    }
}
