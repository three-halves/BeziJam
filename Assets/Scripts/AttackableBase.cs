using UnityEngine;

public class AttackableBase : MonoBehaviour
{
    protected bool isHighlighted = true;
    public virtual void Start()
    {
        SetHighlight(false);
    }

    public virtual void OnAttacked(Player attacker)
    {

    }

    public virtual void SetHighlight(bool highlight)
    {

    }
}