using System.Collections;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cutscene : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(CutsceneWait());
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadSceneAsync(2);
        }
    }
    private IEnumerator CutsceneWait()
    {
        yield return new WaitForSeconds(38.5f);
        SceneManager.LoadSceneAsync(2);
    }
}