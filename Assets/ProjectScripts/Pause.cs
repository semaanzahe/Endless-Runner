using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject ScorePanel;
    public GameObject Controls;
    public GameObject SwipeManager;


    public Slider Slider;
    
    public void pauseGame()
    {
        Time.timeScale = 0;
        pauseMenu.SetActive(true);
        ScorePanel.SetActive(false);
        Controls.SetActive(false);
        SwipeManager.SetActive(false);
    }


   

    public void resumeGame()
    {
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
        ScorePanel.SetActive(true);
        SwipeManager.SetActive(true);
        Controls.SetActive(true);
        
    }
}
