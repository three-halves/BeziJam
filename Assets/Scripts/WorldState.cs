using System;

public class WorldState 
{
    private static WorldState _instance = null;

    public static WorldState Instance
    {
        get
        {
            _instance ??= new WorldState();
            return _instance;
        }
    }

    private bool _blockToggled = false;
    public bool BlockToggled
    {
        get {return _blockToggled;}
        set 
        {
             _blockToggled = value;
             BlockToggledListener.Invoke(value);
        }
    }

    public delegate void BlockToggledDelegate(bool b);
    public event BlockToggledDelegate BlockToggledListener;
}