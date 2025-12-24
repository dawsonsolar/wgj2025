using UnityEngine;

public class KillZone2D : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Stats stats = other.GetComponentInParent<Stats>();
        if (stats == null)
            return;

        Debug.Log($"{other.gameObject.name} fell off!");
        stats.Die();
    }

}
