using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
    [SerializeField] private Player _player;

    private BoxCollider _collider;

    private void Start()
    {
        _collider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        _collider.enabled = false;
        print("Player Triggered");
        Invoke(nameof(ResetCollider), 0.1f);
    }

    private void ResetCollider()
    {
        _collider.enabled = true;
    }
}
