using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public float lifetime = 2.0f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}