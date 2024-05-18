using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;

namespace Xiaobo.Parameter
{
    public enum EnumAnimationType
    {
        None,
        Easing,
        Unlimited,
        Limited,
        Wrap,
        Pingpong
    }
    #region Interface
    public interface IAdjustableParameter
    {
        void Update(float delta_time);
        void CopyFrom(object obj);
    }
    public interface IXmlSettings
    {
        void WriteToXmlElement(XmlElement ele);
        void LoadFromXmlElement(XmlElement ele);
    }
    #endregion

    #region BaseClass
    public class AdjustableParameter<T> : IAdjustableParameter, IXmlSettings
    {
        // Original elements
        protected T curValue;
        protected T lastValue;
        public T DefaultValue { get; set; }
        private T minValue;
        public T MinValue
        {
            get
            {
                return minValue;
            }
            set
            {
                minValue = value;
                ResetElapsedTimeBasedOnCurrentValue();
            }
        }
        private T maxValue;
        public T MaxValue 
        { 
            get
            {
                return maxValue;
            }
            set 
            {
                maxValue = value;
                ResetElapsedTimeBasedOnCurrentValue();
            } 
        }
        

        // For Easing
        protected T srcValue;
        protected T dstValue;

        public bool NeedClamp { get; set; } = false;

        // For Looping
        private float speed = 1;
        public float Speed
        {
            get => speed;
            set
            {
                speed = Mathf.Abs(value);
            }
        }
        private int pingpongCounter = 0;
        private bool bReverse = false;
        public bool Reverse
        {
            get
            {
                return bReverse;
            }
            set
            {
                bReverse = value;
                //elapsedTime = Duration - elapsedTime;
                ResetElapsedTimeBasedOnCurrentValue();
            }
        }
        private void ResetElapsedTimeBasedOnCurrentValue()
        {
            if (animationType == EnumAnimationType.None)
                return;

            if(animationType == EnumAnimationType.Easing)
            {
                elapsedTime = PercentageFunc(srcValue, dstValue, curValue) * Duration;
                return;
            }

            elapsedTime = PercentageFunc(MinValue, MaxValue, curValue) * Duration;

            if(animationType == EnumAnimationType.Pingpong && pingpongCounter % 2 == 1)
                elapsedTime = Duration - elapsedTime;

            if(Reverse)
                elapsedTime = Duration - elapsedTime;
        }

        // Set / Get Value
        public virtual T Value
        {
            get
            {
                return curValue;
            }

            set
            {
                if (NeedClamp)
                    value = ClampFunc(value);

                if (AnimationType == EnumAnimationType.None)
                {
                    lastValue = curValue;
                    curValue = value;
                    postActionAfterUpdate?.Invoke();
                    if (curValue.Equals(lastValue) == false)
                        onValueChaned?.Invoke();
                }
                else if(AnimationType == EnumAnimationType.Easing)
                {
                    srcValue = curValue;
                    dstValue = value;
                    elapsedTime = 0;
                }
                else
                {
                    if (AnimationType != EnumAnimationType.Unlimited)
                        value = ClampFunc(value);

                    lastValue = curValue;
                    curValue = value;
                    postActionAfterUpdate?.Invoke();
                    if (curValue.Equals(lastValue) == false)
                        onValueChaned?.Invoke();

                    ResetElapsedTimeBasedOnCurrentValue();

                    pingpongCounter = 0;
                }
            }
        }
        
