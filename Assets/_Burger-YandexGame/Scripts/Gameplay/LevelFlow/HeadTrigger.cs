using UnityEngine;

public class HeadTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.transform == GameManager.Instance.Player.BurgerTop)
        {
            GameManager.Instance.FinalGame();
        }

        other.gameObject.SetActive(false);
    }
}
