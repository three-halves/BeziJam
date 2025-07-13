using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PauseMenu : MonoBehaviour
{
    private PlayerInput playerInput;
    [SerializeField] private PlayerPrefSetter[] prefSetters;
    void Start()
    {
        gameObject.SetActive(false);
        playerInput = FindFirstObjectByType<PlayerInput>();
    }
    void OnEnable()
    {
        Time.timeScale = 0f;
        if (playerInput) playerInput.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Array.ForEach(prefSetters, pref => pref.Sync());
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
        if (playerInput) playerInput.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
    }
}