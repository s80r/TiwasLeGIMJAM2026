using System.Collections;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneGoodEnd : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(CutsceneWait2());
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadSceneAsync(0);
        }
    }
    private IEnumerator CutsceneWait2()
    {
        yield return new WaitForSeconds(37.0f);
        SceneManager.LoadSceneAsync(0);
    }
}