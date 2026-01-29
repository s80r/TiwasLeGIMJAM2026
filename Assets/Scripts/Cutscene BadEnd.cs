using System.Collections;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneBadEnd : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(CutsceneWait1());
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadSceneAsync(0);
        }
    }
    private IEnumerator CutsceneWait1()
    {
        yield return new WaitForSeconds(47.0f);
        SceneManager.LoadSceneAsync(0);
    }
}