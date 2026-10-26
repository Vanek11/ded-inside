using UnityEngine;

namespace DI
{
    /// Сообщает менеджеру, что петли крышки сорвало.
    public class LidBreakReporter : MonoBehaviour
    {
        void OnJointBreak(float force) => DIGameManager.I?.ReportLidBroken();
    }
}
