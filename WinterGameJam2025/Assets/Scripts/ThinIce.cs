using System.Collections;
using UnityEngine;

public class ThinIce : MonoBehaviour
{
    [Header("Break Settings")]
    [Tooltip("Total hits before fully breaking. With 2 hits: first hit shows cracked, second breaks it.")]
    public int hitsToBreak = 2;
    [Tooltip("Minimum speed for an item sliding through to count as a hit.")]
    public float impactForceThreshold = 2.5f;
    [Tooltip("Seconds between the broken sprite appearing and the kill zone activating.")]
    public float breakDelay = 0.4f;

    [Header("Sprites")]
    [Tooltip("Leave null to keep the zone invisible until cracked.")]
    public Sprite spriteNormal;
    public Sprite spriteCracked;
    [Tooltip("Blue circle matching the water colour, shown when fully broken.")]
    public Sprite spriteBroken;

    [Header("SFX")]
    public AudioClip crackSound;
    public AudioClip breakSound;

    [Header("References")]
    [Tooltip("Drag the KillZone child GameObject here. The whole object is disabled until the ice breaks.")]
    public GameObject killZoneObject;

    private SpriteRenderer sr;
    private AudioSource audioSource;
    private int currentHits = 0;
    private bool isBroken = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        // Disable the entire child so neither its collider nor script can fire
        if (killZoneObject != null)
            killZoneObject.SetActive(false);

        SetSprite(spriteNormal);
    }

    // Called by ThrowableItem.Explode, ExplosionEffect, or any other mechanic
    public void OnDestructiveImpact(float force)
    {
        if (isBroken) return;
        if (force < impactForceThreshold) return;

        currentHits++;

        if (currentHits >= hitsToBreak)
            StartCoroutine(BreakSequence());
        else
            ApplyCrack();
    }

    void ApplyCrack()
    {
        SetSprite(spriteCracked);
        PlaySound(crackSound);
    }

    IEnumerator BreakSequence()
    {
        isBroken = true;

        SetSprite(spriteBroken);
        PlaySound(breakSound);

        // Brief pause so the player sees the break before the kill zone activates
        yield return new WaitForSeconds(breakDelay);

        if (killZoneObject != null)
            killZoneObject.SetActive(true);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isBroken) return;

        // Explosion blast radius (from ExplosionEffect prefab)
        ExplosionEffect explosion = other.GetComponent<ExplosionEffect>();
        if (explosion != null)
        {
            OnDestructiveImpact(explosion.force);
            return;
        }

        // A thrown item sliding through the zone
        // Only reacts if cracksThinIceOnContact is true on that item.
        // Bombs set this to false so only their explosion blast cracks the ice,
        // preventing a double-hit (once on contact, once from OverlapCircleAll in Explode).
        ThrowableItem item = other.GetComponent<ThrowableItem>();
        if (item != null && item.cracksThinIceOnContact)
        {
            Rigidbody2D itemRb = item.GetComponent<Rigidbody2D>();
            float speed = itemRb != null ? itemRb.linearVelocity.magnitude : item.triggerForce;
            OnDestructiveImpact(speed);
        }
    }

    void SetSprite(Sprite sprite)
    {
        if (sr == null) return;
        if (sprite != null) sr.sprite = sprite;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}