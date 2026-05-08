using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class GridAutoScaler : MonoBehaviour
{
    [Header("Reference Resolution")]
    public Vector2 referenceResolution = new Vector2(1920, 1080);

    private RectTransform rectTransform;
    private Vector3 initialScale;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        initialScale = transform.localScale;
        UpdateScaling();
    }

    void Update()
    {
        // Обновляем только при изменении разрешения
        if (Screen.width != rectTransform.rect.width ||
            Screen.height != rectTransform.rect.height)
        {
            UpdateScaling();
        }
    }

    void UpdateScaling()
    {
        // Рассчитываем масштаб
        float widthRatio = Screen.width / referenceResolution.x;
        float heightRatio = Screen.height / referenceResolution.y;
        float scaleFactor = Mathf.Min(widthRatio, heightRatio);

        // Применяем масштаб
        transform.localScale = initialScale * scaleFactor;
    }
}
