using DG.Tweening;
using UnityEngine;

public class Trap : MonoBehaviour
{
    [field: SerializeField] public int RemoveIngredient {  get; private set; }
    [field: SerializeField] public bool IsActive { get; private set; } = true;
    [SerializeField] protected float rotateSpeed;
}
