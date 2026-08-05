using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeUIManager : MonoBehaviour
{
    public void PlayBtnClciked()
    {
        SceneManager.LoadScene("Gameplay");
    }
}
