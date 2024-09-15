using UnityEngine;

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
    }
}
