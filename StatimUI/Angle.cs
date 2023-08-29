using System;
using System.Collections.Generic;
using System.Text;

namespace StatimUI
{
    public struct Angle : IEquatable<Angle>
    {
        public static readonly Angle Empty = new Angle();

        private static float PI = (float)Math.PI;

        public float Radians { get; set; }
        public float Degrees
        {
            get => Radians * (180f / PI);
            set => Radians = value * (PI / 180f);
        }
        public float Turns
        {
            get => Radians * (50f / PI);
            set => Radians = value * (PI / 50f);
        }

        public static Angle FromRadians(float radians) => new Angle { Radians = radians };
        public static Angle FromDegrees(float degrees) => new Angle {  Degrees = degrees };
        public static Angle FromTurns(float turns) => new Angle { Turns = turns };


        public static bool operator ==(Angle a, Angle b) => a.Equals(b);
        public static bool operator !=(Angle a, Angle b) => !a.Equals(b);
        public bool Equals(Angle other) => Radians == other.Radians;
    }
}
