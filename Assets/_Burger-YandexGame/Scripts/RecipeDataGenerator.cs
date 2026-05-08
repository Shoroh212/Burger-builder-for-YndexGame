#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public static class RecipeDataGenerator
{
    [MenuItem("Tools/Generate RecipeData from Scene Ingredients")]
    public static void Generate()
    {
        string outputDir = "Assets/Resources/Recipe/";
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        var buildScenes = EditorBuildSettings.scenes;

        foreach (var scene in buildScenes)
        {
            if (!scene.enabled) continue;

            string scenePath = scene.path;
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            Ingredient[] allIngredients = GameObject.FindObjectsOfType<Ingredient>();

            // Убираем дубликаты по Icon (визуальные копии не учитываются)
            var uniqueIngredients = allIngredients
                .Where(i => i.PrefabReference != null && i.Icon != null)
                .GroupBy(i => i.Icon)
                .Select(g => g.First())
                .ToList();

            if (uniqueIngredients.Count == 0)
            {
                Debug.LogWarning($"Scene {sceneName} не содержит уникальных ингредиентов с заполненными PrefabReference и Icon");
                continue;
            }

            int countToPick = Random.Range(2, Mathf.Min(6, uniqueIngredients.Count) + 1);
            var selected = uniqueIngredients.OrderBy(_ => Random.value).Take(countToPick).ToList();

            var recipeList = new List<Ingredient>();

            foreach (var ing in selected)
            {
                var prefab = (Ingredient)PrefabUtility.GetCorrespondingObjectFromSource(ing);
                if (prefab != null && AssetDatabase.Contains(prefab))
                {
                    recipeList.Add(prefab);
                }
            }


            // Создание и заполнение ScriptableObject
            RecipeData recipeData = ScriptableObject.CreateInstance<RecipeData>();
            recipeData.RecipeIngredients = recipeList;

            string assetPath = Path.Combine(outputDir, sceneName + ".asset");

            if (File.Exists(assetPath))
                AssetDatabase.DeleteAsset(assetPath);

            AssetDatabase.CreateAsset(recipeData, assetPath);
            Debug.Log($"[{sceneName}] создан RecipeData с {recipeList.Count} уникальными ингредиентами.");
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        Debug.Log("✅ Генерация RecipeData завершена.");
    }
}

#endif