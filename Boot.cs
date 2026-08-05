using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    [SerializeField] private string welcomeSceneName = "Welcome";

    private void Start()
    {
        SceneManager.LoadScene(welcomeSceneName);
    }
}