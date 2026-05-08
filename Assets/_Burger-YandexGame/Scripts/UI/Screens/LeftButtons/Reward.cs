using UnityEngine;
using UnityEngine.Events;

public class Reward : MonoBehaviour
{
    [SerializeField] private UnityEvent _claimEvent;

    public void ClaimReward()
    {
        _claimEvent?.Invoke();
    }    

    public void ClaimMoney(int moneyCount)
    {
        //GameManager.Instance.UpdateMoney(moneyCount);
    }
}
