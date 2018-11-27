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
            // MIDI入力デバイス(No.0)の名前を調べる:

            int InDev数 = MIDIIN.GetDeviceNum();

            Console.WriteLine("MIDI入力デバイスの選択");
            for (int i = 0; i < InDev数; i++)
            {
                Console.WriteLine(i + " " + MIDIIN.GetDeviceName(i));
            }

            Console.Write(">");
            int InSelect;
            bool 変換成功 = int.TryParse(Console.ReadLine(), out InSelect);

            while (InSelect >= InDev数 || !変換成功)
            {
                Console.WriteLine("正しい数値を入力してください！！");
                Console.Write(">");
                System.Media.SystemSounds.Beep.Play();
                変換成功 = int.TryParse(Console.ReadLine(), out InSelect);
            }

            string InDeviceName = MIDIIN.GetDeviceName(InSelect);

            // MIDI入力デバイスを開く:
            MIDIIN midiIn = new MIDIIN(InDeviceName);
            try
            {
                Console.WriteLine("MIDI入力デバイス「{0:s}」を開きました", InDeviceName);

                Console.Beep();

                System.Threading.Thread.Sleep(1000);

                while (Console.KeyAvailable == false)
                {
                    // MIDI入力デバイスからMIDIメッセージを1つ取得する:
                    byte[] byMessage = midiIn.GetMIDIMessage();
                    // MIDIメッセージを取得した場合:
                    if (byMessage.Length > 0)
                    {
                        for (int i = 0; i < byMessage.Length; ++i)
                        {
                            Console.Write("0x{0:x2} ", byMessage[i]);
                        }
                        Console.Write("\n");
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(1);
                    }
                }

            }
            finally
            {
                midiIn.Close();
            }
        }
    }
}
