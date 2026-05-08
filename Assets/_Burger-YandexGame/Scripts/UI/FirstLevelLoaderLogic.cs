using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstLevelLoaderLogic : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    public static FirstLevelLoaderLogic instance;

    void Awake()
    {
        panel.SetActive(true);
        instance = this;
    }

    public void DeletePanel()
    {
        Destroy(this.gameObject);
    }
}
