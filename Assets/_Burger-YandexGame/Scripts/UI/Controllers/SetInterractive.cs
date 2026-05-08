using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SetInterractive : MonoBehaviour
{
    void OnEnable()
    {
        this.gameObject.GetComponent<Button>().interactable = true;
    }
}
