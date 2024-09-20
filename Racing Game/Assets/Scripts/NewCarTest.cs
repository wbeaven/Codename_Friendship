using TreeEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class NewCarTest : MonoBehaviour
{
    public Rigidbody carRb;
    public Transform wheel;

    public float strength;
    public float damping;
    public float restDistance;

    public float offset;
    public float velocity;
    public float force;

    public RaycastHit hit;
    public bool wheelRayHit;

    public Transform wheelVisual;
    public float visualOffset;
    public float visualRot;
    public float visualRotSpd;

    public float wheelMass;
    [Range (0f, 1f)]
    public float tyreGrip;

    private PlayerInput playerInput;
    private InputSystem_Actions playerInputActions;

    public bool turnable;
    public float turnRot;
    public float rotSpeed = 1f;

    public AnimationCurve torqueCurve;
    public float carSpeedMultiplier;
    public float carTopSpeed;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        playerInputActions = new InputSystem_Actions();
        playerInputActions.Player.Enable();
    }

    private void FixedUpdate()
    {
        WheelRaycast();

        if(wheelRayHit)
        {
            Spring();
            SteeringPhysics();
            Acceleration();
        }

        if (turnable)
            SteeringControls();

        WheelVisuals();
    }


    private void WheelRaycast()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.8f))
        {
            wheelRayHit = true;
        }
        else
        {
            wheelRayHit = false;
        }
    }

    private void Spring()
    {
        Vector3 springDir = transform.up;
        Vector3 wheelVel = carRb.GetPointVelocity(transform.position);
        offset = restDistance - hit.distance;
        velocity = Vector3.Dot(springDir, wheelVel);
        force = (offset * strength) - (velocity * damping);

        carRb.AddForceAtPosition(springDir * force, transform.position);
    }

    private void SteeringPhysics()
    {
        Vector3 steeringDir = transform.right;
        Vector3 wheelVel = carRb.GetPointVelocity(transform.position);
        float steeringVel = Vector3.Dot(steeringDir, wheelVel);
        float targetVelChange = -steeringVel * tyreGrip;
        float targetAccel = targetVelChange / Time.fixedDeltaTime;

        carRb.AddForceAtPosition(steeringDir * wheelMass * targetAccel, transform.position);
    }

    private void Acceleration()
    {
        Vector3 accelDir = transform.forward;
        Vector2 movement = playerInputActions.Player.Move.ReadValue<Vector2>();

        if (movement.y != 0f)
        {
            float carSpeed = Vector3.Dot(carRb.transform.forward, carRb.linearVelocity);
            float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carTopSpeed);   
            float availableTorque = torqueCurve.Evaluate(normalizedSpeed) * movement.y;

            carRb.AddForceAtPosition(accelDir * availableTorque * carSpeedMultiplier, transform.position);
            print(availableTorque * carSpeedMultiplier);
        }
    }

    private void SteeringControls()
    {
        Vector2 movement = playerInputActions.Player.Move.ReadValue<Vector2>();

        if (movement.x > 0)
        {
            // turn wheels 30 degrees to the right
            turnRot += rotSpeed * Time.deltaTime;
        }
        else if (movement.x < 0)
        {
            // turn wheels 30 degrees to the left
            turnRot -= rotSpeed * Time.deltaTime;
        }

        float minRot = 0 - 30 * Mathf.Abs(movement.x);
        float maxRot = 0 + 30 * Mathf.Abs(movement.x);
        turnRot = Mathf.Clamp(turnRot, minRot, maxRot);
        transform.localRotation = Quaternion.Euler(0, turnRot, 0);
    }

    private void WheelVisuals()
    {
        if (wheelRayHit)
        {
            wheelVisual.position = new Vector3(wheelVisual.position.x, hit.point.y + visualOffset, wheelVisual.position.z);
        }
        else
        {
            wheelVisual.localPosition = new Vector3(0, 0, 0);    
        }


        visualRotSpd = carRb.GetPointVelocity(transform.position).magnitude / carTopSpeed;
        visualRot += visualRotSpd * Time.deltaTime * 10000;
        wheelVisual.localRotation = Quaternion.Euler(visualRot, 0, 90);
    }
}
