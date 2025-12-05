using UnityEngine;
using UnityEngine.Tilemaps;

public class SecretWall : MonoBehaviour
{
    public Tilemap tilemap;

    void OnCollisionEnter2D(Collision2D collision)
    {
        var unlock = collision.collider.GetComponent<PlayerUnlockState>();
        if (unlock != null && unlock.hasSecretKey)
            tilemap.ClearAllTiles();
    }
}
