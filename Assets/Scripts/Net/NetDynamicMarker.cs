using UnityEngine;

namespace DI
{
    /// Маркер «этот объект в сетевой смене должен жить только на сервере».
    /// При подключении клиенты уничтожают помеченные локальные копии,
    /// сервер пересоздаёт их и раздаёт через NetworkObject.
    public class NetDynamicMarker : MonoBehaviour { }
}
