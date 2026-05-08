using UnityEngine;
using UnityEngine.UI;

public class AutoScaler : MonoBehaviour
{
    public RectTransform contentRoot; // Объект с ContentSizeFitter + VerticalLayoutGroup
    public Canvas canvas;
    public float minScale = 0.5f;
    public float maxScale = 1f;

    void Update()
    {
        // Получаем высоту контента
        float contentHeight = contentRoot.rect.height;

        // Получаем высоту экрана в пикселях UI
        float screenHeight = ((RectTransform)canvas.transform).rect.height;

        // Если выходит за экран — масштабируем
        float scale = Mathf.Clamp(screenHeight / contentHeight, minScale, maxScale);
        contentRoot.localScale = new Vector3(scale, scale, 1);
    }
}
