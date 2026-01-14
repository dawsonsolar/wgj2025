using UnityEngine;
using UnityEngine.SceneManagement;

public class Button : MonoBehaviour
{
    public void QuitToTitle()
    {

        if (!string.IsNullOrEmpty("TitleScreen"))
            SceneManager.LoadScene("TitleScreen");
        else
            Application.Quit();
    }
}
