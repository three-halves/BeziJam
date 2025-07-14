using UnityEngine;

public class AppearAtPlayer : MonoBehaviour
{
    [SerializeField] private float forwardOffset;
    [SerializeField] private float YOffset;
    void OnEnable()
    {
        Player player = GameObject.Find("Player").GetComponent<Player>();
        transform.position = player.transform.position + player.GetLookVector() * forwardOffset + Vector3.up * YOffset;
    }
}
