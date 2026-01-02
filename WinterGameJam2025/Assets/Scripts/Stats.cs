using UnityEngine;

public class Stats : MonoBehaviour
{
    public int currentHealth;
    public float maxVelocity = 12f;
    public float velocityMultiplier = 5f;
    public int maxHealth = 100;
    public int damage = 10;
    public HealthBarUI healthBarPrefab;
    private HealthBarUI healthBarInstance;



    void Awake()
    {
        currentHealth = maxHealth;
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            healthBarInstance.gameObject.SetActive(false);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (healthBarInstance != null)
        {
            healthBarInstance.Initialize(transform, currentHealth, maxHealth);
            Debug.Log("Displaying Health Bar");
        }

        Debug.Log($"{name} took {amount} damage. HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log($"{name} died!");

        // Mark AI as dead so coroutines can exit safely
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.IsDead = true;
            ai.StopAllCoroutines();
        }

        PlayerFlinger2D penguin = GetComponent<PlayerFlinger2D>();
        if (penguin != null && TurnManager.instance != null)
        {
            TurnManager.instance.OnPenguinDied(penguin);
        }

        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance.gameObject);
        }

        Destroy(gameObject);
    }
}
