using UnityEngine;

namespace DI
{
    /// Ворчливый смотритель кладбища: стоит у ограды и комментирует доставку.
    public class GrumpCaretaker : MonoBehaviour
    {
        static readonly string[] Lines =
        {
            "Тише! Он спит...",
            "В моё время гробы носили с уважением!",
            "Аккуратнее с газоном, разгильдяи.",
            "Опять небрежная доставка... Куда смотрит профком?",
            "Гроб должен ехать В яму, а не ВОКРУГ неё.",
            "Поставьте ровно! РОВ-НО!",
            "И часто вы так с покойными обращаетесь?",
        };

        Transform player;
        float next = 4f;

        void Start()
        {
            var p = FindFirstObjectByType<PlayerController>();
            if (p != null) player = p.transform;
        }

        void Update()
        {
            if (player == null || DIGameManager.I == null || DIGameManager.I.Phase != OrderPhase.Carry) return;
            next -= Time.deltaTime;
            if (next > 0f) return;
            if ((player.position - transform.position).sqrMagnitude < 36f)
            {
                DIGameManager.I.Event($"Смотритель: «{Lines[Random.Range(0, Lines.Length)]}»");
                next = Random.Range(9f, 16f);
            }
        }
    }
}
