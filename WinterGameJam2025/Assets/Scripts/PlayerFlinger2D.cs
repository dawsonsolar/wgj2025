using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerFlinger2D : MonoBehaviour
{
    public float velocityMultiplier;
    public float maxVelocity;
    public float lineLengthMultiplier = 0.25f;
    public float damage;
    public float maxHealth;
    public float health;

    public GameObject player;
    private Rigidbody2D rb;
    private LineRenderer line;

    private bool isSelected;
    private bool isDragging;

    public bool penguinHasMoved = false;
    public bool isActiveTurn = false;


    void Awake()
    {
        SetStats();
        rb = GetComponent<Rigidbody2D>();
        line = GetComponent<LineRenderer>();
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
        TurnManager.instance.CheckTurn(this);

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

    public void SetStats()
    {
        velocityMultiplier = player.GetComponent<Stats>().velocityMultiplier;
        maxVelocity = player.GetComponent<Stats>().maxVelocity;
        maxHealth = player.GetComponent<Stats>().maxHealth;
        health = maxHealth;
        damage = player.GetComponent<Stats>().damage;
    }
}
