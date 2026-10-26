using UnityEngine;

namespace DI
{
    /// Режим этой смены и копилка бюро (соло-прогресс между запусками).
    public enum SessionMode { Solo, Host, Client }

    public static class SessionState
    {
        public static SessionMode Mode = SessionMode.Solo;
        public static string Ip = "127.0.0.1";
        public const int Port = 7777;

        public static int Money => PlayerPrefs.GetInt("di_money", 0);

        public static void AddMoney(int amount) => PlayerPrefs.SetInt("di_money", Money + Mathf.Max(0, amount));
    }
}
