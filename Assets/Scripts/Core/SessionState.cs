using UnityEngine;

namespace DI
{
    /// Режим этой смены, копилка бюро, звёзды заказов и купленное снаряжение.
    public enum SessionMode { Solo, Host, Client }

    public static class SessionState
    {
        public static SessionMode Mode = SessionMode.Solo;
        public static string Ip = "127.0.0.1";
        public const int Port = 7777;

        public static int Money => PlayerPrefs.GetInt("di_money", 0);

        public static void AddMoney(int amount) => PlayerPrefs.SetInt("di_money", Money + Mathf.Max(0, amount));

        // --- прогрессия заказов ---

        public static int Stars(string sceneName) => PlayerPrefs.GetInt($"di_stars_{sceneName}", 0);

        public static void SetStars(string sceneName, int stars)
        {
            if (stars > Stars(sceneName)) PlayerPrefs.SetInt($"di_stars_{sceneName}", stars);
        }

        public static bool IsUnlocked(string sceneName)
        {
            switch (sceneName)
            {
                case "Village1": return true;
                case "Village2": return Stars("Village1") >= 1;
                case "Village3": return Stars("Village2") >= 1;
                default: return false;
            }
        }

        // --- лавка снаряжения ---

        public static bool HasGear(string id) => PlayerPrefs.GetInt($"di_gear_{id}", 0) == 1;

        public static bool BuyGear(string id, int price)
        {
            if (HasGear(id) || Money < price) return false;
            PlayerPrefs.SetInt($"di_gear_{id}", 1);
            PlayerPrefs.SetInt("di_money", Money - price);
            return true;
        }
    }
}
