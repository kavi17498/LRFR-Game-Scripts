using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeUIManager : MonoBehaviour
{
    public void PlayBtnClciked()
    {
        AdsManager.Instance.ShowInterstitial(() =>
        {
            SceneManager.LoadScene("Gameplay");
        });
    }

    public void LevelsBtnClicked()
    {
        SceneManager.LoadScene("Levels");
    }
}
