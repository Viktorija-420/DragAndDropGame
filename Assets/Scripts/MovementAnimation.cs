using UnityEngine;

public class MoveBetweenPoints : MonoBehaviour
{
    [Header("Movement Points")]
    public Transform pointA;   // Start position
    public Transform pointB;   // End position

    [Header("Movement Settings")]
    public float speed = 2f;   // Units per second
    public bool loop = true;   // Should it go back and forth?

    private Vector3 target;

    void Start()
    {
        if (pointA != null && pointB != null)
        {
            transform.position = pointA.position; // Start at point A
            target = pointB.position;             // First target is B
        }
    }

    void Update()
    {
        if (pointA == null || pointB == null) return;

        // Move toward the target
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // If we reached the target, switch target
        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            if (loop)
            {
                target = (target == pointA.position) ? pointB.position : pointA.position;
            }
            else
            {
                enabled = false; // Stop script after reaching B
            }
        }
    }
}
