using UnityEngine;

public class ObtainKey : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        var unlock = other.GetComponent<PlayerUnlockState>();
        if (unlock != null)
        {
            unlock.GrantKey();
            Destroy(gameObject);
        }
    }
}
