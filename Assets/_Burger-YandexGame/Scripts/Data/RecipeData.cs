using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RecipeData", menuName = "Recipe/RecipeData", order = 1)]
public class RecipeData : ScriptableObject
{
    [field: SerializeField] public List<Ingredient> RecipeIngredients { get;  set; }
}
