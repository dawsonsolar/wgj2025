using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerFlinger2D : MonoBehaviour
{
    public float velocityMultiplier = 5f;
    public float maxVelocity = 12f;
    public float lineLengthMultiplier = 0.25f;

    private Rigidbody2D rb;
    private LineRenderer line;

    private bool isSelected;
    private bool isDragging;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        line = GetComponent<LineRenderer>();
        line.enabled = false;

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    public void BeginDrag()
    {
        if (rb.linearVelocity.magnitude > 0.05f)
            return;

        isDragging = true;
    }

    public void Release()
    {
        if (!isSelected || !isDragging)
            return;

        isDragging = false;
        line.enabled = false;
        rb.linearVelocity = GetLaunchVelocity();
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
}
