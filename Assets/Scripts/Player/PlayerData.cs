using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Player
{
    public struct UserInput
    {
        public float X;
        public float Y;
        public bool Jump;
        public bool HoldJump;
        public bool Dash;
    }

    public struct CollisionSet
    {
        public bool Up;
        public bool Left;
        public bool Right;
        public bool Down;
    }

    public struct RaySet
    {
        public bool X;
        public float Start;
        public float End;
        public float Other;
        public Vector2 Dir;

        public IEnumerable<Vector2> GetOrigins(int extraRays) {
            var list = new List<Vector2>(2 + extraRays);
            for (float value = Start; value <= End + 0.01f; value += (End - Start) / (1 + extraRays)) {
                list.Add(new Vector2(X ? value : Other, X ? Other : value));
            }
            return list;
        }
    }
}