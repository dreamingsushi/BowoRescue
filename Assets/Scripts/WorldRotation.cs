using UnityEngine;

public class WorldRotation : MonoBehaviour
{
    public float rotationSpeed;

    void Update()
    {
         transform.Rotate(rotationSpeed * Time.deltaTime, 0f, 0f);
    }
}
