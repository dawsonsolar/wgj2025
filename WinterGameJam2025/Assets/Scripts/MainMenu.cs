using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Taken Mostly from https://www.youtube.com/watch?v=eki-6QBtDAg 
public class MainMenu : MonoBehaviour
{
    public string firstLevel;
    public GameObject optionsScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartGame()
    {
        SceneManager.LoadScene(firstLevel);
    }
    public void OpenOptions()
    {
        optionsScreen.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsScreen.SetActive(false);
    }

    public void QuitButton()
    {
        Application.Quit();
    }
}
