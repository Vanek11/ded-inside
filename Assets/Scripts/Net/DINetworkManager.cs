using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace DI
{
    /// Поднимает NGO-сеть из кода: хост или клиент по IP, спавн игроков,
    /// перевод динамики уровня (гроб, дед, крышка, гуси) под серверную синхронизацию.
    public class DINetworkManager : MonoBehaviour
    {
        NetworkManager nm;
        bool worldNetworked;

        void FixedUpdate()
        {
            // сервер прикладывает силу за клиентов, держащих сетевые объекты
            if (nm != null && nm.IsServer) ServerGrabExecutor.ExecuteAll();
        }

        public void Bootstrap(SessionMode mode, string ip)
        {
            var go = new GameObject("NetworkManager");
            nm = go.AddComponent<NetworkManager>();
            nm.NetworkConfig = new NetworkConfig(); // в NGO 2.x это обычный класс

            var utp = go.AddComponent<UnityTransport>();
            nm.NetworkConfig.NetworkTransport = utp;
            nm.NetworkConfig.EnableSceneManagement = false; // сцену грузим сами, обе стороны одинаково
            nm.NetworkConfig.ConnectionApproval = true;
            nm.ConnectionApprovalCallback = Approve;
            nm.OnClientConnectedCallback += OnClientConnected;

            if (mode == SessionMode.Host)
            {
                utp.SetConnectionData("0.0.0.0", (ushort)SessionState.Port);
                if (nm.StartHost()) Debug.Log($"[ДИ] Сеть: хост поднят, порт {SessionState.Port}");
                else Debug.LogError("[ДИ] Не удалось поднять хост!");
            }
            else
            {
                utp.SetConnectionData(string.IsNullOrEmpty(ip) ? "127.0.0.1" : ip, (ushort)SessionState.Port);
                if (nm.StartClient()) Debug.Log($"[ДИ] Сеть: подключаюсь к {ip}:{SessionState.Port}...");
                else Debug.LogError("[ДИ] Не удалось начать подключение!");
            }
        }

        void Approve(NetworkManager.ConnectionApprovalRequest request,
                     NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved = true;
            response.CreatePlayerObject = false; // игроков спавним сами
        }

        void OnClientConnected(ulong clientId)
        {
            if (nm.IsServer)
            {
                if (!worldNetworked)
                {
                    RebuildDynamic();
                    worldNetworked = true;
                }
                SpawnPlayer(clientId);
            }
            else
            {
                // клиент: снести локальную динамику сцены (гроб/дед/гуси/игрок),
                // настоящие придут спавном от сервера; снести и сценическую камеру
                DestroyLocalDynamics(true);
            }
        }

        static void DestroyLocalDynamics(bool alsoCamera)
        {
            foreach (var m in FindObjectsByType<NetDynamicMarker>(FindObjectsSortMode.None))
                Destroy(m.gameObject);
            if (alsoCamera)
                foreach (var cam in FindObjectsByType<ThirdPersonCamera>(FindObjectsSortMode.None))
                    Destroy(cam.gameObject);
        }

        /// Пересоздаёт динамику на сервере (чтобы не конфликтовать со сценовыми копиями)
        /// и выдаёт каждому корню NetworkObject + NetworkTransform.
        void RebuildDynamic()
        {
            var game = DIGameManager.I;
            var coffin = game != null && game.coffin != null
                ? game.coffin
                : FindFirstObjectByType<CoffinBody>();

            Vector3 coffinPos = coffin != null ? coffin.transform.position : new Vector3(0f, 0.84f, -19f);
            Quaternion coffinRot = coffin != null ? coffin.transform.rotation : Quaternion.identity;

            var geese = new System.Collections.Generic.List<Vector3>();
            foreach (var js in FindObjectsByType<JumpScare>(FindObjectsSortMode.None))
                geese.Add(js.transform.position);

            DestroyLocalDynamics(false);

            var newCoffin = CoffinFactory.Build(coffinPos, coffinRot);
            var newGrandpa = GrandpaFactory.Build(newCoffin.transform);
            newCoffin.Grandpa = newGrandpa;
            if (game != null)
            {
                game.coffin = newCoffin;
                game.grandpa = newGrandpa;
            }

            foreach (var p in geese)
                LevelVillage1.SpawnGoose(p);

            MakeNetworked(newCoffin.gameObject);
            if (newCoffin.Lid != null) MakeNetworked(newCoffin.Lid.gameObject);
            foreach (var part in newGrandpa.GetComponentsInChildren<GrandpaPart>())
                MakeNetworked(part.gameObject);

            Debug.Log("[ДИ] Динамика уровня отдана под серверную синхронизацию");
        }

        static void MakeNetworked(GameObject go)
        {
            var nob = go.AddComponent<NetworkObject>();
            go.AddComponent<NetworkTransform>();
            nob.Spawn(true);
        }

        void SpawnPlayer(ulong clientId)
        {
            bool hostLocal = clientId == nm.LocalClientId;
            var go = PlayerFactory.BuildPlayer(new Vector3(1.5f, 1.2f, -15f), false);

            if (!hostLocal)
            {
                // серверная копия игрока клиента — кинематик: позицию рулит его владелец
                var rb = go.GetComponent<Rigidbody>();
                rb.isKinematic = true;
            }

            var nt = go.AddComponent<NetworkTransform>();
            nt.AuthorityMode = NetworkTransform.AuthorityModes.Owner;

            var nob = go.GetComponent<NetworkObject>();
            nob.SpawnAsPlayerObject(clientId, true);
        }
    }
}
