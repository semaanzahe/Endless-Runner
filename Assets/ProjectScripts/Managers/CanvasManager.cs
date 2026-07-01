using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager instance;
    
    private GameObject PauseMenu;
    private GameObject Hud;
    private GameObject DeathScreen;
    private GameObject Controls;
    private GameObject SwipeManager;

    private bool swipe = true;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    { 
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        GetReferences();
       PauseMenu.SetActive(false);
       Hud.SetActive(true);
       DeathScreen.SetActive(false);
       Controls.SetActive(false);
       SwipeManager.SetActive(true);
    }

    public void Pause()
    {
        Time.timeScale = 0;
        PauseMenu.SetActive(true);
        Hud.SetActive(false);
        Controls.SetActive(false);
        SwipeManager.SetActive(false);
    }
    public void Resume()
    {
        Time.timeScale = 1;
        PauseMenu.SetActive(false);
        Hud.SetActive(true);
        if (swipe)
        {
            SwipeManager.SetActive(true);
        }
        else
        {
           Controls.SetActive(true); 
        }
        
    }

    public void Death()
    {
        Time.timeScale = 0;
        PauseMenu.SetActive(false);
        Hud.SetActive(false);
        DeathScreen.SetActive(true);
        Controls.SetActive(false);
    }
    
    void GetReferences()
    {
        GameObject PauseMenuObject = GameObject.FindGameObjectWithTag("PauseMenu");
        if (PauseMenuObject != null)
            PauseMenu = PauseMenuObject;
        GameObject HudObject = GameObject.FindGameObjectWithTag("Hud");
        if (HudObject != null)
            Hud = HudObject;
        GameObject DeathScreenObject = GameObject.FindGameObjectWithTag("DeathScreen");
        if (DeathScreenObject != null)
            DeathScreen = DeathScreenObject;
        GameObject ControlsObject = GameObject.FindGameObjectWithTag("Controls");
        if (ControlsObject != null)
            Controls = ControlsObject;
        GameObject SwipeObject = GameObject.FindGameObjectWithTag("Swipe");
        if (SwipeObject != null)
            SwipeManager = SwipeObject;
        
            
    }
    
    public void ToggleControl()
    {
        if (swipe)
        {
            swipe = false;
        }
        else
        {
            swipe = true;
        }
    }
}
