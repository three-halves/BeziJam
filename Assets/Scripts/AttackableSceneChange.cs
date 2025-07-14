using UnityEngine;
using UnityEngine.SceneManagement;

public class AttackableSceneChange : AttackableBase
{
    [SerializeField] private Collider _collider;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite unhighlightedSprite;
    [SerializeField] private Sprite highlightedSprite;
    [SerializeField] private Sprite hitSprite;
    [SerializeField] private string _scene;

    public override void Start()
    {
        base.Start();
    }

    public override void OnAttacked(Player attacker)
    {
        base.OnAttacked(attacker);
        SceneManager.LoadScene(_scene);
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
            _spriteRenderer.sprite = highlightedSprite;
        }
        else
        {
            _spriteRenderer.sprite = unhighlightedSprite;
        }
    }
}