
using Unity.VisualScripting;
using UnityEngine;

public class PenguinSpriteController : MonoBehaviour
{
    [Header("Base Sprites")]
    public Sprite upSprite;
    public Sprite downSprite;
    public Sprite rightSprite;
    public Sprite uprightSprite;
    public Sprite downrightSprite;

    [Header("Sliding Sprite")]
    public Sprite slideSprite;
    
    private SpriteRenderer sr;
    private Rigidbody2D rb;

    private Vector2 lastDirection = Vector2.right;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        transform.localScale = Vector3.one * 0.5f;
    }

    void Update()
    {
        UpdateSprite();
    }

    void UpdateSprite()
    {
        Vector2 velocity = rb.linearVelocity;

        if (velocity.sqrMagnitude > 0.05f)
        {
            sr.sprite = slideSprite;

            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;
            sr.transform.rotation = Quaternion.Euler(0, 0, angle);

            sr.flipX = velocity.x < 0;
            lastDirection = velocity.normalized;
        }
        else
        {
            sr.transform.rotation = Quaternion.identity;
            float angle = Vector2.SignedAngle(Vector2.right, lastDirection);

            sr.flipX = false;

            if (angle >= -22.5f && angle < 22.5f) // right
            {
                sr.sprite = rightSprite;
            }
            else if (angle >= 22.5f && angle < 67.5f) // up right
            {
                sr.sprite = uprightSprite;
            }
            else if (angle >= 67.5f && angle < 112.5f) // up
            {
                sr.sprite = upSprite;
            }
            else if (angle >= 112.5f && angle < 157.5f) // upleft
            {
                sr.sprite = uprightSprite;
                sr.flipX = true;
            }
            else if (angle >= 157.5f || angle < -157.5f) // left
            {
                sr.sprite = rightSprite;
                sr.flipX = true;
            }
            else if (angle >= -157.5f && angle < -112.5f) // downleft
            {
                sr.sprite = downrightSprite;
                sr.flipX = true;
            }
            else if (angle >= -112.5f && angle < -67.5f) // down
            {
                sr.sprite = downSprite;
            }
            else if (angle >= -67.5f && angle < -22.5f) // downright
            {
                sr.sprite = downrightSprite;
            }
        }
    }
}
