using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class endNote : MonoBehaviour
{
    public float speed = 1f;
    public bool canBePressed;
    public KeyCode keyToPress;
    public GameObject notehit_effect;

    public float realspeed;
    public GameObject realposition;
    
    public GameObject hp;
    private Shake shake;

    private GameObject wayPoint;
    private Vector3 wayPointPos;
    public GameObject MusicPlayer;
    public GameObject jobevent;
    public GameObject stageclear;
    public GameObject tosong;
    // Start is called before the first frame update
    void Start()
    {
        wayPoint = GameObject.Find("wayPoint");
        shake = GameObject.FindGameObjectWithTag("ScreenShake").GetComponent<Shake>();
    }

    // Update is called once per frame
    void Update()
    {
      wayPointPos = new Vector3(wayPoint.transform.position.x, wayPoint.transform.position.y, wayPoint.transform.position.z);
      //Here, the zombie's will follow the waypoint.
      transform.position = Vector3.MoveTowards(transform.position, wayPointPos, speed * Time.deltaTime * realspeed);
        //this.transform.Translate(-speed * Time.deltaTime * 8, 0, 0);

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
                    stageclear.SetActive(true);
                    tosong.SetActive(true);
                    MusicPlayer.SetActive(false);
                    jobevent.SetActive(false);
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
