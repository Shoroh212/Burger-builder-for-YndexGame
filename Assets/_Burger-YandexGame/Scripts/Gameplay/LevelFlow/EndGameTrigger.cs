using UnityEngine;

public class EndGameTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Rigidbody rb))
        {
            Vector3 velocity = new Vector3(0, 0, 0);

            Vector3 worldVelocity = transform.TransformDirection(velocity);
            rb.velocity = worldVelocity;
            rb.angularVelocity = velocity;
            
            GameManager.Instance.StopGame();
        }
    }
}
