using DG.Tweening;
using System.Linq;
using System;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

public class HeadController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GameObject _foodPosition;
    [SerializeField] private GameObject _fireParticle;
    [SerializeField] private Transform _playerContainer;
    [SerializeField] private SkinnedMeshRenderer _headMesh;
    [SerializeField] private AudioClip _fire;

    [Header("Settings")]
    [SerializeField] private float _headRotateSpeed;
    [SerializeField] private float _sequenceSpeed;
    [SerializeField] private float tolerance = 0.01f;

    [Header("Camera Pos")]
    [SerializeField] private Transform _cameraEatPos;
    [SerializeField] private Transform _cameraLookPos;
    [field: SerializeField] public HeadSkinData HeadSkinData { get; private set; }

    private bool feetHeadTriggered;

    private Animator _animator;
    private GameObject _playerCamera;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _playerCamera = Camera.main.gameObject;
        _headMesh = GetComponentInChildren<SkinnedMeshRenderer>();

        _fireParticle.SetActive(false);
    }

    public void PlayEatAnimation()
    {
        _playerCamera.transform.SetParent(null);
        _animator.Play("LoopEat");

        var cameraTransform = _playerCamera.transform;

        Vector3 vampirPos = new Vector3(0, 2, -3.4f);
        Vector3 vampirRot = new Vector3(-75, 180, 0);

        DG.Tweening.Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOLocalMove(vampirPos, _sequenceSpeed))
                .Join(transform.DOLocalRotate(vampirRot, _sequenceSpeed))
                .Append(cameraTransform.DOMove(_cameraEatPos.position, _sequenceSpeed))
                .Join(cameraTransform.DORotate(_cameraEatPos.eulerAngles, _sequenceSpeed))
                .OnComplete(() => FeetHead());
    }

    public enum PersonState
    {
        Idle,
        Eat,
        Tasty,
        Sick,
        Bad
    }

    public void PlayAngryAnimation(int stars = -1)
    {
        var cameraTransform = _playerCamera.transform;
        PersonState state;

        state = ChooseStateFromIngredients();
        if (state == PersonState.Sick || state == PersonState.Bad)
        {
            print("oh no");
        }
        else if (stars >= 0)
        {
            print(stars);
            state = stars switch
            {
                2 or 3 => PersonState.Tasty,      // Радость
                1 => PersonState.Eat,   // Нейтральность
                0 => PersonState.Bad,        // Злость
                _ => PersonState.Idle
            };
        }
        else
        {
            print("pizda");
            // Если звёзды не переданы, использовать старую логику
            state = ChooseStateFromIngredients();
        }

        string clip = state switch
        {
            PersonState.Eat => "LoopEat",
            PersonState.Tasty => "LoopTasty",
            PersonState.Sick => "LoopSick",
            PersonState.Bad => "LoopBad",
            _ => "LoopEat"
        };
        _animator.Play(clip);

        Color targetColor = state switch
        {
            PersonState.Bad => Color.red,
            PersonState.Sick => new Color(0.04227482f, 0.4716981f, 0.04227482f),
            _ => Color.clear
        };

        Color baseColorToFind1 = new Color(0.8001417f, 0.5620998f, 0.4395702f);
        Color baseColorToFind2 = new Color(0.8588235f, 0.9098039f, 0.8431373f);

        bool shouldTint = state == PersonState.Bad || state == PersonState.Sick;

        var mats = _headMesh.materials;
        for (int i = 0; i < mats.Length; i++)
        {
            Material m = mats[i];

            string readProp = m.HasProperty("_BaseColor") ? "_BaseColor"
                             : m.HasProperty("_Color") ? "_Color"
                             : null;
            if (readProp == null)
                continue;

            Color current = m.GetColor(readProp);

            float dist1 = Vector3.Distance(
                new Vector3(current.r, current.g, current.b),
                new Vector3(baseColorToFind1.r, baseColorToFind1.g, baseColorToFind1.b));
            float dist2 = Vector3.Distance(
                new Vector3(current.r, current.g, current.b),
                new Vector3(baseColorToFind2.r, baseColorToFind2.g, baseColorToFind2.b));

            if (shouldTint && (dist1 < tolerance || dist2 < tolerance))
            {
                DOTween.To(
                    () => m.GetColor(readProp),
                    c => m.SetColor(readProp, c),
                    targetColor,
                    _headRotateSpeed
                );
            }
        }

        if (state == PersonState.Bad)
        {
            _fireParticle.SetActive(true);
            GameManager.Instance.UIController.PlaySound(_fire);
        }

        DG.Tweening.Sequence sequence = DOTween.Sequence();
        sequence.Append(cameraTransform.DOMove(_cameraLookPos.position, _sequenceSpeed))
                .Join(cameraTransform.DORotate(_cameraLookPos.eulerAngles, _sequenceSpeed));
    }

    private static readonly Dictionary<string, PersonState> _ingredientReactions = new()
    {
        { "Avocado Slice", PersonState.Tasty },
        { "Arugula Leaf",  PersonState.Tasty },
        { "Tomato Slice",  PersonState.Tasty },
        { "Cucumber",      PersonState.Tasty },
        { "Salad Slice",   PersonState.Tasty },
        { "Cheese Slice",  PersonState.Tasty },
        { "Egg Boiled",    PersonState.Tasty },

        { "Bacon Slice",   PersonState.Eat },
        { "Cutlet B",      PersonState.Eat },
        { "Anchovy",       PersonState.Eat },
        { "Fish Slice",    PersonState.Eat },
        { "Shrimp",        PersonState.Eat },
        { "Onion Slice",   PersonState.Eat },

        { "Bone",   PersonState.Sick },
        { "CrushedCan",   PersonState.Sick },
        { "FishBone",   PersonState.Sick },
        { "Glasses",   PersonState.Sick },
        { "Phone",   PersonState.Sick },
        { "ToiletPaper",   PersonState.Sick },

        { "Pepper Red",    PersonState.Bad }
    };

    private static readonly PersonState[] _priority =
    {
        PersonState.Sick,
        PersonState.Bad,
        PersonState.Tasty,
        PersonState.Eat,
    };

    private PersonState ChooseStateFromIngredients()
    {
        var ingredients = GameManager.Instance.FinalIngredients;
        if (ingredients == null || ingredients.Count == 0)
            return PersonState.Idle;

        PersonState result = PersonState.Eat;

        foreach (var ing in ingredients)
        {
            string key = ing.Icon.name;

            if (_ingredientReactions.TryGetValue(key, out var reaction))
            {
                if (Array.IndexOf(_priority, reaction) < Array.IndexOf(_priority, result))
                    result = reaction;
            }
        }

        return result;
    }

    private void FeetHead()
    {
        if (feetHeadTriggered)
            return;

        feetHeadTriggered = true;
        _playerContainer.DOMove(_foodPosition.transform.position, _sequenceSpeed);

        Rigidbody rigidbody = _playerContainer.AddComponent<Rigidbody>();
        rigidbody.drag = 4;
        rigidbody.isKinematic = false;

        GameManager.Instance.Player.BurgerTop.GetComponent<Rigidbody>().isKinematic = false;
        GameManager.Instance.Player.DeleteJoint(_playerContainer);
    }
}