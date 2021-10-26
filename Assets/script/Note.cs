using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class Note : MonoBehaviour
{
    public float speed = 1f;
    public bool canBePressed;
    public KeyCode keyToPress;
    public GameObject notehit_effect;

    public GameObject realposition;

    public GameObject hp;
    private Shake shake;
    // Start is called before the first frame update
    void Start()
    {
        shake = GameObject.FindGameObjectWithTag("ScreenShake").GetComponent<Shake>();
    }

    // Update is called once per frame
    void Update()
    {

        this.transform.Translate(-speed * Time.deltaTime * 8, 0, 0);

         if (Input.touchCount>0) {
            if(!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                if(canBePressed)
                {
                    Instantiate(notehit_effect, realposition.transform.position , Quaternion.identity);
                    //gameObject.SetActive(false);
                    shake.CamShake();
                    Destroy(gameObject);
                    Destroy(notehit_effect);
                }
            }
        }
        if(Input.GetMouseButtonDown(0)) 
        {
            if(!EventSystem.current.IsPointerOverGameObject())
            {  
                if(canBePressed)
                {
                    notehit_effect = (GameObject) Instantiate(notehit_effect, realposition.transform.position , Quaternion.identity);
                    //gameObject.SetActive(false);
                    shake.CamShake();
                    Destroy(gameObject);                    
                }
            }
            
        }
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Activator"))
        {
            canBePressed = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Activator"))
        {
            canBePressed = false;
        }
    }


    
    
}
