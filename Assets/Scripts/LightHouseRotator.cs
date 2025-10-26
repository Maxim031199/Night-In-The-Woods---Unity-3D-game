
using UnityEngine;

public class LightHouseRotator : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 3.5f;
    
    void Update()
    {
        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
    }
}
