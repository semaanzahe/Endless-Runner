using UnityEngine;
using UnityEngine.SceneManagement;

public class restart : MonoBehaviour
{

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        AudioManager.Instance.ResumeMusic();
        Time.timeScale = 1;
    }
    
}
