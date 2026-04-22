using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerClickSelector2D : MonoBehaviour
{
    public LayerMask playerLayer;

    private PlayerFlinger2D selectedPenguin;
    private bool waitingForConfirm;

    void Update()
    {
        if (Mouse.current == null || Keyboard.current == null) return;

        // Input is only meaningful on the player's turn
        if (TurnManager.instance == null || TurnManager.instance.CurrentTeamIndex != 0) return;
        if (PauseMenu.instance != null && PauseMenu.instance.IsPaused) return;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        HandleLeftClick(mouseWorld);
        HandleModeToggle();
        HandleCancel();
    }

    // Left click

    void HandleLeftClick(Vector2 mouseWorld)
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        if (!waitingForConfirm)
        {
            // First click — try to select a penguin
            TrySelectPenguin(mouseWorld);
        }
        else
        {
            // Second click — confirm launch in the mouse direction
            selectedPenguin?.ConfirmLaunch(mouseWorld);
            ClearSelection();
        }
    }

    void TrySelectPenguin(Vector2 mouseWorld)
    {
        Collider2D hit = Physics2D.OverlapPoint(mouseWorld, playerLayer);
        if (hit == null) return;

        PlayerFlinger2D penguin = hit.GetComponent<PlayerFlinger2D>();
        if (penguin == null) return;
        if (!penguin.isActiveTurn) return;
        if (penguin.penguinHasMoved) return;

        // Deselect any previously selected penguin
        if (selectedPenguin != null)
            selectedPenguin.Deselect();

        selectedPenguin = penguin;
        waitingForConfirm = true;

        PlayerSelectionManager2D.Instance?.Select(penguin);
        penguin.StartAiming();
    }

    // E key — toggle fling / item mode

    void HandleModeToggle()
    {
        if (!waitingForConfirm) return;
        if (selectedPenguin == null) return;
        if (!selectedPenguin.HasItem) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            selectedPenguin.ToggleAimMode();
    }

    // Cancel 

    void HandleCancel()
    {
        bool rightClick = Mouse.current.rightButton.wasPressedThisFrame;
        bool escape = Keyboard.current.escapeKey.wasPressedThisFrame;

        if (rightClick || escape)
            ClearSelection();
    }

    void ClearSelection()
    {
        if (selectedPenguin != null)
        {
            selectedPenguin.Deselect();
            selectedPenguin = null;
        }
        waitingForConfirm = false;
    }
}