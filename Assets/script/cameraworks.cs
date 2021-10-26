using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraworks : MonoBehaviour
{
    public Vector3 targetPosition;
    float Speed = 3f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if(transform.position.y <=4){
            moveUp();
        }
    }
    void moveUp(){
        transform.Translate(Vector3.up * Speed * Time.deltaTime);
    }
}
