using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public Button levelButton;
    public Sprite YelloImg;
    public Sprite OneStarSprite;

    public void changebg()
    {
        levelButton.image.sprite = YelloImg;
        levelButton.image.SetNativeSize();
    }
}
