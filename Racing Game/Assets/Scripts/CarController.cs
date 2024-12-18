using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class CarController : MonoBehaviour
{
    [SerializeField] Rigidbody carRb;
    private Transform wheel;

    [SerializeField] float strength;
    [SerializeField] float damping;
    [SerializeField] float restDistance;

    private float offset;
    private float velocity;
    private float force;

    private RaycastHit hit;
    private bool wheelRayHit;

    private Transform wheelVisual;

    [Space(15)]

    [SerializeField] float visualOffset;
    private float visualRot;
    [SerializeField] float visualRotSpd;
    [SerializeField] float visualRotMultiplier;
    [SerializeField] float returnSpeed;

    [Space(15)]

    [SerializeField] float wheelMass;
    [Range (0f, 1f)]
    [SerializeField] float tyreGrip;

    private PlayerInput playerInput;
    private InputSystem_Actions playerInputActions;

    [SerializeField] bool turnable;
    private float turnRot;
    [SerializeField] float rotSpeed;

    [SerializeField] AnimationCurve torqueCurve;
    [SerializeField] float accelSpeed;
    [SerializeField] float decelSpeed;
    [SerializeField] float carTopSpeed;
    [SerializeField] bool driveWheel;
    [SerializeField] float dragMultiplier;

    [SerializeField] float brakeForce;

    CinemachineInputAxisController controller;
    
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        playerInputActions = new InputSystem_Actions();
        playerInputActions.Player.Enable();

        wheel = transform;
        wheelVisual = transform.GetChild(0);

        controller = GameObject.Find("FreeLook Camera").GetComponent<CinemachineInputAxisController>();
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
        // Add force to the side of the wheel so the car doesn't like moving laterally
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
            //Vector2 movement = playerInputActions.Player.Move.ReadValue<Vector2>();
            float movement = playerInputActions.Player.Accelerate.ReadValue<float>();

            if (movement > 0f)
            {
                // If input vector is positive, add acceleration force to the wheel based on torque and speed variables
                //float carSpeed = Vector3.Dot(carRb.transform.forward, carRb.linearVelocity);
                float carSpeed = carRb.linearVelocity.magnitude;
                float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carTopSpeed);   
                float availableTorque = torqueCurve.Evaluate(normalizedSpeed) * movement;

                carRb.AddForceAtPosition(accelDir * availableTorque * accelSpeed, transform.position);
                print(name + " normalized speed: " + normalizedSpeed + ", movement: " + movement);
                print(name + " available torque: " + availableTorque + ", acceleration speed: " + accelSpeed + ", available torque * acceleration speed: " + availableTorque * accelSpeed);
                print(name + " - " + carRb.GetPointVelocity(transform.position).magnitude);
                print(name + " - " + carRb.linearVelocity.magnitude);
                print(name + " - " + Vector3.Dot(carRb.transform.forward, carRb.linearVelocity));
                print(name + " - " + Mathf.Abs(carSpeed));
                print(name + " - " + Mathf.Abs(carSpeed) / carTopSpeed);
            }
            else if (movement < 0f)
            {
                // If input vector is negative, add deceleration force to the wheel based on torque and speed variables
                float carSpeed = carRb.linearVelocity.magnitude;
                float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carTopSpeed);   
                float availableTorque = torqueCurve.Evaluate(normalizedSpeed) * movement;

                carRb.AddForceAtPosition(accelDir * availableTorque * decelSpeed, transform.position);
                print(name + " normalized speed: " + normalizedSpeed + ", movement: " + movement);
                print(name + " available torque: " + availableTorque + ", deceleration speed: " + decelSpeed + ", available torque * deceleration speed: " + availableTorque * decelSpeed);
                print(name + " - " + carRb.GetPointVelocity(transform.position).magnitude);
                print(name + " - " + carRb.linearVelocity.magnitude);
                print(name + " - " + Vector3.Dot(carRb.transform.forward, carRb.linearVelocity));
                print(name + " - " + Mathf.Abs(carSpeed));
                print(name + " - " + Mathf.Abs(carSpeed) / carTopSpeed);
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