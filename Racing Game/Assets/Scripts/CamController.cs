using UnityEngine;
using UnityEngine.InputSystem;

public class CamController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform carTarget;

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0, 3, -6);
    [SerializeField] Vector3 targetOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] float followSpeed = 10f;
    [SerializeField] float maxDistance = 10f;
    [SerializeField] float minHeight = 2f;

    [Header("Turn Speeds")]
    [SerializeField] float sideTurnSpeed = 180f;
    [SerializeField] float lookBackTurnSpeed = 360f;

    [Header("Angles")]
    [SerializeField] float lookAngle = 60f;
    [SerializeField] float lookBackAngle = 180f;

    [Header("Input")]
    [SerializeField] float inputThreshold = 0.3f;

    private float currentYaw = 0f;
    private float targetYaw = 0f;
    private float currentTurnSpeed;

    private Vector2 lookInput;
    private bool isLookingBack;

    private PlayerInput playerInput;
    private InputAction lookAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        lookAction = playerInput.actions["Look"];
    }

    private void Update()
    {
        lookInput = lookAction.ReadValue<Vector2>();
        HandleLookInput();
    }

    private void LateUpdate()
    {
        currentYaw = Mathf.MoveTowardsAngle(currentYaw, targetYaw, currentTurnSpeed * Time.deltaTime);
        Quaternion rotation = Quaternion.Euler(0, currentYaw, 0);

        Vector3 rotatedOffset = rotation * offset;
        Vector3 desiredPosition = carTarget.position + rotatedOffset;

        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Distance clamp
        Vector3 dir = transform.position - carTarget.position;
        dir = Vector3.ClampMagnitude(dir, maxDistance);
        transform.position = carTarget.position + dir;

        // Height clamp to prevent camera going too low
        float minCameraHeight = carTarget.position.y + minHeight;
        if (transform.position.y < minHeight)
        {
            transform.position = new Vector3(transform.position.x, minCameraHeight, transform.position.z);
        }

        transform.LookAt(carTarget.position + targetOffset);
    }

    private void HandleLookInput()
    {
        float horizontal = lookInput.x;
        float vertical = lookInput.y;
        float carYaw = carTarget.eulerAngles.y;

        if (vertical < -inputThreshold)
        {
            // Look back
            targetYaw = carYaw + lookBackAngle;
            currentTurnSpeed = lookBackTurnSpeed;
            isLookingBack = true;
        }
        else if (horizontal < -inputThreshold)
        {
            // Look left
            targetYaw = carYaw - lookAngle;
            currentTurnSpeed = sideTurnSpeed;
            isLookingBack = false;
        }
        else if (horizontal > inputThreshold)
        {
            // Look right
            targetYaw = carYaw + lookAngle;
            currentTurnSpeed = sideTurnSpeed;
            isLookingBack = false;
        }
        else
        {
            // Default behind view
            targetYaw = carYaw;
            currentTurnSpeed = isLookingBack ? lookBackTurnSpeed : sideTurnSpeed;
            isLookingBack = false;
        }
    }
}