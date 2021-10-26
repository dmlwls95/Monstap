using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class health : MonoBehaviour
{
    public int numOfHealth;

    public GameObject gameOver;
    public GameObject tryagain;
    public GameObject[] hearts;
    public Animator playerAnim;
    public GameObject MusicPlayer;
    public GameObject jobevent;
    // Start is called before the first frame update
    void Start()
    {
        numOfHealth=5;
    }

    // Update is called once per frame
    void Update()
    {
        for(int i =0; i< hearts.Length; i++){
            if(i == numOfHealth){
                hearts[i].SetActive(true);
            }
            else{
                hearts[i].SetActive(false);
            }
        }
        if(numOfHealth <= 0){
                playerAnim.SetTrigger("die");
                gameOver.SetActive(true);
                tryagain.SetActive(true);
                MusicPlayer.SetActive(false);
                jobevent.SetActive(false);
                return;
        }
    }

    private void OnTriggerEnter2D(Collider2D other){
        if(other.gameObject.tag.Equals("Slimejob")){
            numOfHealth = numOfHealth - 1;
            Destroy(other.gameObject);
        }
        if(other.gameObject.tag.Equals("poison")){
            numOfHealth = numOfHealth - 2;
            Destroy(other.gameObject);
        }
        
    }
}
