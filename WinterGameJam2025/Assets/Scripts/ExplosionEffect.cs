using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [Tooltip("Force value reported to ThinIce tiles caught in the blast.")]
    public float force = 8f;

    [Tooltip("How long the trigger stays active (keep very short — just long enough for physics to register).")]
    public float lifetime = 0.12f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}