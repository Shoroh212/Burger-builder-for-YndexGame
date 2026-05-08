using UnityEngine;

public class TrapShovel : Trap
{
    [SerializeField] private Transform _spawner;
    [SerializeField] private GameObject _dust;
    [SerializeField] private float _timeToDestroy;


    public void SpawnDust()
    {
        if(!GameManager.Instance.BonusLevelEnabled)
        {
            GameObject dust = Instantiate(_dust, _spawner.position, _spawner.rotation);
            dust.transform.parent = transform;
            Destroy(dust, _timeToDestroy);
        }
    }
}
