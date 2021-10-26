using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SonicBloom.Koreo;

public class EventSubscriber : MonoBehaviour
{
    public Transform obj;
    // Start is called before the first frame update
    void Start()
    {
        Koreographer.Instance.RegisterForEvents("TestEventID", FireEventDebugLog);
    }


    void FireEventDebugLog(KoreographyEvent koreoEvent)
    {
        //Debug.Log("Koreography Event Fired.");
        //Invoke("noteCreate",-3);
        Instantiate(obj, new Vector3(13, -1, 0), Quaternion.identity);
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}