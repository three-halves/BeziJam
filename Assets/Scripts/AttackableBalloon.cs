using UnityEngine;

public class AttackableBalloon : AttackableBase
{
    [SerializeField] private Vector3 _dashSpeed;
    [SerializeField] private float _reappearTime;
    [SerializeField] private Collider _collider;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private float reappearTimer;

    public override void Start()
    {
        base.Start();
    }

    public void Update()
    {
        reappearTimer -= Time.deltaTime;
        _collider.enabled = reappearTimer < 0;
        _spriteRenderer.enabled = reappearTimer < 0;
    }

    public override void OnAttacked(Player attacker)
    {
        attacker.ApplyForce(Vector3.Scale(attacker.GetLookVector(), _dashSpeed));
        reappearTimer = _reappearTime;
    }

    public override void SetHighlight(bool highlighted)
    {
        if (isHighlighted == highlighted) return;
        isHighlighted = highlighted;

        if (highlighted)
        {
            _spriteRenderer.color = Color.white;
        }
        else
        {
            _spriteRenderer.color = Color.gray;
        }
    }
}