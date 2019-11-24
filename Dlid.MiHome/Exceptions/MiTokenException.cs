using System;
using System.Collections.Generic;
using System.Text;

namespace Dlid.MiHome.Exceptions
{
    public class MiTokenException : Exception
    {
        public MiTokenException()
        {
        }

        public MiTokenException(string message)
            : base(message)
        {
        }

        public MiTokenException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
