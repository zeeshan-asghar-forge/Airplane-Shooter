using UnityEngine;

public class ObjectFollow : MonoBehaviour
{
    public Transform target; // Assign the object to follow

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position;
        }
    }
}
