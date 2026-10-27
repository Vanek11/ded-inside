using UnityEngine;
using Unity.Netcode;

namespace DI
{
    /// Сборка игрока-грузчика. Используется и в соло (LevelVillage1),
    /// и в сети (DINetworkManager спавнит по одному на подключение).
    public static class PlayerFactory
    {
        public static GameObject BuildPlayer(Vector3 position, bool withCamera)
        {
            var go = new GameObject("Player");
            go.tag = "Player";
            go.layer = LayerMask.NameToLayer("Player");
            go.transform.position = position;
            go.AddComponent<NetDynamicMarker>();

            var col = go.AddComponent<CapsuleCollider>();
            col.radius = 0.34f;
            col.height = 1.7f;
            col.center = new Vector3(0f, 0.85f, 0f);

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 80f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Body";
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.transform.SetParent(go.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            visual.transform.localScale = new Vector3(0.62f, 1.2f, 0.5f);
            visual.GetComponent<Renderer>().material.color = new Color(0.9f, 0.5f, 0.15f);
            visual.SetActive(false); // примитив-заглушка — поверх ставим блочного персонажа

            // блочный персонаж Kenney (текстурный)
            var character = Art.Model("characters/character-a", Vector3.zero, 180f, 1f, 0f, keepMaterials: true);
            if (character != null)
            {
                character.name = "Character";
                character.transform.SetParent(go.transform, false);
                character.transform.localPosition = new Vector3(0f, 0f, 0f);
                CharacterBob.Attach(character, rb);
            }

            var controller = go.AddComponent<PlayerController>();
            go.AddComponent<GrabSystem>();

            // сетевой каркас (в соло NetworkObject остаётся неспавненным — безвредно)
            go.AddComponent<NetworkRig>();
            go.AddComponent<NetworkObject>();

            if (withCamera) AttachCamera(go.transform, controller);
            return go;
        }

        public static ThirdPersonCamera AttachCamera(Transform target, PlayerController controller)
        {
            var camGo = new GameObject("MainCamera");
            camGo.tag = "MainCamera";
            camGo.transform.position = target.position + new Vector3(0f, 3f, -6f);
            camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            var cam = camGo.AddComponent<ThirdPersonCamera>();
            cam.target = target;
            if (controller != null) controller.CameraTransform = camGo.transform;
            return cam;
        }
    }
}
