using UnityEngine;

public class PlayerSelectionManager2D : MonoBehaviour
{
    public static PlayerSelectionManager2D Instance;

    private PlayerFlinger2D current;

    void Awake()
    {
        Instance = this;
    }

    public void Select(PlayerFlinger2D player)
    {
        if (current != null)
            current.Deselect();

        current = player;
        current.Select();
    }


}
