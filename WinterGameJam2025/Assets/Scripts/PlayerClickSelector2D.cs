using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerClickSelector2D : MonoBehaviour
{
    public LayerMask playerLayer;

    private PlayerFlinger2D draggingPlayer;

    void Update()
    {
        if (Mouse.current == null)
            return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);

        // Mouse pressed
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Collider2D hit = Physics2D.OverlapPoint(mouseWorld, playerLayer);
            if (hit == null)
                return;

            PlayerFlinger2D player = hit.GetComponent<PlayerFlinger2D>();
            if (player == null)
                return;

            PlayerSelectionManager2D.Instance.Select(player);
            player.BeginDrag();
            draggingPlayer = player;
        }

        // Mouse released
        if (Mouse.current.leftButton.wasReleasedThisFrame && draggingPlayer != null)
        {
            draggingPlayer.Release();
            draggingPlayer = null;
        }
    }
}
