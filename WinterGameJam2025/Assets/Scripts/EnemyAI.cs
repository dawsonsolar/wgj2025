using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerFlinger2D))]
public class EnemyAI : MonoBehaviour
{
    public float thinkDelay = 0.5f;
    public float aimErrorDegrees = 7f;
    public int maxAimAttempts = 12;
    public float dangerCheckDistance = 4f;
    public LayerMask obstacleMask;   // walls
    public LayerMask killZoneMask;    // kill zones

    private PlayerFlinger2D flinger;
    private Rigidbody2D rb;

    public bool IsDead { get; set; }


    void Awake()
    {
        flinger = GetComponent<PlayerFlinger2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    public IEnumerator TakeTurn()
    {
        if (IsDead || this == null)
            yield break;

        yield return AISequence();
    }

    IEnumerator AISequence()
    {
        yield return new WaitForSeconds(thinkDelay);

        PlayerFlinger2D target = FindNearestEnemy();
        if (target == null)
        {
            flinger.penguinHasMoved = true;
            TurnManager.instance.CheckTurn(flinger);
            yield break;
        }

        Vector2 goalPos = target.transform.position;

        // If direct path is blocked, aim for a gap instead
        if (PathIsDangerous((goalPos - rb.position).normalized))
        {
            Transform gap = FindBestGap();
            if (gap != null)
            {
                goalPos = gap.position;
            }
        }
        Debug.DrawLine(rb.position, goalPos, Color.green, 1.5f);
        Vector2 launchVelocity = ComputeSafeVelocity(goalPos);


        rb.linearVelocity = launchVelocity;
        flinger.penguinHasMoved = true;

        yield return new WaitUntil(() => rb.linearVelocity.sqrMagnitude < 0.01f);
        if (IsDead || this == null) 
            yield break;
        rb.linearVelocity = Vector2.zero;

        TurnManager.instance.CheckTurn(flinger);
    }

    // Find nearest opposing penguin
    PlayerFlinger2D FindNearestEnemy()
    {
        float bestDist = float.MaxValue;
        PlayerFlinger2D best = null;

        foreach (var p in TurnManager.instance.teamPlayers)
        {
            if (p == null) continue;

            float d = Vector2.Distance(transform.position, p.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = p;
            }
        }

        return best;
    }

    // Pick a safe launch vector
    Vector2 ComputeSafeVelocity(Vector2 targetPos)
    {
        Vector2 baseDir = (targetPos - (Vector2)transform.position).normalized;

        for (int i = 0; i < maxAimAttempts; i++)
        {
            float spread = aimErrorDegrees + i * 5f;
            float angle = (i % 2 == 0 ? 1 : -1) * spread;

            Vector2 testDir = Quaternion.Euler(0, 0, angle) * baseDir;

            if (!PathIsDangerous(testDir))
            {
                Vector2 velocity = testDir * flinger.velocityMultiplier;
                return Vector2.ClampMagnitude(velocity, flinger.maxVelocity);
            }
        }

        // fallback (still imperfect on purpose)
        Vector2 fallback = baseDir * flinger.velocityMultiplier;
        return Vector2.ClampMagnitude(fallback, flinger.maxVelocity);
    }



    // Detect walls or kill zones in the launch path
    bool PathIsDangerous(Vector2 direction)
    {
        Vector2 rayDir = direction.normalized;

        float radius = 0.4f; // tweak to roughly penguin size

        RaycastHit2D hitWall = Physics2D.CircleCast(
            rb.position,
            radius,
            rayDir,
            dangerCheckDistance,
            obstacleMask
        );

        if (hitWall.collider != null)
            return true;

        RaycastHit2D hitKill = Physics2D.CircleCast(
            rb.position,
            radius,
            rayDir,
            dangerCheckDistance,
            killZoneMask
        );
        Debug.Log($"AI checking path: {direction}");
        Debug.DrawRay(rb.position, rayDir * dangerCheckDistance, Color.red, 1f);

        return hitKill.collider != null;
    }

    Transform FindBestGap()
    {
        GameObject[] gaps = GameObject.FindGameObjectsWithTag("AIGap");

        Transform best = null;
        float bestDist = float.MaxValue;

        foreach (var g in gaps)
        {
            float d = Vector2.Distance(transform.position, g.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = g.transform;
            }
        }

        return best;
    }


}
