using UnityEngine;
using UnityEngine.SceneManagement;

public class WinController : MonoBehaviour
{
    public void RestartGame()
    {
        SceneManager.LoadScene("MainScene");
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