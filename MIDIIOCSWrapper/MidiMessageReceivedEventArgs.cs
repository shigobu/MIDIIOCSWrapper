using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDIIOCSWrapper
{
    /// <summary>
    /// MIDIメッセージ受信イベントの引数です。
    /// </summary>
    public class MidiMessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// デフォルトコンストラクタ。外部から使用できない。
        /// </summary>
        private MidiMessageReceivedEventArgs()
        {
        }

        /// <summary>
        /// MIDIメッセージを指定して、オブジェクトを初期化します
        /// </summary>
        /// <param name="message">MIDIメッセージ</param>
        public MidiMessageReceivedEventArgs(byte[] message)
        {
            MIDIMessage = message;
        }

        /// <summary>
        /// 受信したMIDIメッセージを表します
        /// </summary>
        public byte[] MIDIMessage { get; private set; }
    }
}
