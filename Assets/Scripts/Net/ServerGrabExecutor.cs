using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace DI
{
    /// Активный удалённый захват: клиент держит объект — сервер прикладывает силу.
    public class NetGrab
    {
        public NetworkObject target;
        public Vector3 localPoint;
        public Vector3 handPos;
    }

    /// Серверная часть захватов клиентов: применяет пружину к сетевым объектам.
    public static class ServerGrabExecutor
    {
        public const float Spring = 3000f;
        public const float MaxForce = 1100f;

        static readonly Dictionary<ulong, NetGrab> active = new Dictionary<ulong, NetGrab>();

        public static void Set(ulong clientId, NetworkObject target, Vector3 localPoint, Vector3 handPos)
        {
            active[clientId] = new NetGrab { target = target, localPoint = localPoint, handPos = handPos };
        }

        public static void Clear(ulong clientId)
        {
            if (active.ContainsKey(clientId)) active.Remove(clientId);
        }

        public static void ExecuteAll()
        {
            foreach (var kv in active)
            {
                var g = kv.Value;
                if (g.target == null) continue;
                var rb = g.target.GetComponent<Rigidbody>();
                if (rb == null) continue;

                float damp = 2f * Mathf.Sqrt(Spring * Mathf.Max(1f, rb.mass)) * 0.9f;
                Vector3 point = g.target.transform.TransformPoint(g.localPoint);
                Vector3 vel = rb.GetPointVelocity(point);
                Vector3 force = (g.handPos - point) * Spring - vel * damp;
                if (force.sqrMagnitude > MaxForce * MaxForce)
                    force = force.normalized * MaxForce;
                rb.AddForceAtPosition(force, point);
            }
        }
    }
}
