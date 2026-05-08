using DG.Tweening;
using TMPro;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public bool IsBadIngredient { get; private set; }
    [field: SerializeField] public GameObject LuckParticlePrefab { get; private set; }
    [field: SerializeField] public GameObject LuckTextPrefab { get; private set; }

    [SerializeField] private float _rotateSpeed;
    [field: SerializeField] public Ingredient PrefabReference { get; set; }

    public BoxCollider BoxCollider { get; private set; }
    public bool IsDropped { get; set; }
    public bool IsLuckIngredient { get; set; }

    private Tween _tween;
    private bool _animationStoped;

    private GameObject _luckParticle;

    private void Start()
    {
        BoxCollider = GetComponent<BoxCollider>();

        if (!_animationStoped)
        {
            var startEuler = transform.eulerAngles;
            _tween = transform.DORotate(new Vector3(startEuler.x, startEuler.y + 360f, startEuler.z), _rotateSpeed, RotateMode.FastBeyond360)
                              .SetEase(Ease.Linear)
                              .SetLoops(-1, LoopType.Incremental);
        }

        //if(Random.value < GameManager.Instance.LuckModeChance)
        //{
        //    IsLuckIngredient = true;
        //}
    }

    public void StopAnimation()
    {
        TextMeshPro meshRenderer = GetComponentInChildren<TextMeshPro>();
        ParticleSystem particleSystem = GetComponentInChildren<ParticleSystem>();

        if(meshRenderer != null)
        {
            Destroy(meshRenderer.gameObject);
        }
        if(particleSystem != null)
        {
            Destroy(particleSystem.gameObject);
        }

        _tween.Kill();
        _animationStoped = true;
        Destroy(_luckParticle);
    }

    private void OnDestroy()
    {
        _tween.Kill();

    }

    public void SetLuckIngredient()
    {
        _luckParticle = Instantiate(LuckParticlePrefab, transform);
        IsLuckIngredient = true;
    }
}