        private float duration = 1;
        public float Duration
        {
            get
            {
                return duration;
            }
            set
            {
                //elapsedTime = value * Progress;
                duration = value;
                ResetElapsedTimeBasedOnCurrentValue();
            }
        }
        protected float elapsedTime = 0;
        public float Progress
        {
            get
            {
                if (AnimationType == EnumAnimationType.None) 
                    return 1;

                if (AnimationType == EnumAnimationType.Unlimited)
                {
                    return duration != 0 ? (elapsedTime / duration) : 1;
                }
                else if(AnimationType == EnumAnimationType.Pingpong)
                {
                    return pingpongCounter%2==0 ? Mathf.Min(duration != 0 ? (elapsedTime / duration) : 1, 1) : 1 - Mathf.Min(duration != 0 ? (elapsedTime / duration) : 1, 1);
                }
                else
                {
                    return Mathf.Min(duration != 0 ? (elapsedTime / duration) : 1, 1);
                }
            }
            set
            {
                if (AnimationType == EnumAnimationType.None)
                    return;

                if (AnimationType == EnumAnimationType.Easing)
                    return;

                if(AnimationType == EnumAnimationType.Unlimited)
                    elapsedTime = Reverse ? (duration * (1-value)) : (duration * value);
                else
                {
                    elapsedTime = Reverse ? (duration * (1 - Mathf.Clamp01(value))) : (duration * Mathf.Clamp01(value));
                    if (animationType == EnumAnimationType.Pingpong && pingpongCounter % 2 == 1)
                        elapsedTime = Duration - elapsedTime;
                }
                    
            }
        }

        private EnumAnimationType animationType = EnumAnimationType.None;
        public EnumAnimationType AnimationType 
        {
            get
            {
                return animationType;
            }
            set
            {
                if (animationType == value) return;

                animationType = value;

                switch (animationType)
                {
                    case EnumAnimationType.None:

                        break;
                    case EnumAnimationType.Easing:
                        srcValue = curValue;
                        if(NeedClamp)
                            dstValue = ClampFunc(curValue);
                        else
                            dstValue = curValue;
                        elapsedTime = PercentageFunc(srcValue, dstValue, curValue) * Duration;
                        break;
                    case EnumAnimationType.Unlimited:
                        ResetElapsedTimeBasedOnCurrentValue();
                        break;
                    case EnumAnimationType.Limited:
                    case EnumAnimationType.Wrap:
                    case EnumAnimationType.Pingpong:
                        curValue = ClampFunc(curValue);
                        if (curValue.Equals(lastValue) == false)
                        {
                            postActionAfterUpdate?.Invoke();
                            onValueChaned?.Invoke();
                        }
                        ResetElapsedTimeBasedOnCurrentValue();
                        pingpongCounter = 0;
                        break;
                }
            }
        }
        
        
        public float Timer { get; protected set; } = 0;
        

        protected Func<float, float> easingFunc = Linear;
        public Func<float, float> EasingFunc
        {
            get => easingFunc;
            set => easingFunc = value;
        }
        private static float Linear(float t) => t;

        public Action postActionAfterUpdate;
        public event Action onValueChaned;
        public Action onEasingComplete;

        public AdjustableParameter()
        {
            //NeedClamp = false;
            //Easing = false;
            //EasingTime = 1f;
            //easingElapsedTime = 0f;
            //easingFunc = Linear;
        }

        public void InitParameter(T cur_value, T default_value, T min_value, T max_value, bool need_clamp, EnumAnimationType animation_type, float duration, float speed)
        {
            curValue = cur_value;
            DefaultValue = default_value;
            lastValue = dstValue = curValue;

            MinValue = min_value;
            MaxValue = max_value;
            NeedClamp = need_clamp;

            AnimationType = animation_type;
            Duration = duration;
            Speed = speed;
        }

