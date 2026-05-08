using UnityEngine.UI;
using UnityEngine;

public class UiModelViewport : MonoBehaviour
{
    private RawImage _viewport;
    private Camera _modelCam;

    void Start()
    {
        _modelCam = GameObject.FindWithTag("ShopCamera").GetComponent<Camera>();
        _viewport = GetComponent<RawImage>();

        var rect = _viewport.rectTransform.rect;
        float scale = Mathf.Max(1f, Screen.dpi / 160f);

        int texW = Mathf.CeilToInt(rect.width * scale);
        int texH = Mathf.CeilToInt(rect.height * scale);

        var rt = new RenderTexture(texW, texH, 24) { antiAliasing = 4 };
        _modelCam.targetTexture = rt;
        _viewport.texture = rt;
    }
}
