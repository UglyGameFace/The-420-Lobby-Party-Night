using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PartyNight.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class HotboxHavocPrototypeVisuals : MonoBehaviour
    {
        public const int SpawnMarkerCount = 16;
        public const float ArenaHalfSize = 10f;

        private readonly List<Material> ownedMaterials = new();
        private readonly List<Transform> hazePuffs = new();

        private HotboxHavocRoundController roundController;
        private Transform localPlayer;
        private Transform visualRoot;
        private Transform safeZoneDisc;
        private Transform playerBeacon;
        private Material hazeMaterial;
        private bool initialized;

        public bool IsInitialized => initialized;
        public Transform VisualRoot => visualRoot;
        public Transform SafeZoneDisc => safeZoneDisc;
        public Transform PlayerBeacon => playerBeacon;

        public void Initialize(HotboxHavocRoundController round, Transform player)
        {
            if (initialized)
            {
                throw new System.InvalidOperationException(
                    "HotboxHavocPrototypeVisuals is already initialized.");
            }

            roundController = round != null
                ? round
                : throw new System.ArgumentNullException(nameof(round));
            localPlayer = player != null
                ? player
                : throw new System.ArgumentNullException(nameof(player));

            visualRoot = new GameObject("Hotbox Havoc Prototype Visuals").transform;
            visualRoot.SetParent(transform, false);

            var floorMaterial = CreateMaterial(new Color(0.075f, 0.03f, 0.11f, 1f));
            var wallMaterial = CreateMaterial(new Color(0.12f, 0.045f, 0.18f, 1f));
            var greenMaterial = CreateMaterial(new Color(0.32f, 1f, 0.48f, 1f));
            var purpleMaterial = CreateMaterial(new Color(0.72f, 0.24f, 1f, 1f));
            var cyanMaterial = CreateMaterial(new Color(0.24f, 0.9f, 1f, 1f));
            var clearZoneMaterial = CreateMaterial(
                new Color(0.18f, 0.72f, 0.32f, 0.48f),
                transparent: true);
            hazeMaterial = CreateMaterial(
                new Color(0.64f, 0.28f, 0.82f, 0.32f),
                transparent: true);

            CreateFloor(floorMaterial, wallMaterial);
            CreateLoungeAccents(purpleMaterial, greenMaterial);
            CreateSpawnMarkers(greenMaterial, purpleMaterial);

            safeZoneDisc = CreatePrimitive(
                "Clear Zone",
                PrimitiveType.Cylinder,
                new Vector3(0f, 0.015f, 0f),
                new Vector3(
                    roundController.ClearRadius * 2f,
                    0.015f,
                    roundController.ClearRadius * 2f),
                clearZoneMaterial,
                keepCollider: false).transform;

            CreateHazePuffs();
            CreatePlayerBeacon(cyanMaterial, greenMaterial);

            initialized = true;
            Refresh();
        }

        private void Update()
        {
            if (initialized)
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            if (!initialized || safeZoneDisc == null)
            {
                return;
            }

            var diameter = roundController.ClearRadius * 2f;
            safeZoneDisc.localScale = new Vector3(diameter, 0.015f, diameter);

            if (playerBeacon != null && localPlayer != null)
            {
                playerBeacon.position = localPlayer.position;
            }

            var haze = roundController.HazeNormalized;
            for (var index = 0; index < hazePuffs.Count; index++)
            {
                var puff = hazePuffs[index];
                var baseScale = 1.05f + (index % 3) * 0.28f;
                var scale = baseScale * Mathf.Lerp(0.72f, 1.55f, haze);
                puff.localScale = new Vector3(
                    scale * 1.3f,
                    scale,
                    scale * 1.15f);

                var angle =
                    (Mathf.PI * 2f * index / hazePuffs.Count) +
                    haze * 0.16f;
                var radius = Mathf.Lerp(9.1f, 6.2f, haze);
                puff.localPosition = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0.75f + (index % 2) * 0.55f,
                    Mathf.Sin(angle) * radius);
            }
        }

        private void CreateFloor(Material floorMaterial, Material wallMaterial)
        {
            CreatePrimitive(
                "Prototype Floor",
                PrimitiveType.Cube,
                new Vector3(0f, -0.045f, 0f),
                new Vector3(20f, 0.08f, 20f),
                floorMaterial,
                keepCollider: false);

            CreatePrimitive("North Lounge Wall", PrimitiveType.Cube,
                new Vector3(0f, 1.35f, ArenaHalfSize),
                new Vector3(20f, 2.7f, 0.3f), wallMaterial, true);
            CreatePrimitive("South Lounge Wall", PrimitiveType.Cube,
                new Vector3(0f, 1.35f, -ArenaHalfSize),
                new Vector3(20f, 2.7f, 0.3f), wallMaterial, true);
            CreatePrimitive("East Lounge Wall", PrimitiveType.Cube,
                new Vector3(ArenaHalfSize, 1.35f, 0f),
                new Vector3(0.3f, 2.7f, 20f), wallMaterial, true);
            CreatePrimitive("West Lounge Wall", PrimitiveType.Cube,
                new Vector3(-ArenaHalfSize, 1.35f, 0f),
                new Vector3(0.3f, 2.7f, 20f), wallMaterial, true);
        }

        private void CreateLoungeAccents(Material purpleMaterial, Material greenMaterial)
        {
            var positions = new[]
            {
                new Vector3(8.7f, 1.15f, 8.7f),
                new Vector3(-8.7f, 1.15f, 8.7f),
                new Vector3(8.7f, 1.15f, -8.7f),
                new Vector3(-8.7f, 1.15f, -8.7f),
            };

            for (var index = 0; index < positions.Length; index++)
            {
                CreatePrimitive(
                    $"Neon Column {index + 1}",
                    PrimitiveType.Cylinder,
                    positions[index],
                    new Vector3(0.35f, 1.15f, 0.35f),
                    index % 2 == 0 ? purpleMaterial : greenMaterial,
                    false);
            }

            CreatePrimitive(
                "Prototype Lounge Pod A",
                PrimitiveType.Capsule,
                new Vector3(-6.8f, 0.65f, 0f),
                new Vector3(1.4f, 0.55f, 1.4f),
                purpleMaterial,
                false).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            CreatePrimitive(
                "Prototype Lounge Pod B",
                PrimitiveType.Capsule,
                new Vector3(6.8f, 0.65f, 0f),
                new Vector3(1.4f, 0.55f, 1.4f),
                greenMaterial,
                false).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }

        private void CreateSpawnMarkers(Material greenMaterial, Material purpleMaterial)
        {
            const float radius = 7.5f;
            for (var index = 0; index < SpawnMarkerCount; index++)
            {
                var angle = Mathf.PI * 2f * index / SpawnMarkerCount;
                var position = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0.04f,
                    Mathf.Sin(angle) * radius);

                CreatePrimitive(
                    $"Future Spawn {index + 1:00}",
                    PrimitiveType.Cylinder,
                    position,
                    new Vector3(0.34f, 0.025f, 0.34f),
                    index % 2 == 0 ? greenMaterial : purpleMaterial,
                    false);
            }
        }

        private void CreateHazePuffs()
        {
            const int puffCount = 10;
            for (var index = 0; index < puffCount; index++)
            {
                hazePuffs.Add(
                    CreatePrimitive(
                        $"Cartoon Haze Puff {index + 1:00}",
                        PrimitiveType.Sphere,
                        Vector3.zero,
                        Vector3.one,
                        hazeMaterial,
                        false).transform);
            }
        }

        private void CreatePlayerBeacon(Material cyanMaterial, Material greenMaterial)
        {
            playerBeacon = new GameObject("Local Player Engineering Beacon").transform;
            playerBeacon.SetParent(visualRoot, false);

            var ring = CreatePrimitive(
                "Player Beacon Ring",
                PrimitiveType.Cylinder,
                Vector3.zero,
                new Vector3(1.25f, 0.025f, 1.25f),
                cyanMaterial,
                false,
                playerBeacon);
            ring.transform.localPosition = new Vector3(0f, 0.035f, 0f);

            var orb = CreatePrimitive(
                "Player Beacon Orb",
                PrimitiveType.Sphere,
                Vector3.zero,
                new Vector3(0.3f, 0.3f, 0.3f),
                greenMaterial,
                false,
                playerBeacon);
            orb.transform.localPosition = new Vector3(0f, 1.25f, 0f);
        }

        private GameObject CreatePrimitive(
            string objectName,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            bool keepCollider,
            Transform parentOverride = null)
        {
            var primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = objectName;
            primitive.transform.SetParent(
                parentOverride != null ? parentOverride : visualRoot,
                false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localScale = localScale;
            primitive.GetComponent<Renderer>().sharedMaterial = material;

            if (!keepCollider)
            {
                var collider = primitive.GetComponent<Collider>();
                if (collider != null)
                {
                    // Destroy is deferred until end-of-frame. Disable immediately so
                    // visual-only primitives can never participate in gameplay physics
                    // during their creation frame.
                    collider.enabled = false;
                    Destroy(collider);
                }
            }

            return primitive;
        }

        private Material CreateMaterial(Color color, bool transparent = false)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                throw new System.InvalidOperationException(
                    "URP Unlit shader is required for Hotbox Havoc prototype visuals.");
            }

            var material = new Material(shader)
            {
                name = "Hotbox Havoc Prototype Material",
            };

            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);

            if (transparent)
            {
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0f);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = (int)RenderQueue.Transparent;
            }

            ownedMaterials.Add(material);
            return material;
        }

        private void OnDestroy()
        {
            foreach (var material in ownedMaterials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }

            ownedMaterials.Clear();
        }
    }
}
