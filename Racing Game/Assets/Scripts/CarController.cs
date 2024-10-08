using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class CarController : MonoBehaviour
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
    public float visualRotMultiplier;
    public float returnSpeed;
    public Quaternion newRot;

    public float wheelMass;
    [Range (0f, 1f)]
    public float tyreGrip;

    private PlayerInput playerInput;
    private InputSystem_Actions playerInputActions;

    public bool turnable;
    public float turnRot;
    public float rotSpeed = 1f;

    public AnimationCurve torqueCurve;
    public float accelMultiplier;
    public float decelMultiplier;
    public float carTopSpeed;
    public bool driveWheel;
    public float dragMultiplier;

    public float brakeForce;

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
            Handbrake();
        }

        if (turnable)
            SteeringControls();

        WheelVisuals();
    }


    private void WheelRaycast()
    {
        // Fire a short raycast below the wheel and return a bool if it hit ground
        if (Physics.Raycast(transform.position, -transform.up, out hit, 0.8f))
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
        // Add a spring force to the car rigidbody at the wheel point to lift the car off the ground
        Vector3 springDir = transform.up;
        Vector3 wheelVel = carRb.GetPointVelocity(transform.position);
        offset = restDistance - hit.distance;
        velocity = Vector3.Dot(springDir, wheelVel);
        force = (offset * strength) - (velocity * damping);

        carRb.AddForceAtPosition(springDir * force, transform.position);
    }

    private void SteeringPhysics()
    {
        // Add force 
        Vector3 steeringDir = transform.right;
        Vector3 wheelVel = carRb.GetPointVelocity(transform.position);
        float steeringVel = Vector3.Dot(steeringDir, wheelVel);
        float targetVelChange = -steeringVel * tyreGrip;
        float targetAccel = targetVelChange / Time.fixedDeltaTime;

        carRb.AddForceAtPosition(steeringDir * wheelMass * targetAccel, transform.position);
    }

    private void Acceleration()
    {
        // Provides acceleration force if selected as a drive wheel
        if (driveWheel)
        {
            Vector3 accelDir = transform.forward;
            Vector2 movement = playerInputActions.Player.Move.ReadValue<Vector2>();

            if (movement.y > 0f)
            {
                // If input vector is positive, add acceleration force to the wheel based on torque and speed variables
                float carSpeed = Vector3.Dot(carRb.transform.forward, carRb.linearVelocity);
                float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carTopSpeed);   
                float availableTorque = torqueCurve.Evaluate(normalizedSpeed) * movement.y;

                carRb.AddForceAtPosition(accelDir * availableTorque * accelMultiplier, transform.position);
                print(name + " normalized speed: " + normalizedSpeed + ", movement: " + movement.y);
                print(name + " available torque: " + availableTorque + ", speed multiplier: " + accelMultiplier + ", available torque * speed multiplier: " + availableTorque * accelMultiplier);
                print(name + carRb.GetPointVelocity(transform.position).magnitude);
                print(name + Vector3.Dot(carRb.transform.forward, carRb.linearVelocity));
            }
            else if (movement.y < 0f)
            {
                // If input vector is negative, add deceleration force to the wheel based on torque and speed variables
                float carSpeed = Vector3.Dot(carRb.transform.forward, carRb.linearVelocity);
                float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carTopSpeed);   
                float availableTorque = torqueCurve.Evaluate(normalizedSpeed) * movement.y;

                carRb.AddForceAtPosition(accelDir * availableTorque * decelMultiplier, transform.position);
                print(name + " normalized speed: " + normalizedSpeed + ", movement: " + movement.y);
                print(name + " available torque: " + availableTorque + ", speed multiplier: " + decelMultiplier + ", available torque * speed multiplier: " + availableTorque * decelMultiplier);
                print(name + carRb.GetPointVelocity(transform.position).magnitude);
                print(name + Vector3.Dot(carRb.transform.forward, carRb.linearVelocity));
            }
            else
            {
                // Add a small counter force to slow car down when not accelerating
                carRb.AddForceAtPosition(Vector3.Normalize(-carRb.GetPointVelocity(transform.position)) * dragMultiplier * tyreGrip, transform.position);
            }
        }

    }

    private void SteeringControls()
    {
        // Turn wheels in the same direction as the input vector
        Vector2 movement = playerInputActions.Player.Move.ReadValue<Vector2>();

        if (movement.x > 0)
        {
            turnRot += rotSpeed * Time.deltaTime;
        }
        else if (movement.x < 0)
        {
            turnRot -= rotSpeed * Time.deltaTime;
        }

        float minRot = 0 - 30 * Mathf.Abs(movement.x);
        float maxRot = 0 + 30 * Mathf.Abs(movement.x);
        turnRot = Mathf.Clamp(turnRot, minRot, maxRot);
        transform.localRotation = Quaternion.Euler(0, turnRot, 0);
    }

    private void Handbrake()
    {
        float braking = playerInputActions.Player.Jump.ReadValue<float>();

        if (braking > 0)
        {
            // If grounded, add a braking force in the opposite direction to current velocity
            carRb.AddForceAtPosition(Vector3.Normalize(-carRb.GetPointVelocity(transform.position)) * brakeForce, transform.position);
        }
    }

    private void WheelVisuals()
    {
        Vector2 movement = playerInputActions.Player.Move.ReadValue<Vector2>();
        Vector3 currentPos = wheelVisual.position;

        if (wheelRayHit)
        {
            // If grounded, make the wheel sit on top of the surface
            wheelVisual.position = -transform.up * visualOffset + hit.point;
        }
        else if (!wheelRayHit && Vector3.Distance(wheelVisual.localPosition, Vector3.zero) > 0.05f)
        {
            // If in the air, move the wheels position back to its resting place
            Vector3 targetDir = (wheelVisual.localPosition - Vector3.zero).normalized;
            wheelVisual.localPosition -= returnSpeed * Time.deltaTime * targetDir;
        }
        else
            wheelVisual.localPosition = Vector3.zero;


        visualRotSpd = carRb.GetPointVelocity(transform.position).magnitude / carTopSpeed;
        visualRot += visualRotSpd * Time.deltaTime * visualRotMultiplier;
        if (wheelRayHit)
        {
            // If grounded, spin the wheel depending on velocity direction
            if (Vector3.Dot(transform.forward, Vector3.Normalize(carRb.GetPointVelocity(transform.position))) > 0)
                wheelVisual.localRotation = Quaternion.Euler(visualRot, 0, 0);
            else if (Vector3.Dot(transform.forward, Vector3.Normalize(carRb.GetPointVelocity(transform.position))) < 0)
                wheelVisual.localRotation = Quaternion.Euler(-visualRot, 0, 0);
            else
                wheelVisual.localRotation = Quaternion.Euler(0,0,0);
        }
        else
        {
            // If in the air, spin the wheel depending on player input
            if (movement.y > 0)
                wheelVisual.localRotation = Quaternion.Euler(visualRot, 0, 0);
            else if (movement.y < 0)
                wheelVisual.localRotation = Quaternion.Euler(-visualRot, 0, 0);
        }
    }
}