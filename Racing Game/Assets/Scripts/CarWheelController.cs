using UnityEngine;

public class CarWheelController : MonoBehaviour
{
    // Suspension variables
    [SerializeField] float springStrength = 100f;
    [SerializeField] float springDamping = 10f;

    // Cast a ray from the wheel's pos straight down. Set the length to how big the wheel is. If the ray hits, the wheel is close enough to the ground to apply forces to.
    // This is so the wheel isn't applying force to the car when it's midair.

    private void Update()
    {
        
    }


    private void Suspension()
    {

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
}