        public virtual void Update(float delta_time)
        {
            if (AnimationType == EnumAnimationType.None) return;

            lastValue = curValue;
            
            delta_time *= speed;
            elapsedTime += delta_time;

            float t = 1;
            switch (AnimationType)
            {
                case EnumAnimationType.Easing:
                    if (elapsedTime > Duration)
                        elapsedTime = Duration;
                    t = Duration != 0 ? (elapsedTime / Duration) : 1;
                    break;
                case EnumAnimationType.Unlimited:
                    t = Duration != 0 ? (elapsedTime / Duration) : 1;
                    break;
                case EnumAnimationType.Limited:
                    if (elapsedTime > Duration)
                        elapsedTime = Duration;
                    t = Duration != 0 ? (elapsedTime / Duration) : 1;
                    break;
                case EnumAnimationType.Wrap:
                    if (elapsedTime > Duration)
                        elapsedTime = elapsedTime - Duration;
                    t = Duration != 0 ? (elapsedTime / Duration) : 1;
                    break;
                case EnumAnimationType.Pingpong:
                    if (elapsedTime > Duration)
                    {
                        pingpongCounter++;
                        elapsedTime = elapsedTime - Duration;
                    }
                    t = Duration != 0 ? (elapsedTime / Duration) : 1;
                    t = pingpongCounter % 2 == 0 ? t : (1 - t);
                    break;
            }


            if(AnimationType == EnumAnimationType.Easing)
            {
                curValue = LerpFunc(srcValue, dstValue, t);
            }
            else
            {
                if (Reverse)
                    t = 1 - t;

                curValue = LerpFunc(MinValue, MaxValue, t);
            }
                
            postActionAfterUpdate?.Invoke();
            if (curValue.Equals(lastValue) == false)
            {
                onValueChaned?.Invoke();
                if(AnimationType == EnumAnimationType.Easing && t >= 1)
                {
                    onEasingComplete?.Invoke();
                }    
            }
                

            Timer += delta_time;
        }
        public virtual T Map01ToRange(float t)
        {
            t = Mathf.Clamp01(t);
            return LerpFunc(MinValue, MaxValue, t);
        }
        protected virtual T ClampFunc(T v)
        {
            throw new NotImplementedException();
        }

        protected virtual T LerpFunc(T start, T end, float t)
        {
            throw new NotImplementedException();
        }
        protected virtual float PercentageFunc(T start, T end, T v)
        {
            throw new NotImplementedException();
        }

        public virtual void SetCurrentValueAsDefault()
        {
            DefaultValue = Value;
        }

        public virtual void SetDefaultValue(T v)
        {
            if (NeedClamp)
                v = ClampFunc(v);

            DefaultValue = v;
        }

        public virtual void RestoreToDefault()
        {
            Value = DefaultValue;
        }

        public virtual void WriteToXmlElement(XmlElement ele)
        {
            ele.SetAttribute("type", this.GetType().FullName);

            ele.SetAttribute("cur_value", Value.ToString());
            ele.SetAttribute("default_value", DefaultValue.ToString());
            ele.SetAttribute("min_value", MinValue.ToString());
            ele.SetAttribute("max_value", MaxValue.ToString());
            ele.SetAttribute("need_clamp", NeedClamp.ToString());

            ele.SetAttribute("animation_type", AnimationType.ToString());
            ele.SetAttribute("duration", Duration.ToString());
            ele.SetAttribute("speed", Speed.ToString());
        }

        public virtual void LoadFromXmlElement(XmlElement ele)
        {
            EnumAnimationType animation_type = EnumAnimationType.None;
            Enum.TryParse(ele.GetAttribute("animation_type"), out animation_type);

            InitParameter(
                ParseFromString(ele.GetAttribute("cur_value")),
                ParseFromString(ele.GetAttribute("default_value")),

                ParseFromString(ele.GetAttribute("min_value")),
                ParseFromString(ele.GetAttribute("max_value")),
                bool.Parse(ele.GetAttribute("need_clamp")),

                animation_type,
                float.Parse(ele.GetAttribute("duration")),
                float.Parse(ele.GetAttribute("speed"))
                );
        }

        protected virtual T ParseFromString(string value)
        {
            //Debug.Log("ParseFromString:" + value.ToString() + "," + typeof(T).ToString());
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public void CopyFrom(object obj)
        {
            AdjustableParameter<T> v = (AdjustableParameter<T>)obj;
            InitParameter(v.Value, v.DefaultValue, v.MinValue, v.MaxValue, v.NeedClamp, v.AnimationType, v.Duration, v.speed);
        }

        //public void SetEaseFunc(string func_name)
        //{
        //    func_name = func_name.ToLower();
        //    if()
        //}
    }

