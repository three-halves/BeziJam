using UnityEngine;

public class MatchRotation : MonoBehaviour
{
    [SerializeField] Transform transformToMatch;
    [SerializeField] private float scalingFactor = 10f;

    // Update is called once per frame
    void Update()
    {
        transform.forward = transformToMatch.forward;
        transform.position = transformToMatch.position /= scalingFactor;
    }
}
