using UnityEngine;

public class ToggleGeometry : MonoBehaviour
{
    [SerializeField] private bool startState;
    [SerializeField] private Material onMat;
    [SerializeField] private Material offMat;

    private Collider _collider;
    private Renderer _renderer;

    void Start()
    {
        _collider = GetComponent<Collider>();
        _renderer = GetComponent<Renderer>();
        WorldState.Instance.BlockToggledListener += OnBlockToggled;
        OnBlockToggled(WorldState.Instance.BlockToggled);
    }

    private void OnBlockToggled(bool b)
    {
        bool isOn = b ^ !startState;
        _collider.enabled = isOn;
        _renderer.material = isOn ? onMat : offMat;
    }

}