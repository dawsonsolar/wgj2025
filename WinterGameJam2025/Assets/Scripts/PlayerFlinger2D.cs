using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerFlinger2D : MonoBehaviour
{
    public float velocityMultiplier;
    public float maxVelocity;
    public float lineLengthMultiplier = 0.25f;

    [Tooltip("Minimum world-units between penguin and cursor before a confirm click registers.")]
    public float minAimDistance = 0.8f;

    public GameObject player;

    public enum AimMode { Fling, Item }
    public AimMode CurrentAimMode { get; private set; } = AimMode.Fling;

    public bool HasItem => heldItemPrefab != null;
    public Sprite HeldItemIcon => heldItemIcon;

    private GameObject heldItemPrefab;
    private Sprite heldItemIcon;

    public bool penguinHasMoved = false;
    public bool isActiveTurn = false;

    private bool isSelected = false;
    private bool isAiming = false;

    public enum Team { Player, Enemy }
    public Team team;

    private Rigidbody2D rb;
    private LineRenderer line;
    private Stats stats;

    [Header("Item Indicator")]
    [Tooltip("SpriteRenderer on a child object positioned above the penguin. Shows the held item icon and dims when in fling mode.")]
    public SpriteRenderer itemIndicator;

    // Fully visible in item mode, dimmed in fling mode so player knows they have something but aren't using it yet
    private static readonly Color IndicatorItemMode = new Color(1f, 1f, 1f, 1f);
    private static readonly Color IndicatorFlingMode = new Color(1f, 1f, 1f, 0.35f);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        line = GetComponent<LineRenderer>();
        stats = GetComponent<Stats>();

        if (line != null) line.enabled = false;

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        RefreshIndicator();
    }

    void Update()
    {
        if (isAiming && isSelected)
            DrawAimLine();
    }

    void FixedUpdate()
    {
        if (rb.linearVelocity.magnitude > maxVelocity)
            rb.linearVelocity = rb.linearVelocity.normalized * maxVelocity;

        if (rb.linearVelocity.sqrMagnitude < 0.01f)
            rb.linearVelocity = Vector2.zero;
    }

    // Called by PlayerClickSelector2D on first click - shows the aim line
    public void StartAiming()
    {
        if (PauseMenu.instance != null && PauseMenu.instance.IsPaused) return;
        if (!isActiveTurn || penguinHasMoved) return;

        isAiming = true;
    }

    // Called by PlayerClickSelector2D on second click - fires in the mouse direction
    public void ConfirmLaunch(Vector2 mouseWorld)
    {
        if (!isAiming || !isSelected || !isActiveTurn || penguinHasMoved) return;

        // Ignore clicks too close to the penguin - cursor probably hasn't moved yet
        if (Vector2.Distance(mouseWorld, rb.position) < minAimDistance) return;

        isAiming = false;
        if (line != null) line.enabled = false;

        if (CurrentAimMode == AimMode.Item && HasItem)
            UseItem(mouseWorld);
        else
            Fling(mouseWorld);
    }

    // Toggle between Fling and Item aim modes, called when player presses E
    public void ToggleAimMode()
    {
        if (!HasItem) return;
        CurrentAimMode = CurrentAimMode == AimMode.Fling ? AimMode.Item : AimMode.Fling;
        RefreshIndicator();
    }

    public void Select()
    {
        isSelected = true;
        CurrentAimMode = AimMode.Fling;
    }

    public void Deselect()
    {
        isSelected = false;
        isAiming = false;
        if (line != null) line.enabled = false;
    }

    // Called by ItemPickup when a penguin slides over a collectible
    public void GiveItem(GameObject itemPrefab, Sprite icon)
    {
        if (HasItem) return;
        heldItemPrefab = itemPrefab;
        heldItemIcon = icon;
        RefreshIndicator();
    }

    void Fling(Vector2 mouseWorld)
    {
        rb.linearVelocity = ComputeVelocityToward(mouseWorld, maxVelocity);
        penguinHasMoved = true;
        StartCoroutine(WaitForStopThenEndTurn());
    }

    void UseItem(Vector2 mouseWorld)
    {
        Vector2 throwDir = (mouseWorld - rb.position).normalized;

        // Spawn offset: use the penguin's own collider extents so the item always clears it
        float clearance = 0.5f;
        Collider2D penguinCol = GetComponent<Collider2D>();
        if (penguinCol != null)
            clearance += penguinCol.bounds.extents.magnitude;

        Vector2 spawnPos = rb.position + throwDir * clearance;

        GameObject itemObj = Instantiate(heldItemPrefab, spawnPos, Quaternion.identity);
        ThrowableItem item = itemObj.GetComponent<ThrowableItem>();

        if (item != null)
        {
            Vector2 velocity = ComputeVelocityToward(mouseWorld, item.maxThrowSpeed);
            item.LaunchImmediately(velocity);
        }

        heldItemPrefab = null;
        heldItemIcon = null;
        CurrentAimMode = AimMode.Fling;
        RefreshIndicator();

        penguinHasMoved = true;
        TurnManager.instance?.CheckTurn(this);
    }

    Vector2 ComputeVelocityToward(Vector2 mouseWorld, float maxSpeed)
    {
        Vector2 dir = mouseWorld - rb.position;
        return Vector2.ClampMagnitude(dir * velocityMultiplier, maxSpeed);
    }

    void DrawAimLine()
    {
        if (line == null || Camera.main == null) return;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        float speed = maxVelocity;
        if (CurrentAimMode == AimMode.Item && HasItem)
        {
            ThrowableItem itemRef = heldItemPrefab.GetComponent<ThrowableItem>();
            if (itemRef != null) speed = itemRef.maxThrowSpeed;
        }

        Vector2 velocity = ComputeVelocityToward(mouseWorld, speed);
        Vector2 start = rb.position;
        Vector2 end = start + velocity * lineLengthMultiplier;

        // White line for fling, yellow line for item use
        Color lineColor = (CurrentAimMode == AimMode.Item && HasItem) ? Color.yellow : Color.white;
        line.startColor = lineColor;
        line.endColor = lineColor;
        line.enabled = true;
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    // Updates the item indicator sprite and opacity to reflect current hold/mode state
    void RefreshIndicator()
    {
        if (itemIndicator == null) return;

        if (!HasItem)
        {
            // No item - hide the indicator entirely
            itemIndicator.enabled = false;
            return;
        }

        itemIndicator.enabled = true;
        itemIndicator.sprite = heldItemIcon;

        // Dim the icon when in fling mode to show the item is held but not being used
        itemIndicator.color = CurrentAimMode == AimMode.Item ? IndicatorItemMode : IndicatorFlingMode;
    }

    bool ImmuneThisTurn()
    {
        if (TurnManager.instance == null) return false;
        if (team == Team.Player && TurnManager.instance.CurrentTeamIndex == 0) return true;
        if (team == Team.Enemy && TurnManager.instance.CurrentTeamIndex == 1) return true;
        return false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerFlinger2D other = collision.gameObject.GetComponent<PlayerFlinger2D>();
        if (other == null) return;
        if (other.team == team) return;
        if (rb.linearVelocity.magnitude < 0.5f) return;
        if (other.ImmuneThisTurn()) return;

        Stats otherStats = other.GetComponent<Stats>();
        if (otherStats == null || stats == null) return;

        otherStats.TakeDamage(stats.damage);
    }

    IEnumerator WaitForStopThenEndTurn()
    {
        yield return new WaitUntil(() =>
            this == null || rb == null || rb.linearVelocity.sqrMagnitude < 0.01f
        );

        if (this == null || rb == null) yield break;

        rb.linearVelocity = Vector2.zero;
        TurnManager.instance?.CheckTurn(this);
    }
}