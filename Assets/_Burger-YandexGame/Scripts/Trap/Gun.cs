using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private float _timeToSpawn;
    [SerializeField] private GameObject _shootPrefab; 
    [SerializeField] private Transform _spawner; 

    private void Start()
    {
        StartCoroutine(Spawn());
    }

    private void SpawnPrefab()
    {
        GameObject shootPrefab = Instantiate(_shootPrefab, _spawner.position, _spawner.rotation, transform);
    }

    private IEnumerator Spawn()
    {
        while(true)
        {
            yield return new WaitForSeconds(_timeToSpawn);
            SpawnPrefab();
        }
    }
}