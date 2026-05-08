using System.Collections;
using UnityEngine;

public class PathProgressBar : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Transform _player;
    public Transform[] PathPoints;

    public IEnumerator Start()
    {
        while (GameManager.Instance == null || !GameManager.Instance.IsInitialized)
        {
            yield return null;
        }

        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            _player = GameManager.Instance.Player.transform;
            // Ensure PathPoints is not null and has at least two elements before accessing
            if (PathPoints != null && PathPoints.Length >= 2)
            {
                PathPoints[0].position = _player.position;
                EndGameTrigger endGameTrigger = FindAnyObjectByType<EndGameTrigger>();
                if (endGameTrigger != null)
                {
                    PathPoints[PathPoints.Length - 1].position = endGameTrigger.transform.position;
                }
                else
                {
                    Debug.LogWarning("EndGameTrigger not found. The last PathPoint will not be set automatically.");
                }
            }
            else
            {
                Debug.LogWarning("PathPoints array is null or has less than 2 points. Please assign path points in the inspector.");
            }
        }
        else
        {
            Debug.LogError("GameManager.Instance or Player not found! Please ensure GameManager is set up correctly.");
            enabled = false; // Disable this script if player isn't found
        }
    }

    private void Update()
    {
        if (!GameManager.Instance.IsInitialized)
            return;

        if ((_player == null || PathPoints == null || PathPoints.Length < 2))
        {
            GameManager.Instance.UIController.UpdateGameProgressBar(0f);
            return;
        }

        float progress = GetOverallPathProgress(_player.position, PathPoints);
        GameManager.Instance.UIController.UpdateGameProgressBar(progress);
    }

    /// <summary>
    /// Calculates the overall progress of the player along a path defined by multiple points.
    /// The progress is based on the closest point on the path to the player.
    /// </summary>
    /// <param name="playerPos">The current position of the player.</param>
    /// <param name="pathPoints">An array of Transform points defining the path. The first is the start, the last is the end.</param>
    /// <returns>A float representing the player's progress from 0.0 to 1.0.</returns>
    public float GetOverallPathProgress(Vector3 playerPos, Transform[] pathPoints)
    {
        if (pathPoints.Length < 2)
        {
            Debug.LogWarning("PathPoints array must contain at least two points (start and end).");
            return 0f;
        }

        float totalPathLength = 0f;
        // Calculate the total length of the entire path
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            totalPathLength += Vector3.Distance(pathPoints[i].position, pathPoints[i + 1].position);
        }

        if (totalPathLength <= Mathf.Epsilon)
        {
            return 0f; // Avoid division by zero if path has no length
        }

        float accumulatedPathLengthToClosestPoint = 0f;
        float shortestDistanceSqr = Mathf.Infinity;
        int closestSegmentIndex = -1;
        float tOnClosestSegment = 0f;

        // Find the segment that the player is closest to
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            Vector3 segmentStart = pathPoints[i].position;
            Vector3 segmentEnd = pathPoints[i + 1].position;

            // Get the closest point on the *line segment* to the player
            float t = GetClampedLinearParameter(playerPos, segmentStart, segmentEnd);
            Vector3 closestPointOnSegment = Vector3.Lerp(segmentStart, segmentEnd, t);
            float currentDistanceSqr = (playerPos - closestPointOnSegment).sqrMagnitude;

            if (currentDistanceSqr < shortestDistanceSqr)
            {
                shortestDistanceSqr = currentDistanceSqr;
                closestSegmentIndex = i;
                tOnClosestSegment = t;
            }
        }

        // Calculate progress based on the closest segment
        for (int i = 0; i < closestSegmentIndex; i++)
        {
            accumulatedPathLengthToClosestPoint += Vector3.Distance(pathPoints[i].position, pathPoints[i + 1].position);
        }

        // Add the progress within the closest segment
        if (closestSegmentIndex != -1)
        {
            Vector3 closestSegmentStart = pathPoints[closestSegmentIndex].position;
            Vector3 closestSegmentEnd = pathPoints[closestSegmentIndex + 1].position;
            accumulatedPathLengthToClosestPoint += Vector3.Distance(closestSegmentStart, closestSegmentEnd) * tOnClosestSegment;
        }

        // Clamp the final progress between 0 and 1
        return Mathf.Clamp01(accumulatedPathLengthToClosestPoint / totalPathLength);
    }

    /// <summary>
    /// Calculates the clamped linear parameter (t) for the closest point on a line segment.
    /// This 't' value ranges from 0 (at or before pointA) to 1 (at or after pointB).
    /// </summary>
    /// <param name="point">The point to find the closest position for.</param>
    /// <param name="pointA">The start point of the segment.</param>
    /// <param name="pointB">The end point of the segment.</param>
    /// <returns>The 't' value clamped between 0 and 1.</returns>
    public float GetClampedLinearParameter(Vector3 point, Vector3 pointA, Vector3 pointB)
    {
        Vector3 AB = pointB - pointA;
        Vector3 AP = point - pointA;

        float sqrMagnitudeAB = AB.sqrMagnitude;

        if (sqrMagnitudeAB <= Mathf.Epsilon)
        {
            // If the segment has no length, return 0 or 1 depending on whether point is at A
            return (point - pointA).sqrMagnitude < Mathf.Epsilon ? 0f : 0f;
        }

        float t = Vector3.Dot(AP, AB) / sqrMagnitudeAB;

        return Mathf.Clamp01(t);
    }
}