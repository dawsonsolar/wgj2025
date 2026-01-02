using System.Collections;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerFlinger2D))]
public class EnemyAI : MonoBehaviour
{
    public float thinkDelay = 0.5f;
    public float aimErrorDegrees = 7f;
    public int maxAimAttempts = 12;
    public float dangerCheckDistance = 4f;
    public int maxVisitedGaps = 7;
    public LayerMask obstacleMask;   // walls
    public LayerMask killZoneMask;    // kill zones

    private PlayerFlinger2D flinger;
    private Rigidbody2D rb;
    private HashSet<Transform> visitedPaths = new HashSet<Transform>();

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
            TurnManager.instance?.CheckTurn(flinger);
            yield break;
        }

        Vector2 goalPos = target.transform.position;

        // If path blocked, aim for gap
        if (PathIsDangerous((goalPos - rb.position).normalized))
        {
            Transform gap = FindBestGapWithLOS(goalPos);
            if (gap != null)
                goalPos = gap.position;
        }

        Debug.DrawLine(rb.position, goalPos, Color.green, 1.5f);
        Vector2 launchVelocity = ComputeSafeVelocity(goalPos);

        // Clamp linear velocity
        rb.linearVelocity = Vector2.ClampMagnitude(launchVelocity, flinger.maxVelocity);
        flinger.penguinHasMoved = true;

        // Wait for movement to stop or object to die
        yield return new WaitUntil(() =>
            this != null &&
            rb != null &&
            rb.linearVelocity.sqrMagnitude < 0.01f
        );

        if (this == null || rb == null)
            yield break;

        rb.linearVelocity = Vector2.zero;

        TurnManager.instance?.CheckTurn(flinger);
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
        Vector2 direction = (targetPos - (Vector2)transform.position);
        Vector2 baseDir = direction.normalized;

        // Check if path to player is completely clear
        bool hasLineOfSight = !PathIsDangerous(baseDir);

        float finalSpeed;

        if (hasLineOfSight)
        {
            // Ram the player at max velocity
            finalSpeed = flinger.maxVelocity;
        }
        else
        {
            // Path is blocked, scale velocity based on distance to the target (gap or marker)
            float distance = direction.magnitude;
            float desiredSpeed = distance * 1.2f; // tweak multiplier to slightly overshoot
            finalSpeed = Mathf.Min(desiredSpeed, flinger.maxVelocity);
        }

        // Try multiple angles for randomness / obstacle avoidance
        for (int i = 0; i < maxAimAttempts; i++)
        {
            float spread = aimErrorDegrees + i * 5f;
            float angle = (i % 2 == 0 ? 1 : -1) * spread;

            Vector2 testDir = Quaternion.Euler(0, 0, angle) * baseDir;

            if (!PathIsDangerous(testDir))
            {
                return testDir * finalSpeed;
            }
        }

        // fallback (even if all angles are blocked, still fling in base direction)
        return baseDir * finalSpeed;
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

    Transform FindBestGapWithLOS(Vector2 playerPos)
    {
        GameObject[] gaps = GameObject.FindGameObjectsWithTag("AIGap");

        Transform best = null;
        float bestScore = float.MinValue;

        foreach (var g in gaps)
        {
            Transform gap = g.transform;

            if (visitedPaths.Contains(gap))
                continue;

            bool seesPlayer = HasLineOfSight(gap.position, playerPos);
            float distToPlayer = Vector2.Distance(gap.position, playerPos);
            float distFromEnemy = Vector2.Distance(transform.position, gap.position);

            // Score system
            float score = 0f;

            if (seesPlayer)
                score += 1000f; // HUGE priority

            score -= distToPlayer * 2f;
            score -= distFromEnemy;

            if (score > bestScore)
            {
                bestScore = score;
                best = gap;
            }
        }

        return best;
    }


    void MarkGapVisited(Transform gap)
    {
        if (gap == null) 
            return;

        visitedPaths.Add(gap);

        if (visitedPaths.Count >= maxVisitedGaps)
        {
            visitedPaths.Clear();
        }
    }

    bool HasLineOfSight(Vector2 from, Vector2 to)
    {
        Vector2 dir = (to - from).normalized;
        float dist = Vector2.Distance(from, to);

        RaycastHit2D hit = Physics2D.Raycast(
            from,
            dir,
            dist,
            obstacleMask
        );

        return hit.collider == null;
    }

}
