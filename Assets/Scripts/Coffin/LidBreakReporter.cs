using UnityEngine;

namespace DI
{
    /// Сообщает менеджеру, что петли крышки сорвало.
    public class LidBreakReporter : MonoBehaviour
    {
        void OnJointBreak(float force)
        {
            Sfx.PlayAt("snap", transform.position);
            DIGameManager.I?.ReportLidBroken();
        }
    }
}
