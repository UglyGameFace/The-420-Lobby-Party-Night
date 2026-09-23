using PartyNight.Networking;
using System.Linq;
using UnityEngine;

namespace PartyNight.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class FoundationSceneComposition : MonoBehaviour
    {
        public const string SceneRootName = "Foundation Scene Composition";
        public const string RuntimeRootName = "Foundation Runtime";
        public const string GroundName = "Foundation Ground";
        public const string LocalPlayerName = "Local Player";

        private Transform runtimeRoot;
        private GameObject ground;
        private GameObject localPlayer;
        private PartyNightOrbitCamera orbitCamera;
        private PartyNightLocalPlayerController localController;
        private HotboxHavocPrototype hotboxPrototype;
        [SerializeField]
        private GameObject networkPlayerPrefab;

        private PartyNightNetworkBootstrap networkBootstrap;

        public bool IsComposed => runtimeRoot != null;
        public GameObject Ground => ground;
        public GameObject LocalPlayer => localPlayer;
        public PartyNightOrbitCamera OrbitCamera => orbitCamera;
        public PartyNightLocalPlayerController LocalController => localController;
        public HotboxHavocPrototype HotboxPrototype => hotboxPrototype;
        public PartyNightNetworkBootstrap NetworkBootstrap => networkBootstrap;
        public GameObject NetworkPlayerPrefab => networkPlayerPrefab;

        private void Awake()
        {
            Compose();
        }

        public void Compose()
        {
            if (IsComposed)
            {
                return;
            }

            var sceneCamera = FindSceneMainCamera();
            if (sceneCamera == null)
            {
                throw new System.InvalidOperationException(
                    "PartyNightFoundation requires exactly one MainCamera-tagged Camera.");
            }

            GameObject newRuntimeRoot = null;
            GameObject newGround = null;
            GameObject newLocalPlayer = null;
            PartyNightOrbitCamera newOrbitCamera = null;
            PartyNightLocalPlayerController newLocalController = null;
            HotboxHavocPrototype newHotboxPrototype = null;
            PartyNightNetworkBootstrap newNetworkBootstrap = null;
            bool createdNetworkBootstrap = false;

            try
            {
                newRuntimeRoot = new GameObject(RuntimeRootName);
                newRuntimeRoot.transform.SetParent(transform, false);

                newNetworkBootstrap =
                    PartyNightNetworkBootstrap.GetOrCreateRuntime(
                        out createdNetworkBootstrap);
                newNetworkBootstrap.ConfigurePlayerPrefab(networkPlayerPrefab);

                newGround = new GameObject(GroundName);
                newGround.transform.SetParent(newRuntimeRoot.transform, false);
                newGround.transform.localPosition = new Vector3(0f, -0.5f, 0f);
                newGround.transform.localScale = new Vector3(24f, 1f, 24f);
                newGround.AddComponent<BoxCollider>();

                newLocalPlayer = new GameObject(LocalPlayerName);
                newLocalPlayer.transform.SetParent(newRuntimeRoot.transform, false);
                newLocalPlayer.transform.localPosition = new Vector3(0f, 0.05f, 0f);

                var characterController =
                    newLocalPlayer.AddComponent<CharacterController>();
                characterController.radius = 0.45f;
                characterController.height = 1.8f;
                characterController.center = new Vector3(0f, 0.9f, 0f);
                characterController.slopeLimit = 50f;
                characterController.stepOffset = 0.3f;
                characterController.skinWidth = 0.08f;
                characterController.minMoveDistance = 0f;

                var motor =
                    newLocalPlayer.AddComponent<PartyNightCharacterMotor>();

                newOrbitCamera =
                    sceneCamera.GetComponent<PartyNightOrbitCamera>();
                if (newOrbitCamera == null)
                {
                    newOrbitCamera =
                        sceneCamera.gameObject.AddComponent<PartyNightOrbitCamera>();
                }

                newOrbitCamera.Initialize(newLocalPlayer.transform);

                newLocalController =
                    newLocalPlayer.AddComponent<PartyNightLocalPlayerController>();
                newLocalController.Initialize(motor, newOrbitCamera);

                var newHotboxObject =
                    new GameObject(HotboxHavocPrototype.RuntimeName);
                newHotboxObject.transform.SetParent(
                    newRuntimeRoot.transform,
                    false);
                newHotboxPrototype =
                    newHotboxObject.AddComponent<HotboxHavocPrototype>();
                newHotboxPrototype.Initialize(
                    newLocalPlayer.transform,
                    motor,
                    newOrbitCamera);

                Physics.SyncTransforms();

                runtimeRoot = newRuntimeRoot.transform;
                ground = newGround;
                localPlayer = newLocalPlayer;
                orbitCamera = newOrbitCamera;
                localController = newLocalController;
                hotboxPrototype = newHotboxPrototype;
                networkBootstrap = newNetworkBootstrap;
            }
            catch
            {
                if (newRuntimeRoot != null)
                {
                    Destroy(newRuntimeRoot);
                }

                if (createdNetworkBootstrap && newNetworkBootstrap != null)
                {
                    Destroy(newNetworkBootstrap.gameObject);
                }

                if (newOrbitCamera != null &&
                    newOrbitCamera.gameObject == sceneCamera.gameObject)
                {
                    Destroy(newOrbitCamera);
                }

                throw;
            }
        }

        private Camera FindSceneMainCamera()
        {
            return gameObject.scene
                .GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Camera>(true))
                .SingleOrDefault(cameraComponent =>
                    cameraComponent.CompareTag("MainCamera"));
        }
    }
}
