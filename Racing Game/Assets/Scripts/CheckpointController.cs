using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    public static Transform[] originalCheckpoints { get; private set; } 

    private void Awake()
    {
        int childCount = transform.childCount;
        originalCheckpoints = new Transform[childCount];

        for (int i = 0; i < childCount; i++)
        {
            originalCheckpoints[i] = transform.GetChild(i);
        }
    }
}
