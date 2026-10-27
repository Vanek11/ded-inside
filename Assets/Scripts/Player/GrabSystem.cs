using Unity.Netcode;
using UnityEngine;

namespace DI
{
    /// Физический захват «как в Moving Out»: держишь кнопку — пружина тянет точку
    /// захвата к руке. Точка руки едет по направлению взгляда, высота — мышью.
    /// Сила ограничена: один грузчик гроб еле тащит, двое — несут, четверо — летят.
    /// В сети: у клиента сила не применяется локально — руку передаёт серверу
    /// (ServerGrabExecutor), чтобы физика не рассыпалась.
    public class GrabSystem : NetworkBehaviour
    {
        public float reach = 3.4f;
        public float holdDistance = 1.4f; // на каком расстоянии от тела висит схваченное
        public float spring = 3000f;
        public float maxForcePerHand = 1100f;
        public float heavyMass = 60f;
        public float reactionOnPlayer = 0.55f; // сколько веса чувствуем (горизонтальная отдача)
        public float mouseLiftSpeed = 2.4f;    // скорость подъёма руки мышью, м/с

        Transform hand;
        Rigidbody held;
        Vector3 localPoint;
        float heldMass = 1f;
        float holdHeight = 0.9f;
        bool canGrab;
        ThirdPersonCamera camCtrl;

        public bool IsHolding => held != null;
        public bool IsCarryingHeavy => held != null && held.mass >= heavyMass;

        void Awake()
        {
            hand = new GameObject("HandAnchor").transform;
            hand.SetParent(transform, false);
        }

        void Update()
        {
            if (PauseMenu.Open) return; // на паузе руки не слушаются

            bool want = Input.GetMouseButton(0);
            if (want && held == null) TryGrab();
            else if (!want && held != null)
            {
                ReleaseHeld();
            }
            UpdateHandAnchor();
            UpdateCanGrab();

            if (camCtrl == null) camCtrl = GetComponentInChildren<ThirdPersonCamera>();
            if (camCtrl != null) camCtrl.freezePitch = held != null;
        }

        /// Прицел подскажет: жёлтый — до цели дотягиваешься.
        void UpdateCanGrab()
        {
            canGrab = false;
            if (held != null) return;
            var cam = Camera.main;
            if (cam == null) return;

            int mask = LayerMask.GetMask("Grabbable", "Grandpa");
            if (mask == 0) mask = ~0;
            if (!Physics.Raycast(cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)), out var hit, 100f, mask))
                return;

