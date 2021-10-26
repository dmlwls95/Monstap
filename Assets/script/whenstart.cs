using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class whenstart : MonoBehaviour
{
    
    public Vector2 targetPosition;
    float Speed = 2f;
    // Start is called before the first frame update
    void Start()
    {
        
        targetPosition = new Vector2(0,0);
    }

    // Update is called once per frame
    void Update()
    {
        while(Vector2.Distance(transform.position, targetPosition)>0){
            transform.Translate(Vector2.up * Speed * Time.deltaTime);
        }
    }
}
