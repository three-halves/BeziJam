using UnityEngine;

public class AttackableCheckpoint : AttackableBase
{
    [SerializeField] private Collider _collider;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite unhighlightedSprite;
    [SerializeField] private Sprite highlightedSprite;
    [SerializeField] private Sprite hitSprite;
    private float lastHitTimer;

    public override void Start()
    {
        base.Start();
    }

    public void Update()
    {
        lastHitTimer += Time.deltaTime;
        if (lastHitTimer > 1f)
        {
            _collider.enabled = true;
        }
        UpdateSprite();
    }

    public override void OnAttacked(Player attacker)
    {
        base.OnAttacked(attacker);
        lastHitTimer = 0f;
        attacker.SetSpawn(attacker.transform.position, attacker.transform.forward);
        _collider.enabled = false;
    }

    public override void SetHighlight(bool highlighted)
    {
        if (isHighlighted == highlighted) return;
        isHighlighted = highlighted;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (lastHitTimer < 0.5f)
        {
            _spriteRenderer.sprite = hitSprite;
        }
        else if (isHighlighted)
        {
            _spriteRenderer.sprite = highlightedSprite;
        }
        else
        {
            _spriteRenderer.sprite = unhighlightedSprite;
        }
    }
}