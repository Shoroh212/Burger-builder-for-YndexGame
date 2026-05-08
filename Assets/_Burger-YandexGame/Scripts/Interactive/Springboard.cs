using UnityEngine;
using DG.Tweening;
using System.Collections;

public class Springboard : MonoBehaviour
{
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpDistance = 5f; 
    [SerializeField] private float jumpDuration = 1f; 

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Player player))
        {
            player.LaunchFromSpringboard(transform.forward, jumpHeight, jumpDistance, jumpDuration); // Новый вызов
        }
    }
}
