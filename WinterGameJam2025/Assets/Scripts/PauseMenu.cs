using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instance;

    public GameObject pausePanel;
    public string titleSceneName = "Title"; // optional

    public bool IsPaused { get; private set; }

    void Awake()
    {
        instance = this;
        pausePanel.SetActive(false);
    }

    void Update()
    {
        // Do not allow pausing after game has ended
        if (GameUIController.instance != null &&
            GameUIController.instance.endGamePanel.activeSelf)
        {
            return;
        }

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (IsPaused)
                Resume();
            else
                Pause();
        }
    }


    public void Pause()
    {
        IsPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        IsPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToTitle()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(titleSceneName))
            SceneManager.LoadScene(titleSceneName);
        else
            Application.Quit();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

