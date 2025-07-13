using UnityEngine;

public class AttackableBase : MonoBehaviour
{
    protected AudioSource audioSource;
    [SerializeField] protected Vector2 audioPitchRange;
    protected bool isHighlighted = true;
    public virtual void Start()
    {
        audioSource = GetComponent<AudioSource>();
        SetHighlight(false);
    }

    public virtual void OnAttacked(Player attacker)
    {
        if (audioSource)
        {
            audioSource.pitch = Random.Range(audioPitchRange.x, audioPitchRange.y);
            audioSource.Play();
        }
    }

    public virtual void SetHighlight(bool highlight)
    {

    }
}