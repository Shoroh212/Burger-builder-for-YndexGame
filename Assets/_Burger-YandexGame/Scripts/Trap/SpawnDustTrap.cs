using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnDustTrap : MonoBehaviour
{
    [SerializeField] private Transform _spawner;
    [SerializeField] private GameObject _dust;
    [SerializeField] private float _timeToDestroy;

    public void SpawnDust()
    {
        if(!GameManager.Instance.BonusLevelEnabled)
        {
            GameObject dust = Instantiate(_dust, _spawner.position, _spawner.rotation, transform);
            Destroy(dust, _timeToDestroy);
        }
    }
}
