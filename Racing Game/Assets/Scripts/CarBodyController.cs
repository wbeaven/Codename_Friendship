using System.Collections.Generic;
using UnityEngine;

public class CarBodyController : MonoBehaviour
{    
    [SerializeField] List<CarController> wheels;
    [SerializeField] float angularDamping = 5f;
    private Rigidbody carRb;

    private void Awake()
    {
        carRb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        bool grounded = wheels.Exists(wheel => wheel.grounded);
        bool turning = wheels.Exists(wheel => wheel.turning);

        if (grounded && !turning)
        {
            AlignVelocity();
        }
        print(carRb.angularVelocity);
    }

    void AlignVelocity()
    {
        //carRb.angularVelocity = Vector3.Lerp(carRb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * angularDamping);

        Vector3 angVel = carRb.angularVelocity;
        angVel.y = Mathf.Lerp(angVel.y, 0f, Time.fixedDeltaTime * angularDamping);
        carRb.angularVelocity = angVel;
    }
}

