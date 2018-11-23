using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MIDIIOCSWrapper;

namespace テストアプリ
{
    class Program
    {
        static void Main(string[] args)
        {
            int deviceNum = MIDIIN.GetDeviceNum();
            Console.WriteLine(deviceNum);
            for (int i = 0; i < deviceNum; i++)
            {
                Console.WriteLine(MIDIIN.GetDeviceName(i));
            }
            Console.Read();
        }
    }
}
