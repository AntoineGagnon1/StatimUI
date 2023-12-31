﻿using StatimCodeGenerator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace StatimUI
{
    public interface IEasing
    {
        public float Evaluate(float t);
    }

    public class Hold : IEasing
    {
        public float Evaluate(float t) => 0f;
    }
    public class Linear : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.Linear(t);
    }

    public class InQuad : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.InQuad(t);
    }


    public class OutQuad : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.OutQuad(t);
    }


    public class InOutQuad : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.InOutQuad(t);
    }


    public class InCubic : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.InCubic(t);
    }


    public class OutCubic : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.OutCubic(t);
    }

    public class InOutCubic : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.InOutCubic(t);
    }


    public class InQuart : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.InQuart(t);
    }


    public class OutQuart : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.OutQuart(t);
    }


    public class InOutQuart : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.InOutQuart(t);
    }

    public class InQuint : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.InQuint(t);
    }

    public class OutQuint : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.OutQuint(t);
    }

    public class InOutQuint : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.InOutQuint(t);
    }

    public class InSine : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.InSine(t);
    }

    public class OutSine : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.OutSine(t);
    }

    public class InOutSine : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.InOutSine(t);
    }

    public class InExpo : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.InExpo(t);
    }

    public class OutExpo : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.OutExpo(t);
    }

    public class InOutExpo : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.InOutExpo(t);
    }

    public class InCirc : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.InCirc(t);
    }

    public class OutCirc : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.OutCirc(t);
    }

    public class InOutCirc : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.InOutCirc(t);
    }

    public class InElastic : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.InElastic(t);
    }

    public class OutElastic : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.OutElastic(t);
    }

    public class InOutElastic : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.InOutElastic(t);
    }

    public class InBack : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.InBack(t);
    }

    public class OutBack : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.OutBack(t);
    }

    public class InOutBack : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.InOutBack(t);
    }

    public class InBounce : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.InBounce(t);
    }

    public class OutBounce : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.OutBounce(t);
    }

    public class InOutBounce : IEasing
    {
        public float Evaluate(float t) => EasingFunctions.InOutBounce(t);
    }

    public static class EasingFunctions
    {
        public static float Linear(float t) => t;

        public static float InQuad(float t) => t * t;
        public static float OutQuad(float t) => 1 - InQuad(1 - t);
        public static float InOutQuad(float t)
        {
            if (t < 0.5) return InQuad(t * 2) / 2;
            return 1 - InQuad((1 - t) * 2) / 2;
        }

        public static float InCubic(float t) => t * t * t;
        public static float OutCubic(float t) => 1 - InCubic(1 - t);
        public static float InOutCubic(float t)
        {
            if (t < 0.5) return InCubic(t * 2) / 2;
            return 1 - InCubic((1 - t) * 2) / 2;
        }

        public static float InQuart(float t) => t * t * t * t;
        public static float OutQuart(float t) => 1 - InQuart(1 - t);
        public static float InOutQuart(float t)
        {
            if (t < 0.5) return InQuart(t * 2) / 2;
            return 1 - InQuart((1 - t) * 2) / 2;
        }

        public static float InQuint(float t) => t * t * t * t * t;
        public static float OutQuint(float t) => 1 - InQuint(1 - t);
        public static float InOutQuint(float t)
        {
            if (t < 0.5) return InQuint(t * 2) / 2;
            return 1 - InQuint((1 - t) * 2) / 2;
        }

        public static float InSine(float t) => (1f - (float)Math.Cos((t * (float)Math.PI) / 2f));
        public static float OutSine(float t) => (float)Math.Sin(t * Math.PI / 2);
        public static float InOutSine(float t) => (float)(Math.Cos(t * Math.PI) - 1) / -2;

        public static float InExpo(float t) => (float)Math.Pow(2, 10 * (t - 1));
        public static float OutExpo(float t) => 1 - InExpo(1 - t);
        public static float InOutExpo(float t)
        {
            if (t < 0.5) return InExpo(t * 2) / 2;
            return 1 - InExpo((1 - t) * 2) / 2;
        }

        public static float InCirc(float t) => -((float)Math.Sqrt(1 - t * t) - 1);
        public static float OutCirc(float t) => 1 - InCirc(1 - t);
        public static float InOutCirc(float t)
        {
            if (t < 0.5) return InCirc(t * 2) / 2;
            return 1 - InCirc((1 - t) * 2) / 2;
        }

        public static float InElastic(float t) => 1 - OutElastic(1 - t);
        public static float OutElastic(float t)
        {
            float p = 0.3f;
            return (float)Math.Pow(2, -10 * t) * (float)Math.Sin((t - p / 4) * (2 * Math.PI) / p) + 1;
        }
        public static float InOutElastic(float t)
        {
            if (t < 0.5) return InElastic(t * 2) / 2;
            return 1 - InElastic((1 - t) * 2) / 2;
        }

        public static float InBack(float t)
        {
            float s = 1.70158f;
            return t * t * ((s + 1) * t - s);
        }
        public static float OutBack(float t) => 1 - InBack(1 - t);
        public static float InOutBack(float t)
        {
            if (t < 0.5) return InBack(t * 2) / 2;
            return 1 - InBack((1 - t) * 2) / 2;
        }

        public static float InBounce(float t) => 1 - OutBounce(1 - t);
        public static float OutBounce(float t)
        {
            float div = 2.75f;
            float mult = 7.5625f;

            if (t < 1 / div)
            {
                return mult * t * t;
            }
            else if (t < 2 / div)
            {
                t -= 1.5f / div;
                return mult * t * t + 0.75f;
            }
            else if (t < 2.5 / div)
            {
                t -= 2.25f / div;
                return mult * t * t + 0.9375f;
            }
            else
            {
                t -= 2.625f / div;
                return mult * t * t + 0.984375f;
            }
        }
        public static float InOutBounce(float t)
        {
            if (t < 0.5) return InBounce(t * 2) / 2;
            return 1 - InBounce((1 - t) * 2) / 2;
        }
    }
}
