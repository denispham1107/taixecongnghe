using UnityEngine;

namespace PhoViet
{
    internal struct WheelRolling
    {
        const float CrashDeceleration = 20f;
        float speed;

        public float Advance(float dt, float roadSpeed, bool coasting)
        {
            if (dt <= 0) return 0;
            if (!coasting)
            {
                speed = Mathf.Abs(roadSpeed);
                return speed * dt;
            }

            float coastTime = Mathf.Min(dt, speed / CrashDeceleration);
            float previousSpeed = speed;
            speed = Mathf.Max(0, speed - CrashDeceleration * dt);
            return (previousSpeed + speed) * .5f * coastTime;
        }

        public static float Rotate(float angle, float distance, float radius)
            => Mathf.Repeat(angle + distance / Mathf.Max(.001f, radius) * Mathf.Rad2Deg, 360f);
    }
}
