using UnityEngine;

public class Destroyer : MonoBehaviour
{
    private const float DestroyDelaySeconds = 10f;

    void Start()
    {
        Destroy(gameObject, DestroyDelaySeconds);
    }
}
