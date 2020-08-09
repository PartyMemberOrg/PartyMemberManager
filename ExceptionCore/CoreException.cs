using System;

namespace ExceptionCore
{
    public class CoreException : Exception
    {
        public CoreException() : base() { }
        public CoreException(string message) : base(message) { }
    }
}
