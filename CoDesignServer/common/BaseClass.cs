using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;


namespace LK
{
    namespace LKCommon
    {
        // 单件，线程安全
        public class CSingleton<T> where T : class
        {
            private static T _instance;
            private static readonly object syslock = new object();
            /**
              Returns the instance of this singleton.
           */
            public static T Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        lock (syslock)
                        {
                            if (_instance == null)
                            {
                                ConstructorInfo ci = typeof(T).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
                                if (ci == null) { throw new InvalidOperationException("class must contain a private constructor"); }
                                _instance = (T)ci.Invoke(null);
                            }

                        }
                    }

                    return _instance;
                }
            }
        }
    }
}