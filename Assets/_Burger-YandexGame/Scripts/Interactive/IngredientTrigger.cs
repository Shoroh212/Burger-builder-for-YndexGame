using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum IngredientAction
{
    Add,
    Remove,
}

[ExecuteAlways]
public class IngredientTrigger : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private MeshRenderer _background;

    [SerializeField] private Material _blue;
    [SerializeField] private Material _red;

    [SerializeField] private int _amount = 2;
    [SerializeField] private IngredientAction _action = IngredientAction.Add;

    private bool _activate;

    private void OnValidate()
    {
        if (_text == null || _background == null)
            return;

        switch (_action)
        {
            case IngredientAction.Add:
                _text.text = $"+{_amount}";
                _background.material = _blue; 
                break;

            case IngredientAction.Remove:
                _text.text = $"-{_amount}";
                _background.material = _red;

                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_activate)
            return;

        if (!other.TryGetComponent(out Player player))
            return;

        switch (_action)
        {
            case IngredientAction.Add:
                player.CloneAndAddIngredient(_amount);
                print(1);
                break;

            case IngredientAction.Remove:
                player.DeleteIngredient(_amount);
                print(1);

                break;

            //case IngredientAction.Multiply:
            //    player.CloneAndAddIngredient(player.Ingredients.Count * (_amount - 1));
            //    break;
            //case IngredientAction.Subtract:
            //    //player.CloneAndSubtractIngredient(_amount);
            //    break;
        }
    }
}
