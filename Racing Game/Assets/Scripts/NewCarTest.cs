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
    public float visualRotSpd;

    public float wheelMass;
    [Range (0f, 1f)]
    public float tyreGrip;

    private PlayerInput playerInput;
    private InputSystem_Actions playerInputActions;

    public bool turnable;
    public float rotation;
    public float rotSpeed = 1f;

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

    private void SteeringControls()
    {
        Vector2 movement = playerInputActions.Player.Move.ReadValue<Vector2>();

        if (movement.x > 0)
        {
            // turn wheels 30 degrees to the right
            rotation += rotSpeed * Time.deltaTime;
        }
        else if (movement.x < 0)
        {
            // turn wheels 30 degrees to the left
            rotation -= rotSpeed * Time.deltaTime;
        }

        float minRot = 0 - 30 * Mathf.Abs(movement.x);
        float maxRot = 0 + 30 * Mathf.Abs(movement.x);
        rotation = Mathf.Clamp(rotation, minRot, maxRot);
        transform.localRotation = Quaternion.Euler(0, rotation, 0);
    }

    private void WheelVisuals()
    {
        if (wheelRayHit)
        {
            wheelVisual.position = new Vector3(wheelVisual.position.x, hit.point.y + visualOffset, wheelVisual.position.z);
        }
        else
        {
            wheelVisual.position = new Vector3(0, 0, 0);    
        }

        // Some code trying to get the wheel to spin based on its speed

        //carRb.GetPointVelocity(transform.position).normalized
        //if (carRb.GetPointVelocity(transform.position).)
        //wheelVisual.Rotate(new Vector3(0, visualRotSpd * carRb.GetPointVelocity(transform.position).magnitude * Time.deltaTime, 0));
    }
}
