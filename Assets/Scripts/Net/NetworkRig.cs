using Unity.Netcode;
using UnityEngine;

namespace DI
{
    /// Сетевой каркас игрока: владелец получает камеру и управление,
    /// копии чужих игроков — кинематические (позицию рулит владелец через NetworkTransform).
    public class NetworkRig : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            var rb = GetComponent<Rigidbody>();
            var pc = GetComponent<PlayerController>();
            var gs = GetComponent<GrabSystem>();

            if (!IsOwner)
            {
                if (rb != null) rb.isKinematic = true;
                if (pc != null) pc.enabled = false;
                if (gs != null) gs.enabled = false;
                return;
            }

            PlayerFactory.AttachCamera(transform, pc);
        }
    }
}
