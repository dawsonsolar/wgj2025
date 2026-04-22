using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Tooltip("The ThrowableItem prefab given to the penguin that picks this up.")]
    public GameObject itemPrefab;

    [Tooltip("Icon shown on any UI you add — e.g. a small indicator near the penguin.")]
    public Sprite itemIcon;

    [Tooltip("Optional sound played when picked up.")]
    public AudioClip pickupSound;

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerFlinger2D penguin = other.GetComponent<PlayerFlinger2D>();
        if (penguin == null) return;
        if (penguin.team != PlayerFlinger2D.Team.Player) return; // only players pick up
        if (penguin.HasItem) return; // already carrying one

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        penguin.GiveItem(itemPrefab, itemIcon);
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0f, 0.4f);
        Gizmos.DrawSphere(transform.position, 0.4f);
    }
#endif
}