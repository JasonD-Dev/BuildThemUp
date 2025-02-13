using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LoadoutController))]
[RequireComponent(typeof(MovementController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] public LoadoutController loadoutController;
    [SerializeField] public MovementController movementController;
    [SerializeField] private AudioSource mAudioSource;

    public static PlayerController instance;

    void Awake()
    {
        if (instance != null)
            Log.Warning(this, "Another player controller instance exists! This one will override it.");

        instance = this;
    }

    void Start()
    {
        CameraController.Instance.SetParent(transform, new Vector3(0, 1.5f, 0));
        movementController.playerController = this;
    }

    void Update()
    {
        // debug controls
        if (Debug.isDebugBuild)
        {
            // heal player
            if (Input.GetKeyDown(KeyCode.F1))
            {
                var tPlayerHealth = transform.GetComponent<Health>();
                tPlayerHealth.Heal(9999);
            }

            // heal base
            if (Input.GetKeyDown(KeyCode.F2))
            {
                var tMainBaseTransform = GamePlayManager.instance.mainBaseTransform;
                var tMainBaseHealth = tMainBaseTransform.GetComponent<Health>();
                tMainBaseHealth.Heal(9999);
            }
            
            // add scrap
            if (Input.GetKeyDown(KeyCode.F3))
                GamePlayManager.instance.AddScrap(2000);
            
            // noclip
            //if (Input.GetKeyDown(KeyCode.F4))
             //   movementController.ToggleNoClipping();
            
            // spawn random enemy
            if (Input.GetKeyDown(KeyCode.F5))
            {
                Transform tCameraTransform = CameraController.Instance.transform;
                Vector3 tCamDirection = tCameraTransform.forward;
                
                Ray tShootRay = new Ray(tCameraTransform.position, tCamDirection);
                if (Physics.Raycast(tShootRay, out var hit, 1000f, loadoutController.raycastMask))
                    EnemiesManager.instance.DebugSpawnEnemy(hit.point);
            }
            
            // spawn random boss
            if (Input.GetKeyDown(KeyCode.F6))
            {
                Transform tCameraTransform = CameraController.Instance.transform;
                Vector3 tCamDirection = tCameraTransform.forward;
                
                Ray tShootRay = new Ray(tCameraTransform.position, tCamDirection);
                if (Physics.Raycast(tShootRay, out var hit, 1000f, loadoutController.raycastMask))
                    EnemiesManager.instance.DebugSpawnBoss(hit.point);
            }
            
            // quick restart
            if (Input.GetKeyDown(KeyCode.F12))
                SceneManager.LoadScene("Jashan_Test");
        }
    }

    public void PlayOneShot(AudioClip clip, float volume = 1)
    {
        mAudioSource.PlayOneShot(clip, volume);
    }
}