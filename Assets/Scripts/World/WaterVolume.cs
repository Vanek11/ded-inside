using System.Collections.Generic;
using UnityEngine;

namespace DI
{
    /// Вода: гроб плавает (архимедова сила > веса), дед тонет медленно (< веса).
    public class WaterVolume : MonoBehaviour
    {
        public float surfaceY;
        public float coffinBuoyancy = 2.1f;
        public float grandpaBuoyancy = 0.82f;

        readonly HashSet<Rigidbody> doneThisStep = new HashSet<Rigidbody>();
        static float lastSplash;

        void FixedUpdate() => doneThisStep.Clear();

        void OnTriggerEnter(Collider other)
        {
            var rb = other.attachedRigidbody;
            if (rb == null || rb.isKinematic) return;
            if (rb.GetComponent<CoffinBody>() == null && rb.GetComponent<GrandpaPart>() == null) return;
            if (Time.time - lastSplash < 0.4f) return;
            lastSplash = Time.time;
            Sfx.PlayAt("splash", rb.worldCenterOfMass);
        }

        void OnTriggerStay(Collider other)
        {
            var rb = other.attachedRigidbody;
            if (rb == null || rb.isKinematic) return;
            if (!doneThisStep.Add(rb)) return; // составной коллайдер = одна сила на тело

            float factor;
            if (rb.GetComponent<CoffinBody>() != null) factor = coffinBuoyancy;
            else if (rb.GetComponent<GrandpaPart>() != null) factor = grandpaBuoyancy;
            else return;

            float depth = Mathf.Clamp01((surfaceY - rb.worldCenterOfMass.y + 0.35f) / 0.7f);
            rb.AddForce(Vector3.up * rb.mass * 9.81f * factor * depth);
        }
    }
}
