using UnityEngine;

public class FortuneWheelDetector : MonoBehaviour
{
    public bool IsStoped {  get; set; }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out FortuneWheelAward fortuneWheelAward))
        {
            if(IsStoped)
            {
                fortuneWheelAward.GetAward();
                IsStoped = false;
            }
        }
    }
}
