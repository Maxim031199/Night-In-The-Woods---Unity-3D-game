using UnityEngine;

public class UIScale : MonoBehaviour
{
    private const int FullHDWidth = 1920;
    private const float DefaultScale = 1f;

    [SerializeField] private float scaleValue = DefaultScale;
    [SerializeField] private float UHDScale = 2f;

    void Start()
    {
        if (Screen.width > FullHDWidth)
        {
            scaleValue = UHDScale;
        }

        transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);

    }
}