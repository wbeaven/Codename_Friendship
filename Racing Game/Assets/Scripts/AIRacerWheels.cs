using UnityEngine;

public class AIRacerWheels : MonoBehaviour
{
    [SerializeField] Rigidbody carRb;
    private AIRacerController racerController;
    private Transform wheel;
    public bool grounded, turning;

    [Header("Spring Settings")]
    [SerializeField] float strength;
    [SerializeField] float damping;
    [SerializeField] float restDistance;

    private float offset;
    private float velocity;
    private float force;

    private RaycastHit hit;
    private bool wheelRayHit;
    private bool wheelVisualRayHit;

    private Transform wheelVisual;

    [Space(15)]

    [Header("Wheel Visuals")]
    [SerializeField] float visualOffset;
    private float visualRot;
    private float visualRotSpd;
    [SerializeField] float visualRotMultiplier;
    [SerializeField] float groundTouchDistance;
    [SerializeField] float returnSpeed;

    [Space(15)]

    [Header("Wheel Settings")]
    [SerializeField] float wheelMass;
    [Range(0f, 1f)]
    [SerializeField] float tyreGrip;
    [SerializeField] bool driveWheel;
    [SerializeField] bool turnable;
    private float turnRot;
    [SerializeField] float rotSpeed;

    [Header("Acceleration Settings")]
    [SerializeField] AnimationCurve torqueCurve;
    [SerializeField] float accelSpeed;
    [SerializeField] float decelSpeed;
    [SerializeField] float carTopSpeed;

    [Header("Deceleration Settings")]
    [SerializeField] float dragMultiplier;
    [SerializeField] float brakeForce;

    private void Awake()
    {
        racerController = transform.GetComponentInParent<AIRacerController>();
        wheel = transform;
        wheelVisual = transform.GetChild(0);
    }

    private void FixedUpdate()
    {
        WheelRaycast();
        WheelVisualRaycast();

        if (wheelRayHit)
        {
            Spring();
            SteeringPhysics();
            Acceleration();
            //Handbrake();
        }

        if (turnable)
            SteeringControls();

        WheelVisuals();
    }


    private void WheelRaycast()
    {
        // Fire a short raycast below the wheel and return a bool if it hit ground
        if (Physics.Raycast(transform.position, -transform.up, out hit, groundTouchDistance))
        {
            wheelRayHit = true;
            grounded = true;
        }
        else
        {
            wheelRayHit = false;
            grounded = false;
        }
    }
    private void WheelVisualRaycast()
    {
        // Fire a short raycast below the wheel and return a bool if it hit ground
        if (Physics.Raycast(transform.position, -transform.up, out hit, groundTouchDistance + 0.1f))
        {
            wheelVisualRayHit = true;
        }
        else
        {
            wheelVisualRayHit = false;
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
            float movement = 1; // AI acceleration coming from the main controller in the future

            if (movement > 0f)
            {
                // If input vector is positive, add acceleration force to the wheel based on torque and speed variables
                float carSpeed = carRb.linearVelocity.magnitude;
                float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carTopSpeed);
                float availableTorque = torqueCurve.Evaluate(normalizedSpeed) * movement;

                carRb.AddForceAtPosition(accelDir * availableTorque * accelSpeed, transform.position);
            }
            else if (movement < 0f)
            {
                // If input vector is negative, add deceleration force to the wheel based on torque and speed variables
                float carSpeed = carRb.linearVelocity.magnitude;
                float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carTopSpeed);
                float availableTorque = torqueCurve.Evaluate(normalizedSpeed) * movement;

                carRb.AddForceAtPosition(accelDir * availableTorque * decelSpeed, transform.position);
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
        float steering = racerController.SteeringValue();

        if (steering > 0)
        {
            turnRot += rotSpeed * Time.deltaTime;
            turning = true;
        }
        else if (steering < 0)
        {
            turnRot -= rotSpeed * Time.deltaTime;
            turning = true;
        }
        else
        {
            turning = false;
        }

        float minRot = 0 - 30 * Mathf.Abs(steering);
        float maxRot = 0 + 30 * Mathf.Abs(steering);
        turnRot = Mathf.Clamp(turnRot, minRot, maxRot);
        transform.localRotation = Quaternion.Euler(0, turnRot, 0);
    }

    //private void Handbrake()
    //{
    //    float braking = playerInputActions.Player.Brake.ReadValue<float>();

    //    if (braking > 0)
    //    {
    //        // If grounded, add a braking force in the opposite direction to current velocity
    //        carRb.AddForceAtPosition(Vector3.Normalize(-carRb.GetPointVelocity(transform.position)) * brakeForce, transform.position);
    //    }
    //}

    private void WheelVisuals()
    {
        float steering = racerController.SteeringValue();
        Vector3 currentPos = wheelVisual.position;

        if (wheelVisualRayHit)
        {
            // If grounded, make the wheel sit on top of the surface
            wheelVisual.position = -transform.up * visualOffset + hit.point;
        }
        else if (!wheelVisualRayHit && Vector3.Distance(wheelVisual.localPosition, Vector3.zero) > 0.05f)
        {
            // If in the air, move the wheels position back to its resting place
            Vector3 targetDir = (wheelVisual.localPosition - Vector3.zero).normalized;
            wheelVisual.localPosition -= returnSpeed * Time.deltaTime * targetDir;
        }
        else
            wheelVisual.localPosition = Vector3.zero;


        visualRotSpd = carRb.GetPointVelocity(transform.position).magnitude / carTopSpeed;
        visualRot += visualRotSpd * Time.deltaTime * visualRotMultiplier;
        if (wheelVisualRayHit)
        {
            // If grounded, spin the wheel depending on velocity direction
            if (Vector3.Dot(transform.forward, Vector3.Normalize(carRb.GetPointVelocity(transform.position))) > 0)
                wheelVisual.localRotation = Quaternion.Euler(visualRot, 0, 0);
            else if (Vector3.Dot(transform.forward, Vector3.Normalize(carRb.GetPointVelocity(transform.position))) < 0)
                wheelVisual.localRotation = Quaternion.Euler(-visualRot, 0, 0);
            else
                wheelVisual.localRotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            // If in the air, spin the wheel depending on player input
            if (steering > 0)
                wheelVisual.localRotation = Quaternion.Euler(visualRot, 0, 0);
            else if (steering < 0)
                wheelVisual.localRotation = Quaternion.Euler(-visualRot, 0, 0);
        }
    }
}
