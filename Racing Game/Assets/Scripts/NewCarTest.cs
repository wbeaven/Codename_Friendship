using UnityEngine;
using UnityEngine.InputSystem;

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

    void Start()
    {
        
    }

    void Update()
    {
    }

    private void FixedUpdate()
    {
        WheelRaycast();

        if(wheelRayHit)
        {
            Spring();
            SteeringPhysics();
        }
        
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

    public void SteeringControls(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            print("Controls technically working" + context.phase);
            Vector2 movement = context.ReadValue<Vector2>();
            print(movement.x + ", " + movement.y);


            if (movement.x > 0)
            {
                // turn wheels 30 degrees to the right
                print("turning right");
            }
            else if (movement.x < 0)
            {
                // turn wheels 30 degrees to the left
                print("turning left");

            }
            
        }
        else
        {
            // turn wheels straight
            print("going straight");
        }
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

        //carRb.GetPointVelocity(transform.position).normalized
        //if (carRb.GetPointVelocity(transform.position).)
        //wheelVisual.Rotate(new Vector3(0, visualRotSpd * carRb.GetPointVelocity(transform.position).magnitude * Time.deltaTime, 0));
    }
}
