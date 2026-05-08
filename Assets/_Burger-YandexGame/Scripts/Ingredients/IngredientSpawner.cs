using UnityEngine;

public class IngredientSpawner : MonoBehaviour
{
    private void Start()
    {
        // Отключаем визуальные и коллайдер компоненты, если нужно
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
    }

    public int SpawnerIndex; // Назначается менеджером

    public GameObject SpawnIngredient(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.transform.localPosition = Vector3.zero;
        //obj.transform.localRotation = Quaternion.identity;
        return obj;
    }

    public void SaveLastSpawnedIndex(int prefabIndex)
    {
        string key = GetSaveKey();
        PlayerPrefs.SetInt(key, prefabIndex);
    }

    public bool TryLoadLastSpawnedIndex(out int prefabIndex)
    {
        string key = GetSaveKey();
        if(PlayerPrefs.HasKey(key))
        {
            prefabIndex = PlayerPrefs.GetInt(key);
            return true;
        }

        prefabIndex = -1;
        return false;
    }

    public void ClearSaved()
    {
        PlayerPrefs.DeleteKey(GetSaveKey());
    }

    private string GetSaveKey()
    {
        int levelIndex = GameManager.Instance.LevelIndex; // <-- Используем LevelIndex
        return $"Level_{levelIndex}_Spawner_{SpawnerIndex}";
    }
}
