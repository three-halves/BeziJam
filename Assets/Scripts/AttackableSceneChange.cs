using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class AttackableSceneChange : AttackableBase
{
    [SerializeField] private Collider _collider;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite unhighlightedSprite;
    [SerializeField] private Sprite highlightedSprite;
    [SerializeField] private Sprite hitSprite;
    [SerializeField] private string _scene;
    [SerializeField] private float _delay;

    public override void Start()
    {
        base.Start();
    }

    public override void OnAttacked(Player attacker)
    {
        base.OnAttacked(attacker);
        attacker.GetComponent<PlayerInput>().enabled = false;
        _collider.enabled = false;
        _spriteRenderer.sprite = hitSprite;
        StartCoroutine(SwitchSceneWithDelay());
    }

    public IEnumerator SwitchSceneWithDelay()
    {
        yield return new WaitForSeconds(_delay);
        SceneManager.LoadScene(_scene);
    }

    public override void SetHighlight(bool highlighted)
    {
        if (isHighlighted == highlighted) return;
        isHighlighted = highlighted;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (!_collider.enabled) return;
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