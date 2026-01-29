using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using Unity.Collections;

public class Menu : MonoBehaviour
{
    public Scene SceneNow;
    public int SceneNum;
    void Start()
    {
        SceneNow = SceneManager.GetActiveScene();
        SceneNum = SceneNow.buildIndex;
    }
    public void MainMenu()
    {
        SceneManager.LoadSceneAsync(0);
        SceneNum = SceneNow.buildIndex;
    }
    public void StartGame()
    {
        SceneManager.LoadSceneAsync(1);
        SceneNum = SceneNow.buildIndex;
    }
}
