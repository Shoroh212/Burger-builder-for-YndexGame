using UnityEngine;

public class TrapKnife : Trap
{
    [SerializeField] private float _timeToDestroy;
    [SerializeField] private float _speed;

    private void Start()
    {
        Destroy(gameObject, _timeToDestroy);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * _speed);
    }   
}