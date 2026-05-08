using UnityEngine;

[CreateAssetMenu(fileName = "NewBlockSkin", menuName = "Skins/Block Skin")]
public class BlockSkinData : ScriptableObject
{
    [field: SerializeField] public int SkinID { get; private set; }
    [field: SerializeField] public string SkinName { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }

    [field: SerializeField] public Color One { get; private set; }
    [field: SerializeField] public Color Two { get; private set; }

}
