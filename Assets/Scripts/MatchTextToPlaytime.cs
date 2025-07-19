using TMPro;
using UnityEngine;
using System;

public class MatchTextToPlaytime : MonoBehaviour
{
    [SerializeField] private int _digits = 4;
    private int _factor;
    private string _format;
    private TextMeshProUGUI _textMesh;

    void Start()
    {
        _factor = (int)Math.Pow(10, _digits);
        _format = "00." + new string('0', _digits);

        _textMesh = GetComponent<TextMeshProUGUI>();
        _textMesh.maxVisibleCharacters = _digits + 6;
    }

    void Update()
    {
        _textMesh.text = Mathf.Floor(WorldState.PlayTime / 60f).ToString("00") 
        + ":" + (WorldState.PlayTime % 60f).ToString(_format);
    }
}