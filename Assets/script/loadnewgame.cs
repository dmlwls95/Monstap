using UnityEngine;
using UnityEngine.SceneManagement;

public class loadnewgame : MonoBehaviour
{
   public void LoadScene()
    { 
        Fader.Instance.FadeIn().LoadLevel(1).FadeOut(2);
    }
}