    #endregion

    #region Derived Class
    public class IntParameter : AdjustableParameter<int>
    {
        public IntParameter() : base()
        {
            MinValue = 0;
            MaxValue = 1;
        }

        protected override int ClampFunc(int v)
        {
            return v > MaxValue ? MaxValue : (v < MinValue ? MinValue : v);
        }

        protected override int LerpFunc(int start, int end, float t)
        {
            return Mathf.FloorToInt(Mathf.Lerp(start, end, easingFunc(t)));
        }
        protected override float PercentageFunc(int start, int end, int v)
        {
            if (start == end) return 1;

            return (float)(v - start) / (float)(end - start);
        }
    }

    public class FloatParameter : AdjustableParameter<float>
    {
        public FloatParameter() : base()
        {
            MinValue = 0f;
            MaxValue = 1f;
        }
        protected override float ClampFunc(float v)
        {
            return v > MaxValue ? MaxValue : (v < MinValue ? MinValue : v);
        }
        protected override float LerpFunc(float start, float end, float t)
        {
            if (start == end) return start;

            return (end-start) * easingFunc(t) + start;
        }
        protected override float PercentageFunc(float start, float end, float v)
        {
            if (start == end) return 1;

            return (v - start) / (end - start);
        }
    }

    public class Vector3Parameter : AdjustableParameter<Vector3>
    {
        public Vector3Parameter() : base()
        {
            MinValue = Vector3.zero;
            MaxValue = Vector3.one;
        }
        protected override Vector3 ClampFunc(Vector3 v)
        {
            return new Vector3(Mathf.Clamp(v.x, MinValue.x, MaxValue.x), Mathf.Clamp(v.y, MinValue.y, MaxValue.y), Mathf.Clamp(v.z, MinValue.z, MaxValue.z));
        }
        protected override Vector3 LerpFunc(Vector3 start, Vector3 end, float t)
        {
            return Vector3.Lerp(start, end, easingFunc(t));
        }
        protected override float PercentageFunc(Vector3 start, Vector3 end, Vector3 v)
        {
            if (start == end) return 1;

            return (v - start).magnitude / (end - start).magnitude;
        }

        protected override Vector3 ParseFromString(string value)
        {
            // "(0.00, 0.00, 0.00)"

            if (value == null || value.Length < 7)
            {
                Debug.LogError(string.Format("ParseFromString({0}): Wrong format.", value));
                return Vector3.zero;
            }
            
            value = value.Substring(1, value.Length - 2);
            string[] vec = value.Split(',');
            if(vec.Length != 3)
            {
                Debug.LogError(string.Format("ParseFromString({0}): Wrong format.", value));
                return Vector3.zero;
            }

            return new Vector3(float.Parse(vec[0]), float.Parse(vec[1]), float.Parse(vec[2]));
        }
    }

    public class ColorParameter : AdjustableParameter<Color>
    {
        public ColorParameter() : base()
        {
            MinValue = Color.black;
            MaxValue = Color.white;
        }
        protected override Color ClampFunc(Color v)
        {
            return v;
        }
        protected override Color LerpFunc(Color start, Color end, float t)
        {
            return Color.Lerp(start, end, easingFunc(t));
        }
        protected override float PercentageFunc(Color start, Color end, Color v)
        {
            if (start == end) return 1;

            return ((Vector4)(v - start)).magnitude / ((Vector4)(end - start)).magnitude;
        }
        protected override Color ParseFromString(string value)
        {
            // "RGBA(0.000, 0.000, 0.000, 0.000)"

            if (value == null || value.Length < 12)
            {
                Debug.LogError(string.Format("ParseFromString({0}): Wrong format.", value));
                return Color.black;
            }

            int start = value.IndexOf('(') + 1;
            int end = value.IndexOf(')');
            value = value.Substring(start, end - start);
            string[] vec = value.Split(',');
            if (vec.Length != 4)
            {
                Debug.LogError(string.Format("ParseFromString({0}): Wrong format.", value));
                return Color.black;
            }

            return new Color(float.Parse(vec[0]), float.Parse(vec[1]), float.Parse(vec[2]), float.Parse(vec[3]));
        }
    }

