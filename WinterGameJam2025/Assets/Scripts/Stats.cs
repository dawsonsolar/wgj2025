using UnityEngine;

public class Stats : MonoBehaviour
{
    public int currentHealth;
    public float maxVelocity = 12f;
    public float velocityMultiplier = 5f;
    public int maxHealth = 100;
    public int damage = 10;


    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"{name} took {amount} damage. HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log($"{name} died!");
        PlayerFlinger2D penguin = GetComponent<PlayerFlinger2D>();
        if (penguin != null && TurnManager.instance != null)
        {
            TurnManager.instance.OnPenguinDied(penguin);
        }

        Destroy(gameObject);
    }    
}
