using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDIIOCSWrapper
{
    public class MIDIIOException : Exception
    {
        public MIDIIOException() : base()
        {
        }

        public MIDIIOException(string message) : base(message)
        {
        }

        public MIDIIOException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
