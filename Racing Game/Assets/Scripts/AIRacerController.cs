using UnityEngine;

public class AIRacerController : MonoBehaviour
{
    [SerializeField] float maxSteeringAngle = 45f;
    private Transform[] checkpoints;
    private Transform currentCheckpoint;
    private int currentCheckpointIndex;

    private void Start()
    {
        checkpoints = CheckpointController.originalCheckpoints;
        currentCheckpointIndex = 0;
        currentCheckpoint = checkpoints[currentCheckpointIndex];
    }

    private float GetSignedAngleToTarget(Transform target)
    {
        Vector3 toTarget = (target.position - transform.position).normalized;

        // Project both vectors onto the horizontal plane (Y-up world)
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 targetDir = Vector3.ProjectOnPlane(toTarget, Vector3.up).normalized;

        // Calculate signed angle between forward and target direction
        float angle = Vector3.SignedAngle(forward, targetDir, Vector3.up);

        print(angle);
        Debug.DrawRay(transform.position, transform.forward * 50);
        return angle; // Positive = turn left, Negative = turn right (around Y)
    }

    public float SteeringValue()
    {
        //if (GetSignedAngleToTarget(currentCheckpoint) > 10)
        //    return 1f;
        //else if (GetSignedAngleToTarget(currentCheckpoint) < 10)
        //    return -1f;
        //else return 0f;
        float angle = GetSignedAngleToTarget(currentCheckpoint);

        // Use a proportional response, clamped to -1 to 1
        float steering = Mathf.Clamp(angle / maxSteeringAngle, -1f, 1f);

        return steering;
    }

    public void NextCheckpoint()
    {
        if (currentCheckpointIndex + 1 < checkpoints.Length)
        {
            currentCheckpointIndex++;
            currentCheckpoint = checkpoints[currentCheckpointIndex];
        }
        else
        {
            currentCheckpointIndex = 0;
        }
    }
}
