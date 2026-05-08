using UnityEngine;
using UnityEngine.UI;
using YG;

public class YAData : MonoBehaviour
{
    //[SerializeField] private GameObject[] portObjects;
    [SerializeField] private Canvas _portraitCanvas;

    void Start()
    {
        SetEnvironment();
    }

    //void SetEnvironment()
    //{
    //    switch (YG2.envir.device)
    //    {
    //        case YG2.Device.Desktop:
    //            for (int i = 0; i < portObjects.Length; i++)
    //            {
    //                portObjects[i].SetActive(false);
    //            }
    //            for (int i = 0; i < deskObjects.Length; i++)
    //            {
    //                deskObjects[i].SetActive(true);
    //            }
    //            break;
    //        case YG2.Device.Mobile:
    //            if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
    //            {

    //                for (int i = 0; i < portObjects.Length; i++)
    //                {
    //                    portObjects[i].SetActive(true);
    //                }
    //                for (int i = 0; i < deskObjects.Length; i++)
    //                {
    //                    deskObjects[i].SetActive(false);
    //                }
    //                break;
    //            }
    //            else
    //            {
    //                for (int i = 0; i < portObjects.Length; i++)
    //                {
    //                    portObjects[i].SetActive(false);
    //                }
    //                for (int i = 0; i < deskObjects.Length; i++)
    //                {
    //                    deskObjects[i].SetActive(true);
    //                }
    //                break;
    //            }
    //    }
    //}


    void SetEnvironment()
    {
        GameManager.Instance.UIController = FindAnyObjectByType<UIController>();

        switch (YandexGame.EnvironmentData.deviceType)
        {
            case "desktop":
                break;

            case "mobile":
                if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
                {
                    Destroy(GameManager.Instance.UIController.gameObject);

                    GameManager.Instance.UIController = Instantiate(_portraitCanvas).GetComponent<UIController>();
                }
                break;

        }
    }

}
