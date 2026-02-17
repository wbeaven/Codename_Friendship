using System.Collections.Generic;
using UnityEngine;

public class CarBodyController : MonoBehaviour
{    
    [SerializeField] List<CarController> wheels;
    [SerializeField] float angularDamping = 5f;
    [SerializeField] Camera carCam;
    private Rigidbody carRb;
    private float normalizedSpeed;

    private void Awake()
    {
        carRb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Increase FOV with speed
        normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carRb.linearVelocity.magnitude) / 20f);

        if (normalizedSpeed > 0.3f)
        {
            carCam.fieldOfView = 80 + (normalizedSpeed - 0.3f) * 20f / 0.7f;
        }
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

