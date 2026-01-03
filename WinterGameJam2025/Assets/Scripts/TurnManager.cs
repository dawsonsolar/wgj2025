using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TurnManager : MonoBehaviour
{
    public List<PlayerFlinger2D> teamPlayers;
    public List<PlayerFlinger2D> teamEnemy;
    public static TurnManager instance;
    private Coroutine enemyTurnCoroutine;


    private int currentTeamIndex = 0;
    public int CurrentTeamIndex => currentTeamIndex;

    void Start()
    {
        StartTurn();
    }

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (currentTeamIndex != 1)
            return;     
        if (Keyboard.current == null)
            return;
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("Enemy Turn Forcefully ended!");
            ForceEnemyTurnEnd();
        }
    }

    public void CheckTurn(PlayerFlinger2D penguin)
    {
    penguin.penguinHasMoved = true;

    bool allPenguinMoved = true;

    foreach(var p in GetCurrentTeam())
        {
            if (p == null)
                continue;

           if (!p.penguinHasMoved)
           {
               allPenguinMoved = false;
               break;
           }

        }
        if (allPenguinMoved)
        {
            EndTurn();
        }

    }

    public void StartTurn()
    {
        Debug.Log("Start Turn For Team " + currentTeamIndex);

        if (GameUIController.instance != null)
        {
            if (currentTeamIndex == 0)
            {
                GameUIController.instance.ShowTurnText("Player's Turn!");
            }
            else
            {
                GameUIController.instance.ShowTurnText("Enemies Turn!");
            }
        }

        List<PlayerFlinger2D> currentTeam = GetCurrentTeam();

        foreach (var penguin in currentTeam)
        {
            if (penguin == null)
                continue;

            penguin.penguinHasMoved = false;
            penguin.isActiveTurn = true;
        }

        if (currentTeamIndex == 1)
        {
            enemyTurnCoroutine = StartCoroutine(EnemyTurnSequence());
        }
    }



    public void EndTurn()
    {
        Debug.Log("End Turn For Team " + currentTeamIndex);

        if (enemyTurnCoroutine != null)
        {
            StopCoroutine(enemyTurnCoroutine);
            enemyTurnCoroutine = null;
        }

        foreach (var penguin in GetCurrentTeam())
        {
            if (penguin != null)
                penguin.isActiveTurn = false;
        }

        currentTeamIndex = 1 - currentTeamIndex;
        StartTurn();
    }


    List<PlayerFlinger2D> GetCurrentTeam()
    {
        return currentTeamIndex == 0 ? teamPlayers : teamEnemy;
    }

    void ForceEnemyTurnEnd()
    {
        if (currentTeamIndex != 1)
            return;

        if (enemyTurnCoroutine != null)
        {
            StopCoroutine(enemyTurnCoroutine);
            enemyTurnCoroutine = null;
        }

        EndTurn();
    }


    public void OnPenguinDied(PlayerFlinger2D penguin)
    {
        Debug.Log("Removing Penguin from list: " + penguin.name);
        teamPlayers.Remove(penguin);
        teamEnemy.Remove(penguin);

        // if the penguin died before moving, mark it as moved.
        CheckForImmediateTurnEnd();

        if (teamPlayers.Count == 0)
        {
            Debug.Log("ENEMY WINS"); // future win/lose state here
            GameUIController.instance?.ShowLoss();
            enabled = false;
        }
        if (teamEnemy.Count == 0)
        {
            Debug.Log("PLAYER WINS");
            GameUIController.instance?.ShowWin();
            enabled = false;
        }
    }

    void CheckForImmediateTurnEnd()
    {
        List<PlayerFlinger2D> currentTeam = GetCurrentTeam();

        if (currentTeam.Count == 0)
        {
            EndTurn();
            return;
        }

        bool allMoved = true;

        foreach (var p in currentTeam)
        {
            if (p != null && !p.penguinHasMoved)
            {
                allMoved = false;
                break;
            }
        }

        if (allMoved)
        {
            EndTurn();
        }
    }

    IEnumerator EnemyTurnSequence()
    {
        foreach (var penguin in teamEnemy.ToArray()) // snapshot for safety
        {
            if (currentTeamIndex != 1)
                yield break;

            if (penguin == null)
                continue;

            EnemyAI ai = penguin.GetComponent<EnemyAI>();
            if (ai == null || ai.IsDead)
                continue;

            yield return StartCoroutine(ai.TakeTurn());

            yield return new WaitForSeconds(0.4f);
        }

        // SAFETY: ensure turn ends even if enemies died mid-turn
        if (currentTeamIndex == 1)
        {
            EndTurn();
        }
    }



}

