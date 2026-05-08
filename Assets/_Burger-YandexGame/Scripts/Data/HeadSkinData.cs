using UnityEngine;

[CreateAssetMenu(fileName = "NewHeadSkin", menuName = "Skins/Head Skin")]
public class HeadSkinData : ScriptableObject
{
    [field: SerializeField] public int SkinID { get; private set; }
    [field: SerializeField] public string SkinName { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
}
