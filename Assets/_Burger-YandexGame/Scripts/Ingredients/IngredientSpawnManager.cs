using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IngredientSpawnManager : MonoBehaviour
{
    private List<IngredientSpawner> _spawners;

    //private void Awake()
    //{
    //}

    //private void Start()
    //{
    //    //SpawnIngredients();
    //}

    public void Init()
    {
        _spawners = FindObjectsByType<IngredientSpawner>(FindObjectsSortMode.None)
            .OrderBy(sp => sp.transform.position.sqrMagnitude) // или по x/y/z
            .ToList();

        for(int i = 0; i < _spawners.Count; i++)
        {
            _spawners[i].SpawnerIndex = i;
        }
    }

    public void SpawnIngredients()
    {
        List<Ingredient> ingredientsTotal = new List<Ingredient>();

        foreach (var spawner in _spawners)
        {
            int prefabIndex;

            if (!spawner.TryLoadLastSpawnedIndex(out prefabIndex))
            {
                prefabIndex = Random.Range(0, GameManager.Instance.IngredientPrefabs.Length);
                spawner.SaveLastSpawnedIndex(prefabIndex);
            }

            GameObject prefab = GameManager.Instance.IngredientPrefabs[prefabIndex];
            GameObject spawned = spawner.SpawnIngredient(prefab);

            ingredientsTotal.Add(spawned.GetComponent<Ingredient>());
        }

        GameManager.Instance.TotalIngredientsCount = FindObjectsByType<Ingredient>(FindObjectsSortMode.None).Length;

        GenerateRecipeFrom(ingredientsTotal);
    }

    private void GenerateRecipeFrom(List<Ingredient> ingredients)
    {
        List<Ingredient> uniqueIngredients = ingredients
            .Where(ing => !ing.IsBadIngredient)
            .GroupBy(x => x.Icon)
            .Select(g => g.First())
            .ToList();

        int needCount = Random.Range(3, 8);
        needCount = Mathf.Min(needCount, uniqueIngredients.Count);

        // Перемешиваем
        for (int i = uniqueIngredients.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (uniqueIngredients[i], uniqueIngredients[j]) = (uniqueIngredients[j], uniqueIngredients[i]);
        }

        var selected = uniqueIngredients.Take(needCount);

        GameManager.Instance.Recipe.Clear();
        foreach (var ing in selected)
        {
            GameManager.Instance.Recipe.Add(ing);
        }
    }

    public static void ClearSavedIngredients()
    {
        var spawners = FindObjectsByType<IngredientSpawner>(FindObjectsSortMode.None);
        foreach (var spawner in spawners)
        {
            spawner.ClearSaved();
        }
    }
}
