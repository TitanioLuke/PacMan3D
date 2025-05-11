using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public GameObject menuInicial;
    public GameObject jogoRoot;

    void Start()
    {
        menuInicial.SetActive(true);
        jogoRoot.SetActive(false);
        Time.timeScale = 0f;
    }

    public void StartGame()
    {
        menuInicial.SetActive(false);
        jogoRoot.SetActive(true);
        Time.timeScale = 1f;
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
