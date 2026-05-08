using UnityEngine;

[CreateAssetMenu(fileName = "NewSkin", menuName = "Skins/SkinData")]
public class SkinData : ScriptableObject
{
    [field: SerializeField] public int SkinID { get; private set; }
    [field: SerializeField] public Sprite Icon{ get; private set; }

    [field: Header("Burger Top")]
    [field: SerializeField] public Mesh BurgerTopMesh {  get; private set; }
    [field: SerializeField] public Material[] BurgerTopMaterial {  get; private set; }

    [field: Header("Burger Down")]
    [field: SerializeField] public Mesh BurgerDownMesh {  get; private set; }
    [field: SerializeField] public Material[] BurgerDownMaterial {  get; private set; }
}
