using UnityEngine;

public class AttackableCollectable : AttackableBase
{
    [SerializeField] private Collider _collider;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite unhighlightedSprite;
    [SerializeField] private Sprite highlightedSprite;
    [SerializeField] private ParticleSystem collectParticles;
    [SerializeField] private Sprite[] litAnimation;
    private bool isLit = false;

    public override void Start()
    {
        base.Start();
    }

    public void Update()
    {
        if (isLit)
        {
            _spriteRenderer.sprite = litAnimation[(int)(Time.time * 2 % 2)];
        }
    }

    public override void OnAttacked(Player attacker)
    {
        isLit = true;
        attacker.GetCollectable();
        attacker.SetSpawn(attacker.transform.position);
        _collider.enabled = false;
        collectParticles.Play();
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
            _spriteRenderer.sprite = highlightedSprite;
        }
        else
        {
            _spriteRenderer.sprite = unhighlightedSprite;
        }
    }
}