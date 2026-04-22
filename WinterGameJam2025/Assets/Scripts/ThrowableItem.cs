using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ThrowableItem : MonoBehaviour
{
    [Header("Item Properties")]
    [Tooltip("If true, this item cracks ThinIce zones it physically slides through. Set false for the bomb so only the explosion blast cracks ice, not just touching it.")]
    public bool cracksThinIceOnContact = true;
    [Tooltip("Force reported to ThinIce during contact. The item's actual speed is used when available.")]
    public float triggerForce = 6f;

    [Header("Throw Settings")]
    public float maxThrowSpeed = 15f;

    [Header("Contact Damage")]
    [Tooltip("Damage dealt to a penguin the item physically hits. Used by both bomb (light tap) and rock (heavier hit).")]
    public int contactDamage = 10;
    [Tooltip("Minimum speed required to deal contact damage.")]
    public float contactDamageMinSpeed = 1f;

    [Header("Explosion")]
    [Tooltip("If true, explodes on first solid collision (contact grenade). Leave false for the fuse-timer bomb.")]
    public bool explodeOnImpact = false;

    [Header("Fuse Timer (bomb only)")]
    [Tooltip("If true, the item explodes after fuseTime seconds or as soon as it stops moving, whichever comes first.")]
    public bool explodeOnTimer = false;
    [Tooltip("Seconds until the bomb explodes. Ignored if explodeOnTimer is false.")]
    public float fuseTime = 3f;

    [Header("Explosion Stats")]
    public float explosionRadius = 1.8f;
    public int explosionDamage = 30;
    [Tooltip("Prefab containing ExplosionEffect script and CircleCollider2D set to trigger.")]
    public GameObject explosionPrefab;
    [Tooltip("Layer mask containing penguin colliders for blast damage.")]
    public LayerMask penguinLayer;

    [Header("Despawn (non-exploding items)")]
    [Tooltip("Seconds after coming to rest before the item removes itself. Only used when explodeOnTimer is false.")]
    public float selfDestructDelay = 4f;

    private Rigidbody2D rb;
    private bool thrown = false;
    private bool hasLanded = false;
    private float fuseTimer = 0f;
    private Collider2D ownerCollider;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = false;
    }

    void FixedUpdate()
    {
        if (!thrown || hasLanded) return;

        if (rb.linearVelocity.magnitude > maxThrowSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxThrowSpeed;

        if (explodeOnTimer)
        {
            fuseTimer -= Time.fixedDeltaTime;

            // Explode when the fuse runs out, or when the item stops moving after the immunity window
            bool stopped = rb.linearVelocity.sqrMagnitude < 0.01f;
            if (fuseTimer <= 0f || stopped)
                Explode();
        }
        else
        {
            // Non-exploding items just despawn after coming to rest
            if (rb.linearVelocity.sqrMagnitude < 0.01f)
            {
                hasLanded = true;
                rb.linearVelocity = Vector2.zero;
                StartCoroutine(DespawnAfterDelay());
            }
        }
    }

    // Called by PlayerFlinger2D immediately after spawning this prefab.
    // ownerCollider is the collider of the penguin that threw this item - collision
    // with the owner is ignored briefly so the item clears the penguin before physics resolves.
    public void LaunchImmediately(Vector2 velocity, Collider2D owner = null)
    {
        thrown = true;
        fuseTimer = fuseTime;
        rb.linearVelocity = Vector2.ClampMagnitude(velocity, maxThrowSpeed);

        // Ignore collision with the throwing penguin only - enemies can be hit immediately
        if (owner != null)
        {
            ownerCollider = owner;
            Collider2D myCol = GetComponent<Collider2D>();
            if (myCol != null)
            {
                Physics2D.IgnoreCollision(myCol, owner, true);
                StartCoroutine(ReEnableOwnerCollision(myCol, owner));
            }
        }
    }

    IEnumerator ReEnableOwnerCollision(Collider2D myCol, Collider2D owner)
    {
        // Wait until the item has had time to physically clear the owner's collider
        yield return new WaitForSeconds(0.4f);
        if (myCol != null && owner != null)
            Physics2D.IgnoreCollision(myCol, owner, false);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!thrown || hasLanded) return;

        // Deal light contact damage to any penguin hit directly
        if (contactDamage > 0 && rb.linearVelocity.magnitude >= contactDamageMinSpeed)
        {
            Stats stats = col.gameObject.GetComponentInParent<Stats>();
            if (stats != null) stats.TakeDamage(contactDamage);
        }

        // Explode immediately on contact if configured (not used for the standard bomb)
        if (explodeOnImpact)
            Explode();
    }

    void Explode()
    {
        if (hasLanded) return;
        hasLanded = true;

        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        // Damage all penguins caught in the blast radius
        Collider2D[] penguinHits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, penguinLayer);
        foreach (var hit in penguinHits)
        {
            Stats s = hit.GetComponent<Stats>();
            if (s != null) s.TakeDamage(explosionDamage);
        }

        // Crack or break any ThinIce zones in the blast radius
        // This always runs regardless of cracksThinIceOnContact - explosions always affect ice
        Collider2D[] areaHits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in areaHits)
        {
            ThinIce ice = hit.GetComponent<ThinIce>();
            if (ice != null) ice.OnDestructiveImpact(triggerForce);
        }

        Destroy(gameObject);
    }

    IEnumerator DespawnAfterDelay()
    {
        yield return new WaitForSeconds(selfDestructDelay);
        if (this != null) Destroy(gameObject);
    }
}