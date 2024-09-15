using UnityEngine;

public class SpringTest : MonoBehaviour
{
    public Rigidbody wheelRigidbody;
    public Transform wheel;

    public float strength;
    public float damping;
    public float restDistance;

    public float offset;
    public float velocity;
    public float force;

    void Start()
    {
        
    }

    void Update()
    {
    }

    private void FixedUpdate()
    {
        Spring();
    }

    private void Spring()
    {
        Vector3 springDir = transform.up;
        Vector3 wheelVel = wheelRigidbody.linearVelocity;
        offset = restDistance - wheel.position.y;
        velocity = Vector3.Dot(springDir, wheelVel);
        force = (offset * strength) - (velocity * damping);

        wheelRigidbody.AddForce(springDir * force);
    }
}
