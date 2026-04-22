using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerFlinger2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Timing")]
    public float thinkDelay = 0.8f;

    [Header("Aim Settings")]
    [Tooltip("Angular step for the direction sweep. Smaller = finer search.")]
    public float aimAngleStep = 8f;
    [Tooltip("How many angles to try before giving up and firing anyway.")]
    public int maxAimAttempts = 20;

    [Header("Path Checking")]
    [Tooltip("Radius used for CircleCast — should roughly match the penguin collider radius.")]
    public float penguinRadius = 0.35f;
    [Tooltip("Short look-ahead for the launch direction sweep (detects nearby walls).")]
    public float launchLookAhead = 3.5f;

    [Header("Layer Masks")]
    public LayerMask obstacleMask;
    public LayerMask killZoneMask;

    // AIGap waypoint support 
    // VINCENT - Place empty GameObjects tagged "AIGap" in your levels as navigation hints.
    // The AI works without them, but they help it navigate complex corridors in later levels.
    [Header("Waypoints (optional)")]
    [Tooltip("Max visited gaps before the visited list resets.")]
    public int maxVisitedGaps = 8;

    private readonly HashSet<Transform> visitedGaps = new HashSet<Transform>();

    private int visitedGapCount = 0;

    private PlayerFlinger2D flinger;
    private Rigidbody2D rb;

    public bool IsDead { get; set; }

    void Awake()
    {
        flinger = GetComponent<PlayerFlinger2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    // =========================================================================
    // Public entry point (called by TurnManager)
    // =========================================================================

    public IEnumerator TakeTurn()
    {
        if (IsDead || this == null) yield break;
        yield return AISequence();
    }

    // =========================================================================
    // Main AI coroutine
    // =========================================================================

    IEnumerator AISequence()
    {
        yield return new WaitForSeconds(thinkDelay);

        if (this == null || rb == null) yield break;

        PlayerFlinger2D target = FindNearestEnemy();
        if (target == null)
        {
            flinger.penguinHasMoved = true;
            TurnManager.instance?.CheckTurn(flinger);
            yield break;
        }

        Vector2 targetPos = target.transform.position;
        Vector2 goalPos = DecideGoal(targetPos);
        bool isWaypoint = (goalPos != targetPos);

        Vector2 launchVel = ComputeLaunchVelocity(goalPos, isWaypoint);
        rb.linearVelocity = Vector2.ClampMagnitude(launchVel, flinger.maxVelocity);
        flinger.penguinHasMoved = true;

        // Wait until this penguin stops moving (or gets destroyed)
        yield return new WaitUntil(() =>
            this == null || rb == null || rb.linearVelocity.sqrMagnitude < 0.01f
        );

        if (this == null || rb == null) yield break;
        rb.linearVelocity = Vector2.zero;

        TurnManager.instance?.CheckTurn(flinger);
    }

    // =========================================================================
    // Goal selection — direct shot, gap waypoint, or fallback
    // =========================================================================

    /// <summary>
    /// Returns the world position the AI should aim at this turn.
    /// Tries direct shot first, then optional AIGap waypoints, then fallback.
    /// </summary>
    Vector2 DecideGoal(Vector2 targetPos)
    {
        Vector2 myPos = rb.position;

        // 1. Clear path straight to target
        if (CanReach(myPos, targetPos))
        {
            Debug.DrawLine(myPos, targetPos, Color.green, 2f);
            return targetPos;
        }

        // 2. AIGap waypoint — reachable AND sees player
        Transform bestGap = FindBestReachableGap(targetPos);
        if (bestGap != null)
        {
            MarkGapVisited(bestGap);
            Debug.DrawLine(myPos, bestGap.position, Color.yellow, 2f);
            Debug.DrawLine(bestGap.position, targetPos, Color.cyan, 2f);
            return bestGap.position;
        }

        // 3. Any reachable AIGap (no LOS requirement)
        Transform anyGap = FindAnyReachableGap();
        if (anyGap != null)
        {
            MarkGapVisited(anyGap);
            Debug.DrawLine(myPos, anyGap.position, Color.magenta, 2f);
            return anyGap.position;
        }

        // 4. No clear path at all — aim directly and let the sweep try 
        return targetPos;
    }

    // =========================================================================
    // AIGap helpers
    // =========================================================================

    Transform FindBestReachableGap(Vector2 targetPos)
    {
        GameObject[] gapObjects = GameObject.FindGameObjectsWithTag("AIGap");
        if (gapObjects.Length == 0) return null;

        Transform best = null;
        float bestScore = float.MinValue;
        Vector2 myPos = rb.position;

        foreach (var gObj in gapObjects)
        {
            Transform gap = gObj.transform;
            Vector2 gapPos = gap.position;

            if (visitedGaps.Contains(gap)) continue;
            if (!CanReach(myPos, gapPos)) continue;   // AI must be able to reach gap

            bool seesPlayer = CanReach(gapPos, targetPos);
            float distToGap = Vector2.Distance(myPos, gapPos);
            float distGapToPlr = Vector2.Distance(gapPos, targetPos);

            float score = 0f;
            if (seesPlayer) score += 500f;   // Strongly prefer gaps with LOS to player
            score -= distToGap * 1.5f;
            score -= distGapToPlr * 2.0f;

            if (score > bestScore) { bestScore = score; best = gap; }
        }

        return best;
    }

    Transform FindAnyReachableGap()
    {
        GameObject[] gapObjects = GameObject.FindGameObjectsWithTag("AIGap");
        if (gapObjects.Length == 0) return null;

        Transform best = null;
        float bestDist = float.MaxValue;
        Vector2 myPos = rb.position;

        foreach (var gObj in gapObjects)
        {
            Transform gap = gObj.transform;
            if (visitedGaps.Contains(gap)) continue;
            if (!CanReach(myPos, gap.position)) continue;

            float d = Vector2.Distance(myPos, gap.position);
            if (d < bestDist) { bestDist = d; best = gap; }
        }

        return best;
    }

    void MarkGapVisited(Transform gap)
    {
        if (gap == null) return;
        visitedGaps.Add(gap);
        visitedGapCount++;

        if (visitedGapCount >= maxVisitedGaps)
        {
            visitedGaps.Clear();
            visitedGapCount = 0;
        }
    }

    // =========================================================================
    // Path / reachability checks
    // =========================================================================

    /// <summary>
    /// Full-path check: can a penguin-sized body travel from <from> to <to>
    /// without hitting a wall or kill zone? Checks the entire distance, not just
    /// a fixed look-ahead.
    /// </summary>
    bool CanReach(Vector2 from, Vector2 to)
    {
        Vector2 dir = (to - from).normalized;
        float dist = Vector2.Distance(from, to);

        bool wallHit = Physics2D.CircleCast(from, penguinRadius, dir, dist, obstacleMask).collider != null;
        bool killHit = Physics2D.CircleCast(from, penguinRadius, dir, dist, killZoneMask).collider != null;

        return !wallHit && !killHit;
    }

    /// <summary>
    /// Short look-ahead check used during launch direction sweep.
    /// Only checks a few units ahead so the AI doesn't refuse to fire along
    /// corridors that are open at launch but close later.
    /// </summary>
    bool IsLaunchDirectionBlocked(Vector2 from, Vector2 dir)
    {
        bool wallHit = Physics2D.CircleCast(from, penguinRadius, dir, launchLookAhead, obstacleMask).collider != null;
        bool killHit = Physics2D.CircleCast(from, penguinRadius, dir, launchLookAhead, killZoneMask).collider != null;
        return wallHit || killHit;
    }

    // =========================================================================
    // Launch velocity calculation
    // =========================================================================

    Vector2 ComputeLaunchVelocity(Vector2 goalPos, bool isWaypoint)
    {
        Vector2 myPos = rb.position;
        Vector2 toGoal = goalPos - myPos;
        Vector2 baseDir = toGoal.normalized;
        float dist = toGoal.magnitude;

        // Full speed when ramming the target; scaled when heading to a waypoint
        float speed = isWaypoint
            ? Mathf.Clamp(dist * 1.4f, 4f, flinger.maxVelocity)
            : flinger.maxVelocity;

        for (int i = 0; i < maxAimAttempts; i++)
        {
            float angle;
            if (i == 0)
            {
                angle = 0f; // always try exact direction first
            }
            else
            {
                int pair = (i + 1) / 2;
                float sign = (i % 2 == 1) ? 1f : -1f;
                angle = sign * pair * aimAngleStep;
            }

            // Never aim more than 90° away from the goal since that's just going backwards
            if (Mathf.Abs(angle) > 90f) continue;

            Vector2 testDir = (Vector2)(Quaternion.Euler(0f, 0f, angle) * baseDir);

            if (!IsLaunchDirectionBlocked(myPos, testDir))
            {
                Debug.DrawRay(myPos, testDir * 2f, Color.green, 1.5f);
                return testDir * speed;
            }
        }

        // All swept angles are blocked, fire in base direction as last resort
        Debug.DrawRay(myPos, baseDir * 2f, Color.red, 1.5f);
        return baseDir * speed;
    }

    // =========================================================================
    // Utility
    // =========================================================================

    PlayerFlinger2D FindNearestEnemy()
    {
        float bestDist = float.MaxValue;
        PlayerFlinger2D best = null;

        foreach (var p in TurnManager.instance.teamPlayers)
        {
            if (p == null) continue;
            float d = Vector2.Distance(transform.position, p.transform.position);
            if (d < bestDist) { bestDist = d; best = p; }
        }

        return best;
    }
}