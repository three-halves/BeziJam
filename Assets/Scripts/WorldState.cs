using System;
using System.Collections;
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

    private AudioSource musicSource;

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
            StartMusic();
            foreach (var o in destroyOnFirstCollectable)
                Destroy(o);
        }
        if (collectableCount == requiredCollectables)
        {
            foreach (var o in setActiveOnRequiredCollectables)
                o.SetActive(true);
        }
    }

    private void StartMusic()
    {
        musicSource = GameObject.Find("Music").GetComponent<AudioSource>();
        musicSource.Play();
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn() 
    {
        yield return new WaitForSeconds(5f);
        float timer = 0;

        while (musicSource.volume < 1) 
        {
            musicSource.volume = Mathf.Lerp(0, 1, timer / 5f);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    public delegate void BlockToggledDelegate(bool b);
    public event BlockToggledDelegate BlockToggledListener;
}