using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace StatimUI
{
    public class AnimationDesc
    {
        public readonly Component Component;
        public readonly IAnimation<Component> Animation;
        public readonly float Duration;
        public readonly IEasing Easing = new Linear();
        public int IterationCount = 1;
        public float CurrentTime  = 0f;
        public bool Paused = false;


        public AnimationDesc(Component component, IAnimation<Component> animation, float duration)
        {
            Component = component;
            Animation = animation;
            Duration = duration;
        }

        public AnimationDesc(Component component, IAnimation<Component> animation, float duration, IEasing easing, int iterationCount = 1, float delay = 0f, bool paused = false)
        {
            Component = component;
            Animation = animation;
            Duration = duration;
            Easing = easing;
            IterationCount = iterationCount;
            CurrentTime = -delay; // doing this because we want to start at eg. -3 seconds and when it will it 0 seconds the animation will start
            Paused = paused;
        }
    }
}
