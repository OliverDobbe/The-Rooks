using UnityEngine;

public class AbilityUnlockTrigger : MonoBehaviour
{
    [Header("Abilities to Unlock")]
    public bool unlockDoubleJump = false;
    public bool unlockDash = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            if (unlockDoubleJump)
                player.enableDoubleJump = true;

            if (unlockDash)
                player.enableDash = true;

            Destroy(gameObject);
        }
    }
}
