using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImageSlider : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private Image fillImage;
    [SerializeField] private RectTransform sliderArea;
    [SerializeField] private float maxValue = 1f;

    [SerializeField] private float value = 1f;

    public UnityEvent<float> OnValueChanged;

    public float Value
    {
        get => value;
        private set
        {
            float clamped = Mathf.Clamp(value, 0, maxValue);
            if(!Mathf.Approximately(this.value, clamped))
            {
                this.value = clamped;
                fillImage.fillAmount = clamped / maxValue;
                OnValueChanged?.Invoke(clamped);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateSlider(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateSlider(eventData);
    }

    private void UpdateSlider(PointerEventData eventData)
    {
        if(sliderArea == null) return;

        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(sliderArea, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            float width = sliderArea.rect.width;
            float normalized = Mathf.Clamp01((localPoint.x + width * 0.5f) / width);
            float newValue = normalized * maxValue;
            Value = newValue;
        }
    }

    // Метод для установки значения извне
    public void SetValue(float newValue)
    {
        Value = newValue;
    }
}