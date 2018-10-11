using System;
using System.Collections.Generic;
using System.Text;

namespace EF6AspNetWebApi.Tests
{
    public static class Generator
    {
        private static readonly object locker = new object();
        private static long id = DateTime.Now.Ticks % int.MaxValue;

        public static T Id<T>()
        {
            lock (locker)
            {
                return (T)Convert.ChangeType(++id, typeof(T));
            }
        }
    }
}