    public class MatrixParameter : AdjustableParameter<Matrix4x4>
    {
        public MatrixParameter() : base()
        {
            MinValue = Matrix4x4.zero;
            MaxValue = Matrix4x4.identity;
            NeedClamp = false;
        }
        protected override Matrix4x4 ClampFunc(Matrix4x4 v)
        {
            Matrix4x4 m = new Matrix4x4();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    m[i, j] = Mathf.Clamp(v[i, j], MinValue[i, j], MaxValue[i, j]);
                }
            }
            return m;
        }
        protected override Matrix4x4 LerpFunc(Matrix4x4 start, Matrix4x4 end, float t)
        {
            Matrix4x4 m = new Matrix4x4();
            t = easingFunc(t);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    m[i, j] = Mathf.Lerp(start[i, j], end[i, j], t);
                }
            }
            return m;
        }

        protected override float PercentageFunc(Matrix4x4 start, Matrix4x4 end, Matrix4x4 v)
        {
            if (start == end) return 1;
            Vector4 v1 = start.GetRow(3);
            return ((v.GetRow(3) - start.GetRow(3))).magnitude / ((end.GetRow(3) - start.GetRow(3))).magnitude;
        }

        protected override Matrix4x4 ParseFromString(string value)
        {
            // "0.00000	0.00000	0.00000	0.00000&#xA;0.00000	0.00000	0.00000	0.00000&#xA;0.00000	0.00000	0.00000	0.00000&#xA;0.00000	0.00000	0.00000	0.00000&#xA;"

            if (value == null || value.Length < 31)
            {
                Debug.LogError(string.Format("ParseFromString({0}): Wrong format.", value));
                return Matrix4x4.identity;
            }

            value = value.Replace('\n', '\t');
            value = value.TrimEnd();
            string[] vec = value.Split('\t');
            if (vec.Length != 16)
            {
                Debug.LogError(string.Format("ParseFromString({0}): Wrong format.", value));
                return Matrix4x4.identity;
            }

            return new Matrix4x4(new Vector4(float.Parse(vec[0]), float.Parse(vec[4]), float.Parse(vec[8]), float.Parse(vec[12])),
                new Vector4(float.Parse(vec[1]), float.Parse(vec[5]), float.Parse(vec[9]), float.Parse(vec[13])),
                new Vector4(float.Parse(vec[2]), float.Parse(vec[6]), float.Parse(vec[10]), float.Parse(vec[14])),
                new Vector4(float.Parse(vec[3]), float.Parse(vec[7]), float.Parse(vec[11]), float.Parse(vec[15])));
        }
    }

    public class BoolParameter : AdjustableParameter<bool>
    {
        public BoolParameter() : base()
        {
            MinValue = false;
            MaxValue = true;

            //Easing = false; //no need to ease for toggle
        }
        protected override bool ClampFunc(bool v)
        {
            return v;
        }
        protected override bool LerpFunc(bool start, bool end, float t)
        {
            return t == 1 ? end : start;
        }
        protected override float PercentageFunc(bool start, bool end, bool v)
        {
            if (start == end) return 1;
            
            return v == start ? 0 : 1;
        }

        public void Toggle()
        {
            Value = !Value;
        }
    }



    #endregion

    #region Peripheral Class
    public class GradeParameter : IntParameter
    {
        //public enum EnumGradeType
        //{
        //    Clamp,
        //    Unlimited,
        //    Warp
        //}
        public GradeParameter() : base()
        {
            MinValue = 0;
            MaxValue = 1;
        }
        //public EnumGradeType GradeType { get; set; }
        public void CountUp()
        {
            Value = Value + 1;
        }
        public void CountDown()
        {
            Value = Value - 1;
        }
    }

    #endregion

}