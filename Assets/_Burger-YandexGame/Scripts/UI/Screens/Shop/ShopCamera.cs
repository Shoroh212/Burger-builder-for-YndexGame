using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopCamera : MonoBehaviour
{
    [Header("Burger")]
    [SerializeField] private Vector3 _burgerPivotPos = new Vector3(0, 1f, 3f);
    [SerializeField] private Vector3 _burgerPivotRot = new Vector3(-48f, 270f, -2.4f);
    [SerializeField] private Vector3 _burgerCameraPos = new Vector3(0, -0.5f, 0.5f);
    [SerializeField] private Vector3 _burgerCameraRot = new Vector3(-18, 180, -1.5f);
    [SerializeField] private Vector3 _burgerRot;
    [SerializeField] private LayerMask _burgerLayer;

    [Header("Head")]
    [SerializeField] private Vector3 _headPivotRot = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 _headCameraRot = new Vector3(0, 0, 0);
    [SerializeField] private LayerMask _headLayer;

    [Header("Block")]
    [SerializeField] private Vector3 _blockCameraPos = new Vector3(0, -0.5f, 0.5f);
    [SerializeField] private Vector3 _blockCameraRot = new Vector3(-18, 180, -1.5f);
    [SerializeField] private Vector3 _blockBlockRot = new Vector3(-2.41f, 90f, 48f);
    [SerializeField] private LayerMask _blockLayer;

    [Header("Settings")]
    [SerializeField] private float _rotateSpeed;
    [SerializeField] private GameObject _roadCube;

    private Camera _camera;
    private Tweener _tweener;

    private void Start()
    {
        _burgerPivotPos.z = FindAnyObjectByType<Player>().transform.position.z;

       var startEuler = transform.eulerAngles;
        _camera = GetComponentInChildren<Camera>();
        _tweener = transform.DORotate(new Vector3(startEuler.x, startEuler.y + 360f, startEuler.z), _rotateSpeed, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);

        _camera.cullingMask = _burgerLayer;
        _camera.farClipPlane = 2;

        transform.position = _burgerPivotPos;
        transform.eulerAngles = _burgerPivotRot;
        _camera.transform.localPosition = _burgerCameraPos;
        _camera.transform.localEulerAngles = _burgerCameraRot;
    }

    public void SetCamera(Player player)
    {
        if(_camera == null)
            return;
        _tweener.Kill();
        _camera.cullingMask = _burgerLayer;
        _camera.farClipPlane = 2;

        transform.position = _burgerPivotPos;
        transform.eulerAngles = _burgerPivotRot;
        _camera.transform.localPosition = _burgerCameraPos;
        _camera.transform.localEulerAngles = _burgerCameraRot;

        var startEuler = transform.eulerAngles;
        Vector3 endValue = new Vector3(startEuler.x, startEuler.y + 360f, startEuler.z);
        _tweener = transform.DORotate(endValue, _rotateSpeed, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);
        _roadCube.SetActive(false);
    }

    public void SetCamera(HeadController headController)
    {
        _tweener.Kill();
        _camera.cullingMask = _headLayer;
        _camera.farClipPlane = 30;

        Vector3 cameraPivot = new Vector3();
        Vector3 cameraHead = new Vector3();

        if(headController.name == "VampirHead")
        {
            cameraPivot = new Vector3(0, 2.77f, 0);
            cameraHead = new Vector3(0, 0, -10);
        }
        else if(headController.name == "MenHead")
        {
            cameraPivot = new Vector3(0, 2.77f, 0);
            cameraHead = new Vector3(0, 1, -7);
        }
        else if(headController.name == "NinjaHead")
        {
            cameraPivot = new Vector3(0, 2.77f, 0);
            cameraHead = new Vector3(0, 0, -10);
        }
        else if(headController.name == "GirlHead")
        {
            cameraPivot = new Vector3(0, 2.77f, 0);
            cameraHead = new Vector3(0, 0, -10);
        }

        transform.position = new Vector3(headController.transform.position.x, cameraPivot.y, headController.transform.position.z + 1);
        transform.eulerAngles = _headPivotRot;

        // Проверка: если текущий уровень — последний
        if(SceneManager.GetActiveScene().buildIndex == SceneManager.sceneCountInBuildSettings - 1)
        {
            Vector3 offset = new Vector3(1, 0, -1);
            transform.position += offset;
        }

        _camera.transform.localPosition = cameraHead;
        _camera.transform.localEulerAngles = _headCameraRot;

        var startEuler = transform.eulerAngles;
        Vector3 endValue = new Vector3(startEuler.x, startEuler.y + 360f, startEuler.z);
        _tweener = transform.DORotate(endValue, _rotateSpeed, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);

        _roadCube.SetActive(false);
    }

    public void SetCamera()
    {
        if(_camera == null)
            return;

        _tweener.Kill();
        _roadCube.transform.DOKill(true); // Убиваем все твинны на roadCube, чтобы не конфликтовали

        _camera.cullingMask = _blockLayer;
        _camera.farClipPlane = 10;

        // Камера
        _camera.transform.localPosition = _blockCameraPos;
        _camera.transform.localEulerAngles = _blockCameraRot;

        if(_roadCube != null)
        {
            _roadCube.SetActive(true);

            // Ставим локальный поворот, если roadCube - дочерний (или позицию в мировых координатах)
            _roadCube.transform.localEulerAngles = _blockBlockRot;

            // Запускаем анимацию вращения: добавляем 360 по локальной оси Y бесконечно
            _tweener = _roadCube.transform.DOLocalRotate(
                new Vector3(0, 360, 0),
                _rotateSpeed,
                RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);
        }
    }
}

public enum CameraPos
{
    Burger,
    Head,
}