            canGrab = Vector3.Distance(transform.position, hit.point) <= reach
                      && hit.rigidbody != null && !hit.rigidbody.isKinematic;
        }

        void ReleaseHeld()
        {
            if (held != null && IsRemoteControlled(held) && NetworkManager.Singleton != null
                && NetworkManager.Singleton.IsClient)
            {
                ReleaseNetServerRpc();
            }
            held = null;
            holdHeight = 0.9f;
        }

        static bool IsRemoteControlled(Rigidbody body)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null || body == null) return false;
            return body.GetComponent<NetworkObject>() != null && !nm.IsServer;
        }

        /// Горизонт — по направлению взгляда, высота — напрямую мышью:
        /// тянешь мышь вверх при удержании, рука едет вверх (до 2.6 м).
        void UpdateHandAnchor()
        {
            var cam = Camera.main;
            if (cam == null) return;

            if (held != null)
                holdHeight = Mathf.Clamp(holdHeight + Input.GetAxis("Mouse Y") * mouseLiftSpeed * Time.deltaTime,
                    0.25f, 2.6f);

            Vector3 flat = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
            if (flat.sqrMagnitude < 0.01f) flat = Vector3.forward;

            hand.position = transform.position + flat * holdDistance + Vector3.up * holdHeight;
        }

        void TryGrab()
        {
            var cam = Camera.main;
            if (cam == null) return;

            int mask = LayerMask.GetMask("Grabbable", "Grandpa");
            if (mask == 0) mask = ~0;

            // луч — через центр экрана, но дистанция захвата меряется ОТ ИГРОКА,
            // а не от камеры (камера висит в 5-6 м позади спины)
            if (!Physics.Raycast(cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)), out var hit, 100f, mask))
                return;

            if (Vector3.Distance(transform.position, hit.point) > reach) return;

            var target = hit.rigidbody;
            if (target == null || target.isKinematic) return;
            if (target == GetComponent<Rigidbody>()) return; // за себя хватать нельзя

            held = target;
            heldMass = Mathf.Max(1f, target.mass);
            localPoint = target.transform.InverseTransformPoint(hit.point);
            DIGameManager.I?.Event("Взялись!");
        }

        void FixedUpdate()
        {
            if (held == null) return;

            // клиент и сетевой объект: физику рулит сервер, шлём позицию руки
            if (IsRemoteControlled(held))
            {
                var nm = NetworkManager.Singleton;
                if (nm != null && nm.IsConnectedClient)
                {
                    var nob = held.GetComponent<NetworkObject>();
                    if (nob != null)
                        GrabNetServerRpc(nob, localPoint, hand.position);
                }
                return;
            }

            // демпфер критический относительно массы схваченного: без «резинки»
            // и без вечных колебаний и для 10-кг крышки, и для 120-кг гроба
            float damp = 2f * Mathf.Sqrt(spring * heldMass) * 0.9f;

            Vector3 point = held.transform.TransformPoint(localPoint);
            Vector3 vel = held.GetPointVelocity(point);
            Vector3 force = (hand.position - point) * spring - vel * damp;

            // лёгким предметам — лёгкая сила (иначе трость дёргается как бешеная);
            // гроб по-прежнему упирается на полном пределе
            float maxForce = Mathf.Min(maxForcePerHand, heldMass * 60f + 60f);

            // эксплойт «полёт на схваченном»: стоя на объекте вертикальную силу гасим
            if (Physics.SphereCast(transform.position + Vector3.up * 0.6f, 0.3f,
                    Vector3.down, out var groundHit, 0.72f)
                && groundHit.rigidbody == held)
                force.y = 0f;

            if (force.sqrMagnitude > maxForce * maxForce)
                force = force.normalized * maxForce;
            held.AddForceAtPosition(force, point);

            // отдача веса на игрока: горизонтальная составляющая тянет назад,
            // вертикальную гасим (иначе персонажа подкидывает)
            var prb = GetComponent<Rigidbody>();
            Vector3 reaction = -force * reactionOnPlayer;
            prb.AddForce(new Vector3(reaction.x, 0f, reaction.z));
        }

        [ServerRpc(RequireOwnership = false)]
        void GrabNetServerRpc(NetworkObjectReference target, Vector3 grabLocalPoint, Vector3 handPos,
            ServerRpcParams rpcParams = default)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;
            if (target.TryGet(out var nob))
                ServerGrabExecutor.Set(rpcParams.Receive.SenderClientId, nob, grabLocalPoint, handPos);
        }

        [ServerRpc(RequireOwnership = false)]
        void ReleaseNetServerRpc(ServerRpcParams rpcParams = default)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;
            ServerGrabExecutor.Clear(rpcParams.Receive.SenderClientId);
        }

        void OnGUI()
        {
            // прицел: белый — пусто, жёлтый — можно схватить, зелёный — держим
            var prev = GUI.color;
            GUI.color = held != null ? new Color(0.4f, 1f, 0.4f)
                : canGrab ? new Color(1f, 0.9f, 0.25f)
                : Color.white;
            var style = new GUIStyle(GUI.skin.label) { fontSize = 26, alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(Screen.width / 2f - 30f, Screen.height / 2f - 25f, 60f, 50f),
                held != null ? "\u25CF" : "+", style);
            GUI.color = prev;
        }
    }
}
