using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MIDIIOCSWrapper
{
    // MIDI構造体
    [StructLayout(LayoutKind.Sequential)]
    struct MIDI
    {
        public IntPtr m_pDeviceHandle;
        public IntPtr m_pDeviceName;
        public int m_lMode;
        public IntPtr m_pSysxHeader;
        public int m_bStarting;
        public IntPtr m_pBuf;
        public int m_lBufSize;
        public int m_lReadPosition;
        public int m_lWritePosition;
        public int m_bBufLocked;
        public byte m_byRunningStatus;
    }

    public class MIDIIN
    {
        #region DLLインポート
        /// <summary>
        /// インストールされているMIDI入力デバイスの数を調べる。MIDI入力デバイスが何もインストールされていない場合0を返す。
        /// </summary>
        /// <returns>インストールされているMIDI入力デバイスの数</returns>
        [DllImport("MIDIIO.dll")]
        extern static int MIDIIn_GetDeviceNum();

        /// <summary>
        /// MIDI入力デバイスの名前を調べる。
        /// Indexには0以上、MIDIIn_GetDeviceNumで得られた値-1以下の値を指定すれば、
        /// 正常にMIDI入力デバイスの名前を取得することができる。
        /// その他のインデックスを指定した場合、この関数は失敗し、0を返す。 
        /// なお、MIDI入力デバイスの名前は通常32文字以下(ヌル文字含む)であるので、
        /// バッファとしては32文字のTCHAR型配列を用意しておけば十分である。
        /// </summary>
        /// <param name="Index">MIDI入力デバイスのインデックス(0以上)</param>
        /// <param name="DeviceName">MIDI入力デバイス名を格納するバッファへのポインタ</param>
        /// <param name="Len">MIDI入力デバイス名を格納するバッファの長さ[文字]</param>
        /// <returns>
        /// 正常終了:MIDI入力デバイス名の文字数[文字]
        /// 異常終了:0
        /// returns>
        [DllImport("MIDIIO.dll")]
        extern static int MIDIIn_GetDeviceName(int Index, StringBuilder DeviceName, int Len);

        #endregion

        /// <summary>
        /// インストールされているMIDI入力デバイスの数を調べる。MIDI入力デバイスが何もインストールされていない場合0を返す。
        /// </summary>
        /// <returns>インストールされているMIDI入力デバイスの数</returns>
        public static int GetDeviceNum()
        {
            return MIDIIn_GetDeviceNum();
        }

        /// <summary>
        /// MIDI入力デバイスの名前を調べる。
        /// indexには0以上、MIDIIn_GetDeviceNumで得られた値-1以下の値を指定すれば、
        /// 正常にMIDI入力デバイスの名前を取得することができる。
        /// その他のインデックスを指定した場合、この関数は失敗し、例外を投げる。
        /// </summary>
        /// <param name="index"></param>
        /// <returns>デバイス名</returns>
        public static string GetDeviceName(int index)
        {
            StringBuilder deviceName = new StringBuilder(256);
            int res = MIDIIn_GetDeviceName(index, deviceName, deviceName.Capacity);
            if(res == 0)
            {
                throw new Exception("MIDI入力デバイスの取得に失敗しました。");
            }
            return deviceName.ToString();
        }
    }
}
