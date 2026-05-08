using UnityEngine;

public class ColliderCombiner : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;

    public void Start()
    {
        CombineToBoxCollider();
    }

    private void CombineToBoxCollider()
    {
        MeshFilter[] allMeshFilters = FindObjectsOfType<MeshFilter>();
        var groundObjects = new System.Collections.Generic.List<GameObject>();

        // Собираем объекты нужного слоя
        foreach (var mf in allMeshFilters)
        {
            if (((1 << mf.gameObject.layer) & groundLayer) != 0)
            {
                groundObjects.Add(mf.gameObject);
            }
        }

        if (groundObjects.Count == 0)
        {
            Debug.LogWarning("No ground objects found on specified layer.");
            return;
        }

        // Ищем общий bounds для всех объектов
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        foreach (var go in groundObjects)
        {
            Renderer rend = go.GetComponent<Renderer>();
            if (rend != null)
            {
                Bounds b = rend.bounds;
                min = Vector3.Min(min, b.min);
                max = Vector3.Max(max, b.max);
            }
            else
            {
                // Если рендера нет, можно попробовать MeshFilter bounds, преобразовав к world coords
                MeshFilter mf = go.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    Bounds b = mf.sharedMesh.bounds; // локальные bounds
                    Vector3 worldMin = go.transform.TransformPoint(b.min);
                    Vector3 worldMax = go.transform.TransformPoint(b.max);
                    min = Vector3.Min(min, worldMin);
                    max = Vector3.Max(max, worldMax);
                }
            }
        }

        Vector3 center = (min + max) / 2f;
        Vector3 size = max - min;

        // Создаём объект с BoxCollider
        GameObject combinedColliderObj = new GameObject("CombinedBoxCollider");
        combinedColliderObj.transform.position = center;
        combinedColliderObj.transform.rotation = Quaternion.identity;
        combinedColliderObj.transform.SetParent(transform);

        BoxCollider boxCollider = combinedColliderObj.AddComponent<BoxCollider>();
        boxCollider.size = size;

        // Удаляем старые коллайдеры у исходных объектов
        foreach (var go in groundObjects)
        {
            Collider[] colliders = go.GetComponents<Collider>();
            foreach (var col in colliders)
            {
                Destroy(col);
            }
        }
    }
}
