using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameUIController : MonoBehaviour
{
    public static GameUIController instance;

    [Header("Turn UI")]
    public TMP_Text turnText;
    public float turnTextDuration = 1.5f;

    [Header("End Game UI")]
    public GameObject endGamePanel;
    public TMP_Text resultText;

    void Awake()
    {
        instance = this;
        endGamePanel.SetActive(false);
        turnText.text = "";
    }

    public void ShowTurnText(string text)
    {
        StopAllCoroutines();
        StartCoroutine(ShowTurnRoutine(text));
    }

    IEnumerator ShowTurnRoutine(string text)
    {
        turnText.text = text;
        turnText.gameObject.SetActive(true);

        yield return new WaitForSeconds(turnTextDuration);

        turnText.gameObject.SetActive(false);
    }

    public void ShowWin()
    {
        endGamePanel.SetActive(true);
        resultText.text = "YOU WIN!";
    }

    public void ShowLoss()
    {
        endGamePanel.SetActive(true);
        resultText.text = "YOU LOSE!";
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

}

