using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scene = Unity.VectorGraphics.Scene;

public class Start_Menu : MonoBehaviour
{
    [SerializeField] private GameObject SaveList;

    private void Start()
    {
        if (SaveList != null)
        {
            SaveList.SetActive(false);
        }
        
    }

    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OpenSaveList()
    {
        if (SaveList != null)
        {
            SaveList.SetActive(true);
        }
    }
    public void CloseSaveList()
    {
        if (SaveList != null)
        {
            SaveList.SetActive(false);
        }
    }
}
