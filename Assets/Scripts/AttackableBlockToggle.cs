using UnityEngine;

public class AttackableBlockToggle : AttackableBase
{
    [SerializeField] private Collider _collider;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite unhighlightedOnSprite;
    [SerializeField] private Sprite highlightedOnSprite;
    [SerializeField] private Sprite unhighlightedOffSprite;
    [SerializeField] private Sprite highlightedOffSprite;
    private float lastHitTimer;

    public override void Start()
    {
        base.Start();
        WorldState.Instance.BlockToggledListener += OnBlockToggled;
    }

    public void Update()
    {
        lastHitTimer += Time.deltaTime;
        if (lastHitTimer > 1f)
        {
            _collider.enabled = true;
        }
    }

    private void OnBlockToggled(bool b)
    {
        UpdateSprite();
    }

    public override void OnAttacked(Player attacker)
    {
        base.OnAttacked(attacker);
        lastHitTimer = 0f;
        WorldState.Instance.BlockToggled = !WorldState.Instance.BlockToggled;
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
        if (isHighlighted)
        {
            _spriteRenderer.sprite = WorldState.Instance.BlockToggled ? highlightedOnSprite : highlightedOffSprite;
        }
        else
        {
            _spriteRenderer.sprite = WorldState.Instance.BlockToggled ? unhighlightedOnSprite : unhighlightedOffSprite;
        }
    }
}