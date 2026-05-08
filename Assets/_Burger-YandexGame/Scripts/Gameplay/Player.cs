using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public class Player : MonoBehaviour
{
    [field: SerializeField] public Transform BurgerTop { get; private set; }
    [field: SerializeField] public Transform BurgerDown { get; private set; }
    [field: SerializeField] public float SensitivityMouse { get; set; }
    [field: SerializeField] public float SensitivityKeyboard { get; set; }

    [field: SerializeField] public float Vertical { get; set; }
    public bool CanMove { get; set; } = true;

    [SerializeField] private Transform _burgerComponents;
    public float Speed;
    [SerializeField] private float _sensitivityTouch;

    [SerializeField] private List<Ingredient> _ingredients = new List<Ingredient>();

    private Rigidbody _rb;
    private CameraController _cameraController;
    private float _horizontal;
    public bool HasTriggered { get; set; }

    public List<Ingredient> Ingredients => _ingredients;
    private AudioSource _audioSource;

    private IEnumerator Start()
    {
        while (GameManager.Instance == null || !GameManager.Instance.IsInitialized)
        {
            yield return null;
        }

        _rb = GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();

        Vertical = 1f;
        SensitivityMouse = PlayerPrefs.GetFloat(SaveData.MouseSensitivityKey, 1f);
        SensitivityKeyboard = PlayerPrefs.GetFloat(SaveData.KeyboardSensitivityKey, 1f);
        _sensitivityTouch = SensitivityMouse;

        _cameraController = Camera.main.GetComponent<CameraController>();


        GameManager.Instance.ChangeSkin(GameManager.Instance.CurrentSkinID);
        GameManager.Instance.ChangeHeadSkin(GameManager.Instance.CurrentHeadSkinID);
        GameManager.Instance.ChangeBlockSkin(GameManager.Instance.CurrentBlockSkinID);

        if ((GameManager.Instance.LevelIndex + 1) % 6 == 0 && GameManager.Instance.LevelIndex != 0)
        {
            GameManager.Instance.ProposeBonusLevel();
        }
    }

    //public void ApplyLoadedData()
    //{
    //    Debug.Log("Применяем данные после загрузки");

    //    SceneManager.LoadScene(YandexGame.savesData.levelIndex);
    //}

    private void Update()
    {
        _horizontal = Input.GetAxis("Horizontal");

        if (Input.GetMouseButton(0))
        {
            _horizontal = Input.GetAxis("Mouse X") * SensitivityMouse;
        }

        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Moved)
                {
                    _horizontal = touch.deltaPosition.x * _sensitivityTouch;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            CloneAndAddIngredient(1);
        }
    }

    public void CloneAndAddIngredient(int count)
    {
        if (Ingredients.Count == 0 || count <= 0)
            return;

        Ingredient lastIngredient = Ingredients[Ingredients.Count - 1];
        Sprite targetIcon = lastIngredient.Icon;

        // Найти оригинальный префаб по иконке
        GameObject prefab = null;
        foreach (var prefabGO in GameManager.Instance.IngredientPrefabs)
        {
            Ingredient ingredient = prefabGO.GetComponent<Ingredient>();
            if (ingredient != null && ingredient.Icon == targetIcon)
            {
                prefab = prefabGO;
                break;
            }
        }

        if (prefab == null)
        {
            Debug.LogError("Prefab with matching icon not found in GameManager.IngredientPrefabs");
            return;
        }

        // Спавн чистых копий оригинального префаба
        for (int i = 0; i < count; i++)
        {
            GameObject cloneGO = Instantiate(prefab);
            Ingredient clone = cloneGO.GetComponent<Ingredient>();

            clone.name = prefab.name + "_Copy_" + i;

            AddIngredient(clone);
        }
    }

    public bool IsLaunched { get; private set; }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.IsInitialized)
            return;

        if (IsLaunched)
        {
            return;
        }

        if (CanMove)
        {
            float verticalMovement = IsLaunched ? 0f : Vertical;
            Vector3 velocity = new Vector3(_horizontal, 0, verticalMovement) * Speed;
            Vector3 worldVelocity = transform.TransformDirection(velocity);

            Vector3 currentVelocity = _rb.velocity;
            currentVelocity.x = worldVelocity.x;
            if (!IsLaunched)
                currentVelocity.z = worldVelocity.z;

            _rb.velocity = currentVelocity;
        }
    }

    public void LaunchFromSpringboard(Vector3 direction, float height, float distance, float duration)
    {
        if (IsLaunched) return;
        StartCoroutine(PerformLaunch(direction, height, distance, duration));
    }

    private IEnumerator PerformLaunch(Vector3 direction, float height, float distance, float duration)
    {
        IsLaunched = true;
        _rb.useGravity = false;

        Vector3 startPos = transform.position;
        float timeElapsed = 0f;

        Vector3 sidewaysDirection = Vector3.Cross(Vector3.up, direction).normalized;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.fixedDeltaTime;
            float progress = Mathf.Clamp01(timeElapsed / duration);

            float verticalFactor = Mathf.Sin(progress * Mathf.PI); 
            float currentVerticalHeight = verticalFactor * height;

            Vector3 currentForwardMovement = direction * distance * progress;
            Vector3 targetPos = startPos + currentForwardMovement + new Vector3(0, currentVerticalHeight, 0);

            Vector3 trajectoryVelocity = (targetPos - transform.position) / Time.fixedDeltaTime;

            Vector3 playerSidewaysVelocity = sidewaysDirection * _horizontal * Speed;

            Vector3 forwardVel = Vector3.Project(trajectoryVelocity, direction);
            Vector3 upwardVel = Vector3.Project(trajectoryVelocity, Vector3.up);

            _rb.velocity = forwardVel + upwardVel + playerSidewaysVelocity;

            yield return new WaitForFixedUpdate();
        }

        IsLaunched = false;
        _rb.useGravity = true;
        yield return new WaitForFixedUpdate();
        _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.transform.TryGetComponent(out Ingredient interactableItem))
        {
            AddIngredient(interactableItem);

            //if (interactableItem.IsLuckIngredient)
            //{
            //    GameObject luckText = Instantiate(interactableItem.LuckTextPrefab, interactableItem.transform);
            //    luckText.transform.rotation = transform.rotation;
            //    luckText.transform.DOMoveY(luckText.transform.position.y + 5, 4);

            //    Destroy(luckText, 5);
            //}
        }

        if(HasTriggered)
            return;

        if(collider.gameObject.TryGetComponent(out Trap trap))
        {
            DeleteIngredient(trap.RemoveIngredient);
            HasTriggered = true;

            int count = Mathf.Min(trap.RemoveIngredient, _ingredients.Count);

            for(int i = 0; i < count; i++)
            {
                _cameraController.StepForward();
            }
        }
    }

    private void OnDisable()
    {
        GameManager.Instance.FinalIngredients.Clear();
        GameManager.Instance.FinalIngredients.AddRange(_ingredients);
    }

    public void UpdateSensitivity(float newSensitivity, bool isMouseSensitivity)
    {
        if(isMouseSensitivity)
            SensitivityMouse = newSensitivity;
        else
            SensitivityKeyboard = newSensitivity;
    }

    public void DeleteIngredient(int count)
    {
        int a = Mathf.Min(_ingredients.Count, count);

        for (int i = a - 1; i >= 0; i--)
        {
            if (_ingredients[i] != null)
            {
                _ingredients[i].IsDropped = true;
                _ingredients[i].transform.SetParent(null);

                Destroy(_ingredients[i].gameObject);

                _ingredients.RemoveAt(i);
            }
        }

        if (_ingredients.Count == 0)
        {
            BurgerTop.transform.rotation = BurgerDown.transform.rotation;

            Vector3 topNewWorld = GetBoxColliderTopWorldPoint(BurgerDown.gameObject);
            Vector3 bottomTopLocal = GetBoxColliderBottomLocal(BurgerTop.gameObject);

            BurgerTop.transform.position = Vector3.zero;
            Vector3 bottomTopWorld = BurgerTop.transform.TransformPoint(bottomTopLocal);

            BurgerTop.transform.position = topNewWorld - (bottomTopWorld - BurgerTop.transform.position);

            HingeJoint topHinge = BurgerTop.GetComponent<HingeJoint>();
            if (topHinge)
            {
                topHinge.connectedBody = BurgerDown.GetComponent<Rigidbody>();
            }
        }

        for (int i = 0; i < _ingredients.Count; i++)
        {
            GameObject previousObject = null;
            if (i == 0)
            {
                previousObject = BurgerDown.gameObject;
            }
            else
            {
                previousObject = _ingredients[i - 1].gameObject;
            }

            _ingredients[i].transform.rotation = previousObject.transform.rotation;

            Vector3 topPrev = GetBoxColliderTopWorldPoint(previousObject);
            Vector3 bottomNewLocal = GetBoxColliderBottomLocal(_ingredients[i].gameObject);

            _ingredients[i].transform.position = Vector3.zero;

            Vector3 bottomNewWorld = _ingredients[i].transform.TransformPoint(bottomNewLocal);

            _ingredients[i].transform.position = topPrev - (bottomNewWorld - _ingredients[i].transform.position);

            HingeJoint hingeJoint = _ingredients[i].gameObject.GetComponent<HingeJoint>();
            hingeJoint.useSpring = true;
            hingeJoint.useLimits = true;

            if (i == 0)
            {
                hingeJoint.connectedBody = BurgerDown.GetComponent<Rigidbody>();
            }
            else
            {
                hingeJoint.connectedBody = _ingredients[i - 1].GetComponent<Rigidbody>();
            }

            BurgerTop.transform.rotation = _ingredients[i].transform.rotation;

            Vector3 topNewWorld = GetBoxColliderTopWorldPoint(_ingredients[i].gameObject);
            Vector3 bottomTopLocal = GetBoxColliderBottomLocal(BurgerTop.gameObject);

            BurgerTop.transform.position = Vector3.zero;
            Vector3 bottomTopWorld = BurgerTop.transform.TransformPoint(bottomTopLocal);

            BurgerTop.transform.position = topNewWorld - (bottomTopWorld - BurgerTop.transform.position);

            HingeJoint topHinge = BurgerTop.GetComponent<HingeJoint>();
            if (topHinge)
            {
                topHinge.connectedBody = _ingredients[i].GetComponent<Rigidbody>();
            }
        }

        if(_ingredients.Count > 0)
        {
            StartCoroutine(SetHasTrigger());
        }
        else
        {
            GameManager.Instance.LoseGame();
        }

    }

    public void DeleteJoint(Transform playerContainer)
    {
        foreach (var item in _ingredients)
        {
            Destroy(item.GetComponent<HingeJoint>());
            Destroy(item.GetComponent<Rigidbody>());

            item.transform.SetParent(playerContainer);
            item.transform.localPosition = new Vector3(0, item.transform.localPosition.y, 0);

        }

        Destroy(BurgerDown.GetComponent<HingeJoint>());
        Destroy(BurgerDown.GetComponent<Rigidbody>());
        BurgerDown.SetParent(playerContainer);
        BurgerDown.transform.localPosition = new Vector3(0, BurgerDown.transform.localPosition.y, 0);

        Destroy(BurgerTop.GetComponent<HingeJoint>());
        Destroy(BurgerTop.GetComponent<Rigidbody>());
        BurgerTop.SetParent(playerContainer);
        BurgerTop.transform.localPosition = new Vector3(0, BurgerTop.transform.localPosition.y, 0);

    }

    private IEnumerator SetHasTrigger()
    {
        yield return new WaitForSeconds(0.1f);
        HasTriggered = false;
    }

    public void AddIngredient(Ingredient ingredient)
    {
        if (_ingredients.Contains(ingredient))
            return;

        _audioSource.volume = PlayerPrefs.GetFloat(SaveData.SoundKey, 1f);
        _audioSource.PlayOneShot(_audioSource.clip);

        if (ingredient.IsLuckIngredient)
        {
            GameObject luckText = Instantiate(ingredient.LuckTextPrefab, ingredient.transform);
            luckText.transform.rotation = transform.rotation;
            luckText.transform.DOMoveY(luckText.transform.position.y + 5, 4);

            print(1);
            Destroy(luckText, 5);

        }


        var oldJoint = ingredient.GetComponent<HingeJoint>();
        if (oldJoint != null)
        {
            Destroy(oldJoint); // Удаляем сразу, чтобы не было проблем с физикой
        }

        ingredient.transform.SetParent(_burgerComponents.transform);
        ingredient.StopAnimation();

        GameObject previousObject;
        if (_ingredients.Count == 0)
        {
            previousObject = BurgerDown.gameObject;
        }
        else
        {
            previousObject = _ingredients[_ingredients.Count - 1].gameObject;
        }

        ingredient.transform.rotation = previousObject.transform.rotation;

        Vector3 topPrev = GetBoxColliderTopWorldPoint(previousObject);
        Vector3 bottomNewLocal = GetBoxColliderBottomLocal(ingredient.gameObject);

        ingredient.transform.position = Vector3.zero;

        Vector3 bottomNewWorld = ingredient.transform.TransformPoint(bottomNewLocal);

        ingredient.transform.position = topPrev - (bottomNewWorld - ingredient.transform.position);

        _ingredients.Add(ingredient);

        // Создаём новый HingeJoint с правильным connectedBody
        HingeJoint hingeJoint = ingredient.gameObject.AddComponent<HingeJoint>();
        hingeJoint.useSpring = true;
        hingeJoint.useLimits = true;

        if (_ingredients.Count == 1)
        {
            hingeJoint.connectedBody = BurgerDown.GetComponent<Rigidbody>();
        }
        else
        {
            hingeJoint.connectedBody = _ingredients[_ingredients.Count - 2].GetComponent<Rigidbody>();
        }

        // Обновляем позицию BurgerTop
        BurgerTop.transform.rotation = ingredient.transform.rotation;

        Vector3 topNewWorld = GetBoxColliderTopWorldPoint(ingredient.gameObject);
        Vector3 bottomTopLocal = GetBoxColliderBottomLocal(BurgerTop.gameObject);

        BurgerTop.transform.position = Vector3.zero;
        Vector3 bottomTopWorld = BurgerTop.transform.TransformPoint(bottomTopLocal);

        BurgerTop.transform.position = topNewWorld - (bottomTopWorld - BurgerTop.transform.position);

        HingeJoint topHinge = BurgerTop.GetComponent<HingeJoint>();
        if (topHinge)
        {
            topHinge.connectedBody = ingredient.GetComponent<Rigidbody>();
        }

        Rigidbody rb = ingredient.GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        UpdateMasses();

        hingeJoint.useSpring = true;
        hingeJoint.spring = new JointSpring { spring = 40f, damper = 10f, targetPosition = 0f };
        hingeJoint.useLimits = true;
        hingeJoint.limits = new JointLimits { min = -45f, max = 45f };
        hingeJoint.massScale = 10f;
        hingeJoint.connectedMassScale = 6f;

        _cameraController.StepBack();

        GameManager.Instance.UIController.SetCheckRecipe(ingredient.Icon);
    }

    private void UpdateMasses()
    {
        int count = _ingredients.Count + 2; // верхняя + ингредиенты + нижняя
        for (int i = 0; i < count; i++)
        {
            GameObject go;
            if (i == 0) go = BurgerTop.gameObject;
            else if (i == count - 1) go = BurgerDown.gameObject;
            else go = _ingredients[i - 1].gameObject;

            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Масса растет к низу (i=0 — верхняя булка, i=count-1 — нижняя)
                rb.mass = Mathf.Lerp(1f, 5f, (float)i / (count - 1));
            }
        }
    }

    public void SetMassScale(float massScale, float connectedMassScale)
    {
        foreach(var item in _ingredients)
        {
            HingeJoint hingeJoint = item.GetComponent<HingeJoint>();
            hingeJoint.massScale = massScale;
            hingeJoint.connectedMassScale = connectedMassScale;
        }

        BurgerTop.GetComponent<HingeJoint>().massScale = massScale;
        BurgerTop.GetComponent<HingeJoint>().connectedMassScale = connectedMassScale;
    }

    private Vector3 GetBoxColliderTopWorldPoint(GameObject go)
    {
        BoxCollider coll = go.GetComponent<BoxCollider>();
        if (!coll)
            return go.transform.position;

        Vector3 topLocal = coll.center + new Vector3(0, coll.size.y / 2f, 0);
        return go.transform.TransformPoint(topLocal);
    }

    private Vector3 GetBoxColliderBottomLocal(GameObject go)
    {
        BoxCollider coll = go.GetComponent<BoxCollider>();
        if (!coll)
            return Vector3.zero;

        return coll.center - new Vector3(0, coll.size.y / 2f, 0);
    }
}
