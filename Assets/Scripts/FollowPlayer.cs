using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    private Player _player;
    [SerializeField] private Vector3 _offset;
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
            _player.transform.position.y, 
            _player.transform.position.z
        ) + _offset;
    }
}
