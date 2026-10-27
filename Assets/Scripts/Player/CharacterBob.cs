using UnityEngine;
using System.Collections.Generic;

namespace DI
{
    /// Оживление блочного персонажа: процедурная походка (руки, ноги, голова),
    /// наклон в сторону движения, покачивание. Вешается на визуальную модель.
    /// Части Kenney (leg-left, arm-right, head...) получают пивоты сверху —
    /// чтобы конечности крутились «от плеча/бедра».
    public class CharacterBob : MonoBehaviour
    {
        Rigidbody body;
        Transform legL, legR, armL, armR, head;
        readonly List<Transform> pivots = new List<Transform>();
        float bobPhase;
        bool initialized;

        public static void Attach(GameObject visual, Rigidbody bodyToTrack)
        {
            var bob = visual.AddComponent<CharacterBob>();
            bob.body = bodyToTrack;
        }

        void Start() => MakePivots();

        /// Пивот над каждым элементом: конечность крутится от верха.
        void MakePivots()
        {
            foreach (var t in transform.GetComponentsInChildren<Transform>())
            {
                if (t == transform) continue;
                switch (t.name)
                {
                    case "leg-left": legL = Pivotize(t); break;
                    case "leg-right": legR = Pivotize(t); break;
                    case "arm-left": armL = Pivotize(t); break;
                    case "arm-right": armR = Pivotize(t); break;
                    case "head": head = t; break;
                }
            }
            pivots.RemoveAll(p => p == null);
        }

        Transform Pivotize(Transform limb)
        {
            var r = limb.GetComponentInChildren<MeshRenderer>();
            var pivotGo = new GameObject(limb.name + "_pivot");
            pivotGo.transform.SetParent(limb.parent, false);
            if (r != null)
                pivotGo.transform.position = r.bounds.center + Vector3.up * (r.bounds.size.y * 0.45f);
            else
                pivotGo.transform.position = limb.position;
            limb.SetParent(pivotGo.transform, true);
            pivots.Add(pivotGo.transform);
            return pivotGo.transform;
        }

        void LateUpdate()
        {
            if (body == null) return;

            Vector3 vel = body.linearVelocity;
            vel.y = 0f;
            float speed = vel.magnitude;
            bobPhase += speed * 7.5f * Time.deltaTime;

            // поворот в сторону движения + лёгкий наклон вперёд
            if (speed > 0.5f)
            {
                Vector3 flat = new Vector3(vel.x, 0f, vel.z).normalized;
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(flat), 9f * Time.deltaTime);
                transform.rotation *= Quaternion.Euler(5f, 0f, 0f);
            }

            bool walking = speed > 0.5f;
            float swing = walking ? Mathf.Sin(bobPhase) : 0f;
            float swingCos = walking ? Mathf.Cos(bobPhase) : 0f;

            if (legL != null) legL.localRotation = Quaternion.Euler(swing * 32f, 0f, 0f);
            if (legR != null) legR.localRotation = Quaternion.Euler(-swing * 32f, 0f, 0f);
            if (armL != null) armL.localRotation = Quaternion.Euler(-swing * 26f, 0f, 0f);
            if (armR != null) armR.localRotation = Quaternion.Euler(swing * 26f, 0f, 0f);
            if (head != null)
                head.localRotation = Quaternion.Euler(walking ? -swingCos * 3f : 0f,
                    walking ? swing * 5f : 0f, 0f);

            // покачивание корпуса
            float bob = walking ? Mathf.Abs(Mathf.Sin(bobPhase)) * 0.05f : 0f;
            var lp = transform.localPosition;
            transform.localPosition = new Vector3(lp.x, bob, lp.z);
        }
    }
}
