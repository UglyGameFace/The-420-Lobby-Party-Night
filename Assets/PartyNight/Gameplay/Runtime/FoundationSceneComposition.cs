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

        public bool IsComposed => runtimeRoot != null;
        public GameObject Ground => ground;
        public GameObject LocalPlayer => localPlayer;
        public PartyNightOrbitCamera OrbitCamera => orbitCamera;
        public PartyNightLocalPlayerController LocalController => localController;

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

            runtimeRoot = new GameObject(RuntimeRootName).transform;
            runtimeRoot.SetParent(transform, false);

            ground = new GameObject(GroundName);
            ground.transform.SetParent(runtimeRoot, false);
            ground.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(24f, 1f, 24f);
            ground.AddComponent<BoxCollider>();

            localPlayer = new GameObject(LocalPlayerName);
            localPlayer.transform.SetParent(runtimeRoot, false);
            localPlayer.transform.localPosition = new Vector3(0f, 0.05f, 0f);

            var characterController = localPlayer.AddComponent<CharacterController>();
            characterController.radius = 0.45f;
            characterController.height = 1.8f;
            characterController.center = new Vector3(0f, 0.9f, 0f);
            characterController.slopeLimit = 50f;
            characterController.stepOffset = 0.3f;
            characterController.skinWidth = 0.08f;
            characterController.minMoveDistance = 0f;

            var motor = localPlayer.AddComponent<PartyNightCharacterMotor>();

            orbitCamera = sceneCamera.GetComponent<PartyNightOrbitCamera>();
            if (orbitCamera == null)
            {
                orbitCamera = sceneCamera.gameObject.AddComponent<PartyNightOrbitCamera>();
            }

            orbitCamera.Initialize(localPlayer.transform);

            localController = localPlayer.AddComponent<PartyNightLocalPlayerController>();
            localController.Initialize(motor, orbitCamera);

            Physics.SyncTransforms();
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
