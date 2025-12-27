using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerFlinger2D))]
public class EnemyAI : MonoBehaviour
{
    private PlayerFlinger2D flinger;
    private Rigidbody2D rb;

    public float thinkDelay = 0.5f;

    void Awake()
    {
        flinger = GetComponent<PlayerFlinger2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    public IEnumerator TakeTurn()
    {
        yield return new WaitForSeconds(thinkDelay);

        PlayerFlinger2D target = FindClosestEnemy();
        if (target == null)
            yield break;

        Vector2 direction =
            (target.transform.position - transform.position).normalized;

        // Launch towards a player/enemy penguin
        Vector2 launchVelocity = direction * flinger.maxVelocity;

        rb.linearVelocity = launchVelocity;
        flinger.penguinHasMoved = true;

        // Wait until physics settles
        yield return new WaitUntil(() => rb.linearVelocity.sqrMagnitude < 0.01f);

        rb.linearVelocity = Vector2.zero;

        TurnManager.instance.CheckTurn(flinger);
    }

    PlayerFlinger2D FindClosestEnemy()
    {
        float closestDist = float.MaxValue;
        PlayerFlinger2D closest = null;

        foreach (var p in TurnManager.instance.teamPlayers)
        {
            if (p == null)
                continue;

            float dist = Vector2.Distance(transform.position, p.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = p;
            }
        }
        return closest;
    }
}
