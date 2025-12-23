using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TurnManager : MonoBehaviour
{
    public List<PlayerFlinger2D> teamPlayers;
    public List<PlayerFlinger2D> teamEnemy;
    public static TurnManager instance;

    private int currentTeamIndex = 1;

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
        List<PlayerFlinger2D> currentTeam = GetCurrentTeam();
        foreach (var penguin in currentTeam)
        {
            penguin.penguinHasMoved = false;
            penguin.isActiveTurn = true;
        }

        if (currentTeamIndex == 1)
        {
           //TODO: Enemy AI
        }

        
    }

    public void EndTurn()
    {
        Debug.Log("End Turn For Team " + currentTeamIndex);
        foreach (var penguin in GetCurrentTeam())
        {
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
        foreach (var penguin in GetCurrentTeam())
        {
            penguin.penguinHasMoved = true;
            penguin.isActiveTurn = false;
        }
        EndTurn();
    }
}

