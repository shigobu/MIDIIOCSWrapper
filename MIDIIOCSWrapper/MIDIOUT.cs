using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MIDIIOCSWrapper
{
	class MIDIOUT
	{
		#region DLLインポート

		/// <summary>
		/// インストールされているMIDI出力デバイスの数を調べる。
		/// MIDI出力デバイスが何もインストールされていない場合0を返す。
		/// </summary>
		/// <returns>インストールされているMIDI出力デバイスの数</returns>
		[DllImport("MIDIIO.dll")]
		private static extern int MIDIOut_GetDeviceNum();

		/// <summary>
		/// MIDI出力デバイスの名前を調べる。
		/// lIndexには0以上、MIDIOut_GetDeviceNumで得られた値-1以下の値を指定すれば、
		/// 正常にMIDI出力デバイスの名前を取得することができる。
		/// その他のインデックスを指定した場合、この関数は失敗し、0を返す。 
		/// なお、MIDI出力デバイスの名前は通常32文字以下(ヌル文字含む)であるので、
		/// バッファとしては32文字のTCHAR型配列を用意しておけば十分である。
		/// </summary>
		/// <param name="lIndex">MIDI出力デバイスのインデックス(0以上)</param>
		/// <param name="pszDeviceName">MIDI出力デバイス名を格納するバッファへのポインタ</param>
		/// <param name="lLen">MIDI出力デバイス名を格納するバッファの長さ[文字]</param>
		/// <returns>
		/// 正常終了:MIDI出力デバイス名の文字数[文字]
		/// 異常終了:0
		/// </returns>
		[DllImport("MIDIIO.dll")]
		private static extern int MIDIOut_GetDeviceName(int lIndex, StringBuilder pszDeviceName, int lLen);

		/// <summary>
		/// MIDI出力デバイスを開く。
		/// この関数は、安全にMIDI出力デバイスをオープンし、
		/// MIDI出力デバイスに必要なメモリを適切な順序で確保する。
		/// MIDI出力デバイス名としては、MIDIOut_GetDeviceName関数で取得したものを使う。
		/// MIDI出力デバイスが開けなかった場合はNULLを返す。
		/// MIDI出力デバイスが開けない主な理由は、
		/// 他のアプリケーションが既に指定のMIDI出力デバイスを使っている、
		/// 他のアプリケーションが指定のMIDI出力デバイスを閉じ忘れている、
		/// MIDI出力デバイス名が間違えている(例えば名前の後に見えない'\n'や'\r'が付いている)、
		/// インストールされているMIDI出力デバイス(ドライバ)が古すぎる(例えばWindows3.1用のだったりする)などが考えられる。
		/// どうしても開けない場合は、Windowsを再起動する、ドライバを最新のものにインストールし直すなどが考えられる。
		/// </summary>
		/// <param name="pszDeviceName">MIDI出力デバイス名へのポインタ</param>
		/// <returns>
		/// 正常終了:MIDI出力デバイスオブジェクトへのポインタ
		/// 異常終了:NULL
		/// </returns>
		[DllImport("MIDIIO.dll")]
		private static extern IntPtr MIDIOut_Open(string pszDeviceName);

		/// <summary>
		/// 現在使用しているMIDI出力デバイスを閉じ、新しいMIDI出力デバイスを開く。
		/// この関数は、単純にMIDIOut_Close (pMIDIOut)とMIDIOut_Open (pszDeviceName)をするだけのものである。
		/// 新しいMIDI出力デバイス名としては、MIDIOut_GetDeviceName関数で取得したものを使う。
		/// 新しいMIDI出力デバイスが開けなかった場合はNULLを返す。
		/// また、使用中のMIDI出力デバイスを閉じる際にエラーが発生した場合、
		/// 新しいMIDI出力デバイスは開かずにNULLを返す。
		/// </summary>
		/// <param name="pMIDIOut">現在使用しているMIDI出力デバイスオブジェクトへのポインタ</param>
		/// <param name="pszDeviceName">新しいMIDI出力デバイス名へのポインタ</param>
		/// <returns>
		/// 正常終了:新しいMIDI出力デバイスオブジェクトへのポインタ
		/// 異常終了:NULL
		/// </returns>
		[DllImport("MIDIIO.dll")]
		private static extern IntPtr MIDIOut_Reopen(IntPtr pMIDIOut, string pszDeviceName);

		/// <summary>
		/// MIDI出力デバイスを閉じる。この関数は、MIDI出力デバイスが確保したメモリを適切な順序で解放し、
		/// 安全にMIDI出力デバイスをクローズする。この関数は通常1を返すが、
		/// エラーが発生した場合0を返す。M
		/// IDIOut_Closeに失敗したMIDI出力デバイスは、もはや使用するべきでない。
		/// pMIDIOutにNULLを渡した場合は何も起こらず1を返す。
		/// </summary>
		/// <param name="pMIDIOut">MIDI出力デバイスオブジェクトへのポインタ</param>
		/// <returns>
		/// 正常終了:1
		/// 異常終了:0
		/// </returns>
		[DllImport("MIDIIO.dll")]
		private static extern int MIDIOut_Close(IntPtr pMIDIOut);

		/// <summary>
		/// MIDI出力デバイスをリセットし、MIDI出力デバイスを開いた直後の状態に初期化する。
		/// 具体的には、出力バッファにたまっているMIDIメッセージをすべて削除し、
		/// 読み込み位置と書き込み位置を0に初期化する。また、すべてのチャンネルの音を消音する。
		/// </summary>
		/// <param name="pMIDIOut">MIDI出力デバイスオブジェクトへのポインタ</param>
		/// <returns></returns>
		[DllImport("MIDIIO.dll")]
		private static extern int MIDIOut_Reset(IntPtr pMIDIOut);

		/// <summary>
		/// MIDIメッセージをひとつ送信し、実際に送信したMIDIメッセージのバイト数を返す。
		/// この関数は、通常のMIDIチャンネルメッセージのほか、
		/// システムエクスクルーシヴメッセージ・システムリアルタイムメッセージをも送信することができる。
		/// 一度に送信できるMIDIメッセージのバイト数、すなわちlLenの最大値は256である。
		/// </summary>
		/// <param name="pMIDIOut">MIDI出力デバイスへのポインタ</param>
		/// <param name="pMessage">送信するMIDIメッセージを格納するバッファへのポインタ</param>
		/// <param name="lLen">送信するMIDIメッセージのバイト数</param>
		/// <returns>実際に送信したMIDIメッセージのバイト数</returns>
		[DllImport("MIDIIO.dll")]
		private static extern int MIDIOut_PutMIDIMessage(IntPtr pMIDIOut, byte[] pMessage, int lLen);

		/// <summary>
		/// MIDIメッセージを1バイト送信し、1を返す。
		/// なお、この関数は危険なので、ほとんど使ってはならない。
		/// なぜなら、MIDIメッセージを中途半端に区切って送信することによって、
		/// MIDIメッセージとMIDIメッセージの境界がわからなくなってしまうからである。
		/// </summary>
		/// <param name="pMIDIOut">MIDI出力デバイスへのポインタ</param>
		/// <param name="ucByte">送信するデータ</param>
		/// <returns>実際に送信したMIDIメッセージのバイト数</returns>
		[DllImport("MIDIIO.dll")]
		private static extern int MIDIOut_PutByte(IntPtr pMIDIOut, byte ucByte);

		/// <summary>
		/// MIDIメッセージを指定されたバイト数送信し、実際に送信したバイト数を返す。
		/// なお、この関数は危険なので、ほとんど使ってはならない。
		/// なぜなら、MIDIメッセージを中途半端に区切って送信することによって、
		/// MIDIメッセージとMIDIメッセージの境界がわからなくなってしまうからである。
		/// </summary>
		/// <param name="pMIDIOut">MIDI出力デバイスへのポインタ</param>
		/// <param name="pBuf">送信するデータを格納するバッファへのポインタ</param>
		/// <param name="lLen">バッファの長さかつ送信する長さ[バイト]</param>
		/// <returns>実際に送信したMIDIメッセージのバイト数</returns>
		[DllImport("MIDIIO.dll")]
		private static extern int MIDIOut_PutBytes(IntPtr pMIDIOut, byte[] pBuf, int lLen);

		#endregion

		#region プロパティ

		public string DeviceName{ get; }

		#endregion

		/// <summary>
		/// 指定の名前のMIDIデバイスを開きオブジェクトを初期化します。
		/// </summary>
		public MIDIOUT(string deviceName)
        {
            Open(deviceName);
			DeviceName = deviceName;
        }

        /// <summary>
        /// MIDI出力デバイスオブジェクト
        /// </summary>
        IntPtr MIDIOutDevice = IntPtr.Zero;

        /// <summary>
        /// インストールされているMIDI出力デバイスの数を調べる。MIDI出力デバイスが何もインストールされていない場合0を返す。
        /// </summary>
        /// <returns>インストールされているMIDI出力デバイスの数</returns>
        public static int GetDeviceNum()
        {
            return MIDIOut_GetDeviceNum();
        }

        /// <summary>
        /// MIDI出力デバイスの名前を調べる。
        /// indexには0以上、MIDIIn_GetDeviceNumで得られた値-1以下の値を指定すれば、
        /// 正常にMIDI出力デバイスの名前を取得することができる。
        /// その他のインデックスを指定した場合、この関数は失敗し、例外を投げる。
        /// </summary>
        /// <param name="index"></param>
        /// <returns>デバイス名</returns>
        public static string GetDeviceName(int index)
        {
            if (index >= GetDeviceNum())
            {
                throw new IndexOutOfRangeException();
            }
            StringBuilder deviceName = new StringBuilder(32);
            int res = MIDIOut_GetDeviceName(index, deviceName, deviceName.Capacity);
            if (res == 0)
            {
                throw new MIDIIOException("MIDI出力デバイスの名前取得に失敗しました。");
            }
            return deviceName.ToString();
        }

        /// <summary>
        /// MIDI出力デバイスを開きます。
        /// </summary>
        /// <param name="deviceName">MIDI出力デバイス名</param>
        private void Open(string deviceName)
        {
            if (!MIDIOutDevice.IsZero())
            {
                throw new MIDIIOException("MIDIデバイスはすでに開かれています。");
            }

            MIDIOutDevice = MIDIOut_Open(deviceName);
            if (MIDIOutDevice.IsZero())
            {
                throw new MIDIIOException("MIDIデバイスが開けませんでした。\n他のアプリケーションを終了してから再試行してください。");
            }
        }

        /// <summary>
        /// 現在使用しているMIDI出力デバイスを閉じ、新しいMIDI出力デバイスを開きます。
        /// </summary>
        /// <param name="deviceName">MIDI出力デバイス名</param>
        public void Reopen(string deviceName)
        {
            if (MIDIOutDevice.IsZero())
            {
                throw new MIDIIOException("MIDIデバイスが開かれていません。");
            }

            MIDIOutDevice = MIDIOut_Reopen(MIDIOutDevice, deviceName);
            if (MIDIOutDevice.IsZero())
            {
                throw new MIDIIOException("MIDIデバイスが閉じられなかったか、開けませんでした。");
            }
        }

        /// <summary>
        /// MIDI出力デバイスを閉じます。
        /// </summary>
        private void Close()
        {
            //開かれていない場合、何もしない。
            if (MIDIOutDevice.IsZero())
            {
                return;
            }

            int res = MIDIOut_Close(MIDIOutDevice);
            MIDIOutDevice = IntPtr.Zero;
            if (res == 0)
            {
                throw new MIDIIOException("MIDIデバイスのクローズに失敗しました。このデバイスはもはや使用するべきでない。");
            }
        }

        /// <summary>
        /// MIDI出力デバイスをリセットし、MIDI出力デバイスを開いた直後の状態に初期化する。
        /// 具体的には、出力バッファにたまっているMIDIメッセージをすべて削除し、
        /// 読み込み位置と書き込み位置を0に初期化する。また、すべてのチャンネルの音を消音する。
        /// </summary>
        public void Reset()
        {
            //開かれていない場合、何もしない。
            if (MIDIOutDevice.IsZero())
            {
                return;
            }

            int res = MIDIOut_Reset(MIDIOutDevice);
            if (res == 0)
            {
                throw new MIDIIOException("MIDIデバイスのリセットに失敗しました。");
            }
        }

        /// <summary>
        /// MIDIメッセージをひとつ送信しします。
		/// この関数は、通常のMIDIチャンネルメッセージのほか、
		/// システムエクスクルーシヴメッセージ・システムリアルタイムメッセージをも送信することができる。
		/// 一度に送信できるMIDIメッセージのバイト数、すなわちlLenの最大値は256である。
        /// </summary>
        /// <param name="message">MIDIメッセージ</param>
        public void PutMIDIMessage(byte[] message)
        {
            if (message.Length > 256)
            {
                throw new MIDIIOException("MIDIメッセージの最大バイト数は256バイトです。");
            }
            MIDIOut_PutMIDIMessage(MIDIOutDevice, message, message.Length);
        }
    }
}
