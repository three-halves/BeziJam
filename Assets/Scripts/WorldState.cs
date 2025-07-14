using System;
using UnityEngine;

public class WorldState : MonoBehaviour
{
    public static WorldState Instance {get; private set;}

    private bool _blockToggled = false;
    public bool BlockToggled
    {
        get {return _blockToggled;}
        set 
        {
             _blockToggled = value;
             BlockToggledListener?.Invoke(value);
        }
    }

    [SerializeField] int requiredCollectables;
    [NonSerialized] public int collectableCount;

    [SerializeField] private GameObject[] destroyOnFirstCollectable;
    [SerializeField] private GameObject[] setActiveOnRequiredCollectables;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void GetCollectable()
    {
        collectableCount++;
        if (collectableCount == 1)
        {
            foreach (var o in destroyOnFirstCollectable)
                Destroy(o);
        }
        if (collectableCount == requiredCollectables)
        {
            foreach (var o in setActiveOnRequiredCollectables)
                o.SetActive(true);
        }
    }

    public delegate void BlockToggledDelegate(bool b);
    public event BlockToggledDelegate BlockToggledListener;
}