using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    public void OnSubmit()
    {
        gameObject.GetComponent<AudioSource>().Play();
        StartCoroutine(Helper());
    }

    private IEnumerator Helper()
    {
        yield return new WaitForSeconds(0.25f);
        SceneManager.LoadScene("Main");
    }
}
