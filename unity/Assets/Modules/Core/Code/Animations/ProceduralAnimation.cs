using System;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.Core.Animations
{
    public abstract class ProceduralAnimation<T>
    {
        public delegate void UpdateHandler(T currentValue);
        public event UpdateHandler Update;
        public event Action Finished;

        public T CurrentValue { get; private set; }
        public float AnimationSpeed { get; set; }

        private float _startTime = 0f;
        private bool _isRunning = false;
        public bool IsRunning { get { return _isRunning; } }
        private T _end;
        public T End { get { return _end; } }
        private T _start;

        public ProceduralAnimation(float animationSpeed)
        {
            AnimationSpeed = animationSpeed;
        }

        public void Init(T startValue)
        {
            Stop();
            CurrentValue = startValue;
        }

        public void Start(T start, T end)
        {
            _start = start;
            _end = end;
            CurrentValue = start;
            _startTime = Time.time;

            if (!_isRunning)
            {
                _isRunning = true;
                GameLoop.Instance.StartRoutine(RunAnimation());
            }
        }

        public void Restart(T end)
        {
            Restart(CurrentValue, end);
        }

        public void Restart(T start, T end)
        {
            if (_isRunning)
            {
                _startTime = Time.time;
                _start = CurrentValue;
                _end = end;
            }
            else
            {
                Start(CurrentValue, end);
            }
        }


        public void Adjust(T adjustedEnd)
        {
            // TODO: adjust start, time to avoid possible jumps if
            // adjusted value is greatly different than previous value
            _end = adjustedEnd;
        }

        public void Stop()
        {
            // stops the animation routine the next time it's running,
            // preventing a rather costly StopRoutine call
            _startTime = 0f;
        }


        private IEnumerator RunAnimation()
        {
            var timeDelta = 0f;
            while (timeDelta < 1f)
            {
                yield return new WaitForEndOfFrame();

                timeDelta = (Time.time - _startTime) / AnimationSpeed;

                if (_isRunning)
                {
                    CurrentValue = Lerp(_start, _end, timeDelta);

                    if (Update != null)
                    {
                        Update(CurrentValue);
                    }
                }
            }

            if (Finished != null)
            {
                Finished();
            }

            _isRunning = false;
        }

        protected abstract T Lerp(T start, T end, float weight);
    }
}
