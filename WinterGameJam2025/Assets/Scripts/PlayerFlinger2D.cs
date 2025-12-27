using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerFlinger2D : MonoBehaviour
{
    public float velocityMultiplier;
    public float maxVelocity;
    public float lineLengthMultiplier = 0.25f;



    public GameObject player;
    private Rigidbody2D rb;
    private LineRenderer line;

    private bool isSelected;
    private bool isDragging;

    public bool penguinHasMoved = false;
    public bool isActiveTurn = false;

    private Stats stats;

    public enum Team
    {
        Player,
        Enemy
    }

    public Team team;


    void Awake()
    {   
        rb = GetComponent<Rigidbody2D>();
        line = GetComponent<LineRenderer>();
        stats = GetComponent<Stats>();

        line.enabled = false;

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    public void BeginDrag()
    {
        if (!isActiveTurn || penguinHasMoved || rb.linearVelocity.magnitude > 0.05f)
        {
            return;
        }

        isDragging = true;
        
    }

    public void Release()
    {
        if (!isSelected || !isDragging || !isActiveTurn || penguinHasMoved)
            return;

        isDragging = false;
        line.enabled = false;
        rb.linearVelocity = GetLaunchVelocity();
        penguinHasMoved = true;
        StartCoroutine(WaitForStopThenEndTurn());

    }

    void Update()
    {
        if (!isDragging || !isSelected)
            return;

        DrawLine();
    }

    void FixedUpdate()
    {
        if (!isDragging && rb.linearVelocity.sqrMagnitude < 0.01f)
            rb.linearVelocity = Vector2.zero;
    }

    Vector2 GetLaunchVelocity()
    {
        Vector2 mouseWorld =
            Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        Vector2 dir = (Vector2)transform.position - mouseWorld;
        Vector2 velocity = dir * velocityMultiplier;
        return Vector2.ClampMagnitude(velocity, maxVelocity);
    }

    // Prediction Line
    void DrawLine()
    {
        Vector2 velocity = GetLaunchVelocity();
        Vector2 start = rb.position;
        Vector2 end = start + velocity * lineLengthMultiplier;

        line.enabled = true;
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    public void Select() => isSelected = true;

    public void Deselect()
    {
        isSelected = false;
        isDragging = false;
        line.enabled = false;
    }

    bool ImmuneThisTurn()
    {
        if (TurnManager.instance == null)
            return false;
        if (team == Team.Player && TurnManager.instance.CurrentTeamIndex == 0)
        {
            Debug.Log($"{team} is immune this turn!");
            return true;  
        }
            
        if (team == Team.Enemy && TurnManager.instance.CurrentTeamIndex == 1)
        {
            Debug.Log($"{team} is immune this turn!");
            return true;
        }
        return false;
    }

    //TAKING DAMAGE
    void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerFlinger2D other = collision.gameObject.GetComponent<PlayerFlinger2D>();
        if (other == null)
            return;
        if (other.team == team) 
            return;
        // only apply damage if moving quick enough, adjust number if too high/low
        if (rb.linearVelocity.magnitude < 0.5f)
            return;

        if (other.ImmuneThisTurn())
            return;

        Stats otherStats = other.GetComponent<Stats>();
        if ((otherStats == null || stats == null))
            return;

        otherStats.TakeDamage(stats.damage);
    }

    IEnumerator WaitForStopThenEndTurn()
    {
        // Wait until physics settles
        yield return new WaitUntil(() => rb.linearVelocity.sqrMagnitude < 0.01f);

        rb.linearVelocity = Vector2.zero;

        TurnManager.instance.CheckTurn(this);
    }

}
