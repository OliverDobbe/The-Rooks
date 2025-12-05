using UnityEngine;

public class PlayerUnlockState : MonoBehaviour
{
    public bool hasSecretKey;

    public void GrantKey()
    {
        hasSecretKey = true;
    }
}
