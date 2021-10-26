using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lifetime : MonoBehaviour
{
    public GameObject obj;
    public float time = 3.0f;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(obj, 6);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
