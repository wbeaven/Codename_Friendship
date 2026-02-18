using UnityEngine;
using UnityEngine.Splines;

public class SplineFollow : MonoBehaviour
{
    [SerializeField] SplineContainer splineContainer;
    [SerializeField] Transform connectedCar;

    [Range(0f, 1f)]
    [SerializeField] float progress;

    [SerializeField] float speed = 0.2f;
    [SerializeField] float followDistance = 15f;
    [SerializeField] bool moveForward = true;
    [SerializeField] bool isMoving = true;

    void Update()
    {
        isMoving = Vector3.Distance(transform.position, connectedCar.position) <= followDistance;

        if (isMoving)
        {
            float direction = moveForward ? 1f : -1f;
            progress += direction * speed * Time.deltaTime;

            progress = Mathf.Repeat(progress, 1f); ;
        }

        UpdatePosition();
    }

    void UpdatePosition()
    {
        if (splineContainer == null) return;

        Vector3 position = splineContainer.EvaluatePosition(progress);
        Vector3 forward = splineContainer.EvaluateTangent(progress);

        transform.position = position;

        if (forward != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(forward);
    }
}
