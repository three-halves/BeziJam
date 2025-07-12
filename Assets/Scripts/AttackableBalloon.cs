using UnityEngine;

public class AttackableBalloon : AttackableBase
{
    [SerializeField] private Vector3 _dashSpeed;
    [SerializeField] private float _disableGravTime;
    [SerializeField] private float _reappearTime;
    [SerializeField] private Collider _collider;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite unhighlightedSprite;
    [SerializeField] private Sprite highlightedSprite;
    [SerializeField] private Sprite usedSprite;

    private float reappearTimer;

    public override void Start()
    {
        base.Start();
    }

    public void Update()
    {
        reappearTimer -= Time.deltaTime;
        _collider.enabled = reappearTimer < 0;
        _spriteRenderer.enabled = reappearTimer < _reappearTime / 1.25f;
        if (reappearTimer > 0)
        {
            _spriteRenderer.sprite = usedSprite; 
            if (reappearTimer - Time.deltaTime < 0) _spriteRenderer.sprite = unhighlightedSprite;
        }
    }

    public override void OnAttacked(Player attacker)
    {
        attacker.ApplyForce(Vector3.Scale(attacker.GetLookVector(), _dashSpeed));
        attacker.DisableGravityForSeconds(_disableGravTime);
        reappearTimer = _reappearTime;
    }

    public override void SetHighlight(bool highlighted)
    {
        if (isHighlighted == highlighted || reappearTimer > 0) return;
        isHighlighted = highlighted;

        if (highlighted)
        {
            _spriteRenderer.sprite = highlightedSprite;
        }
        else
        {
            _spriteRenderer.sprite = unhighlightedSprite;
        }
    }
}