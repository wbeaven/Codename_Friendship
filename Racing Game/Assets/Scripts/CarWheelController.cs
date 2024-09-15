using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

public class CarWheelController : MonoBehaviour
{
    // General variables
    [SerializeField] float wheelRadius = 0f;
    [SerializeField] Transform wheelBottom;
    [SerializeField] Transform springTop;
    [SerializeField] LayerMask groundLayer;
    private Rigidbody carRigidbody;
    private bool rayHit;
    private float hitDistance;
    //private LayerMask groundLayer;

    // Suspension variables
    //[SerializeField] Transform restPos
    [SerializeField] float springStrength = 100f;
    [SerializeField] float springDamping = 10f;
    /*[SerializeField] */float restDistance;

    // Cast a ray from the wheel's pos straight down. Set the length to how big the wheel is. If the ray hits, the wheel is close enough to the ground to apply forces to.
    // This is so the wheel isn't applying force to the car when it's midair.

    private void Start()
    {
        carRigidbody = GameObject.Find("Car").GetComponent<Rigidbody>();
        //groundLayer = LayerMask.NameToLayer("Ground");
        restDistance = Vector3.Distance(springTop.position, transform.position);
    }

    private void Update()
    {

        //Ray wheelRay = new(transform.position, new Vector3(transform.position.x, transform.position.y - wheelRadius, transform.position.z));
        //if (Physics.Raycast(wheelRay, out hit, wheelRadius))
        //{
        //    rayHit = true;
        //    hitDistance = hit.distance;
        //}
        //else
        //    rayHit = false;

        //print(name + "'s Offset: " + (restDistance - hitDistance));
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, wheelBottom.position, out hit, wheelRadius, groundLayer))
        {
            print(name + "'s ray is hitting");
            rayHit = true;
            hitDistance = hit.distance;
        }
        else
            rayHit = false;

        Suspension();
    }


    private void Suspension()
    {
        if(rayHit)
        {
            Vector3 springDir = transform.up;
            Vector3 wheelVel = carRigidbody.GetPointVelocity(transform.position);
            float offset = restDistance - hitDistance;
            float velocity = Vector3.Dot(springDir, wheelVel);
            float force = (offset * springStrength) - (velocity * springDamping);

            carRigidbody.AddForceAtPosition(springDir * force, transform.position);
        }
    }

    private void Steering()
    {

    }
    
    private void Acceleration()
    {

    }

    private void CombinedForces()
    {

    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - wheelRadius, transform.position.z));
        Gizmos.DrawLine(transform.position, wheelBottom.position);
        Gizmos.DrawWireSphere(springTop.position, 0.05f);
        //Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y + (restDistance - hitDistance), transform.position.z), 0.05f);
        Gizmos.DrawWireSphere(transform.up * (restDistance - hitDistance), 0.05f);
        
    }
}