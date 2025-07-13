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
        float lastTime = reappearTimer;
        reappearTimer -= Time.deltaTime;
        _collider.enabled = reappearTimer < 0;
        _spriteRenderer.enabled = reappearTimer < _reappearTime / 1.25f;
        if (reappearTimer > 0)
        {
            _spriteRenderer.sprite = usedSprite; 
        }
        else if (reappearTimer < 0 && lastTime > 0) UpdateSprite();
    }

    public override void OnAttacked(Player attacker)
    {
        base.OnAttacked(attacker);
        attacker.ApplyForce(Vector3.Scale(attacker.GetLookVector(), _dashSpeed));
        attacker.DisableGravityForSeconds(_disableGravTime);
        reappearTimer = _reappearTime;
        SetHighlight(false);
    }

    public override void SetHighlight(bool highlighted)
    {
        if (isHighlighted == highlighted) return;
        isHighlighted = highlighted;
        if (reappearTimer < 0) UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (isHighlighted)
        {
            _spriteRenderer.sprite = highlightedSprite;
        }
        else
        {
            _spriteRenderer.sprite = unhighlightedSprite;
        }
    }
}