using UnityEngine;

public class FollowPlayerLateral : MonoBehaviour
{
    private Player _player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(
            _player.transform.position.x, 
            transform.position.y, 
            _player.transform.position.z
        );
    }
}
