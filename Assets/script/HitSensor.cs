using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitSensor : MonoBehaviour
{
    public bool ishit;

    public GameObject[] Hearts;
    public int numofHeart;
    // Start is called before the first frame update
    void Start()
    {
        ishit = false;
        numofHeart=5;
    }

    // Update is called once per frame
    void Update()
    {
        if(ishit == true){
            Hearts[numofHeart].SetActive(false);
            numofHeart -= numofHeart;
            Hearts[numofHeart].SetActive(true);
            ishit=false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other){
        if(other.tag == "Slimejob"){
            ishit = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other){
        if(other.tag == "Slimejob"){
            ishit = false;
        }
    }
}
