using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelGeneration : MonoBehaviour
{
    [Header("Level Part")]
    [SerializeField] private GameObject[] _levelSnippets;
    [SerializeField] private GameObject _roadStartPrefab;
    [SerializeField] private GameObject _rotateCubePrefab;
    [SerializeField] private GameObject _finalPart;

    [Header("Reference")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _headPrefab;

    [SerializeField] private int _gameLength;

    private GameObject _parentLevel;

    private List<IngredientSpawner> _ingredientSpawners = new List<IngredientSpawner>();

    private PathProgressBar _progressBar;
    private List<Transform> _pathPoints = new List<Transform>();

    private void Start()
    {
        _progressBar = FindObjectOfType<PathProgressBar>();

        _parentLevel = new GameObject("Level");

        GameObject road = new GameObject("Road");
        road.transform.parent = _parentLevel.transform;

        GameObject startRoad = Instantiate(_roadStartPrefab);
        startRoad.transform.parent = road.transform;

        GameObject startPoint = new GameObject("StartPos");
        startPoint.transform.position = GameManager.Instance.Player.transform.position;
        _pathPoints.Add(startPoint.transform);

        Vector3 roadDirection = Vector3.forward;

        int maxRotateCount = 1;
        int rotateCount = 0;

        Vector3 offset = new Vector3(0, 0, 0);
        int startSnippet = 1;
        int localI = 0;

        for(int i = 0; i < _gameLength; i++)
        {
            Vector3 snippetPosition = offset + roadDirection * localI * 14f;

            if(_gameLength >= 5)
            {
                if(Random.value > 0.5f && rotateCount < maxRotateCount)
                {
                    if(Random.value > 0.5f)
                    {
                        rotateCount++;

                        Quaternion rotation = Quaternion.Euler(0, 180, 0);
                        Vector3 position = snippetPosition + new Vector3(0, -0.494f, 13.5f);

                        GameObject rotateCube = Instantiate(_rotateCubePrefab, position, rotation, road.transform);

                        _pathPoints.Add(rotateCube.transform.GetChild(13));

                        offset = snippetPosition + new Vector3(6.5f, 0f, 19.5f);

                        startSnippet = 0;
                        roadDirection = Vector3.right;
                        localI = 0;
                    }
                    else
                    {
                        rotateCount++;

                        Quaternion rotation = Quaternion.Euler(0, 270, 0);
                        Vector3 position = snippetPosition + new Vector3(-6f, -0.494f, 19.5f);

                        GameObject rotateCube = Instantiate(_rotateCubePrefab, position, rotation, road.transform);

                        _pathPoints.Add(rotateCube.transform.GetChild(13));

                        offset = snippetPosition + new Vector3(-6.5f, 0f, 19.5f);

                        startSnippet = 0;
                        roadDirection = Vector3.left;
                        localI = 0;
                    }
                }
            }

            snippetPosition = offset + roadDirection * (localI + startSnippet) * 14f;

            localI++;
            Quaternion snippetRotation = Quaternion.identity;

            if(roadDirection == Vector3.forward)
                snippetRotation = Quaternion.Euler(0f, 0f, 0f);
            else if(roadDirection == Vector3.left)
                snippetRotation = Quaternion.Euler(0f, -90f, 0f);
            else if(roadDirection == Vector3.right)
                snippetRotation = Quaternion.Euler(0f, 90f, 0f);

            int randomSnippet = Random.Range(0, _levelSnippets.Length);
            GameObject snippet = Instantiate(_levelSnippets[randomSnippet], snippetPosition, snippetRotation, road.transform);

            foreach(var item in snippet.GetComponentsInChildren<IngredientSpawner>())
            {
                _ingredientSpawners.Add(item);
            }

            if(i == _gameLength - 1)
            {
                snippetPosition = offset + roadDirection * (localI + startSnippet) * 14f;

                GameObject finalPart = Instantiate(_finalPart, snippetPosition, snippetRotation, road.transform);
            }
        }

        GameObject endPoint = new GameObject("EndPoint");
        endPoint.transform.position = offset + roadDirection * (localI + startSnippet) * 14f;
        _pathPoints.Add(endPoint.transform);

        Vector3 xOffset = new Vector3(0, 0, 0);

        if(endPoint.transform.position.x < 0f)
        {
            xOffset = new Vector3(-10, 0, 0);
        }
        else if(endPoint.transform.position.x > 0f)
        {
             xOffset = new Vector3(10, 0, 0);
        }

        Vector3 headpos = endPoint.transform.position + xOffset + Vector3.up * -3f;

        GameObject head = Instantiate(_headPrefab, headpos, Quaternion.identity, _parentLevel.transform);

        head.transform.rotation = Quaternion.LookRotation(roadDirection);

        GameManager.Instance.HeadController = head.GetComponentInChildren<HeadController>();

        if(_progressBar != null)
            _progressBar.PathPoints = _pathPoints.ToArray();

        var ingredientsTotal = new List<Ingredient>(_ingredientSpawners.Count);

        for(int i = 0; i < _ingredientSpawners.Count; i++)
        {
            GameObject prefab = GameManager.Instance.IngredientPrefabs[Random.Range(0, GameManager.Instance.IngredientPrefabs.Length)];
            _ingredientSpawners[i].SpawnIngredient(prefab);

            ingredientsTotal.Add(prefab.GetComponent<Ingredient>());
        }

        GameManager.Instance.TotalIngredientsCount =
            FindObjectsByType<Ingredient>(FindObjectsSortMode.None).Length;

        /* -----------------------------------------------------------
         * 2. Делаем список уникальных И ХОРОШИХ ингредиентов
         * --------------------------------------------------------- */
        List<Ingredient> uniqueIngredients = ingredientsTotal
            .Where(ing => !ing.IsBadIngredient)   // ⬅️ фильтр: игнорируем плохие
            .GroupBy(x => x.Icon)                 // уникальность по иконке
            .Select(g => g.First())
            .ToList();

        /* -----------------------------------------------------------
         * 3. Выбираем случайное количество нужных ингредиентов
         * --------------------------------------------------------- */
        int needCount = Random.Range(3, 8);
        needCount = Mathf.Min(needCount, uniqueIngredients.Count);

        // перемешиваем Фишера-Йейтса
        for(int i = uniqueIngredients.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (uniqueIngredients[i], uniqueIngredients[j]) = (uniqueIngredients[j], uniqueIngredients[i]);
        }

        var selected = uniqueIngredients.Take(needCount);

        /* -----------------------------------------------------------
         * 4. Записываем рецепт
         * --------------------------------------------------------- */
        GameManager.Instance.Recipe.Clear();

        foreach(var ing in selected)
        {
            GameManager.Instance.Recipe.Add(ing);
        }
    }
}
