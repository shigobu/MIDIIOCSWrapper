using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MIDIIOCSWrapper
{
    // MIDI構造体
    [StructLayout(LayoutKind.Sequential)]
    internal struct MIDI
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

    public class MIDIIN : IDisposable
    {
        #region DLLインポート

        /// <summary>
        /// インストールされているMIDI入力デバイスの数を調べる。MIDI入力デバイスが何もインストールされていない場合0を返す。
        /// </summary>
        /// <returns>インストールされているMIDI入力デバイスの数</returns>
        [DllImport("MIDIIO.dll")]
        private static extern int MIDIIn_GetDeviceNum();

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
        private static extern int MIDIIn_GetDeviceName(int Index, StringBuilder DeviceName, int Len);

        /// <summary>
        /// MIDI入力デバイスを開く。
        /// この関数は、安全にMIDI入力デバイスをオープンし、
        /// MIDI入力デバイスに必要なメモリを適切な順序で確保する。
        /// MIDI入力デバイス名としては、MIDIIn_GetDeviceName関数で取得したものを使う。
        /// MIDI入力デバイスが開けなかった場合はNULLを返す。
        /// MIDI入力デバイスが開けない主な理由は、他のアプリケーションが既に指定のMIDI入力デバイスを使っている、
        /// 他のアプリケーションが指定のMIDI入力デバイスを閉じ忘れている、
        /// MIDI入力デバイス名が間違えている(例えば名前の後に見えない'\n'や'\r'が付いている)、
        /// インストールされているMIDI入力デバイス(ドライバ)が古すぎる(例えばWindows3.1用のだったりする)などが考えられる。
        /// どうしても開けない場合は、Windowsを再起動する、ドライバを最新のものにインストールし直すなどが考えられる。
        /// </summary>
        /// <param name="DeviceName">MIDI入力デバイス名</param>
        /// <returns>
        /// 正常終了:MIDI入力デバイスオブジェクトへのポインタ
        /// 異常終了:NULL
        /// </returns>
        [DllImport("MIDIIO.dll")]
        private static extern IntPtr MIDIIn_Open(string DeviceName);

        /// <summary>
        /// 現在使用しているMIDI入力デバイスを閉じ、新しいMIDI入力デバイスを開く。
        /// この関数は、単純にMIDIIn_Close (pMIDIIn)とMIDIIn_Open (pszDeviceName)をするだけのものである。
        /// 新しいMIDI入力デバイス名としては、MIDIIn_GetDeviceName関数で取得したものを使う。
        /// 新しいMIDI入力デバイスが開けなかった場合はNULLを返す。
        /// また、使用中のMIDI入力デバイスを閉じる際にエラーが発生した場合、
        /// 新しいMIDI入力デバイスは開かずにNULLを返す。
        /// </summary>
        /// <param name="pMIDIIn">現在使用しているMIDI入力デバイスオブジェクトへのポインタ</param>
        /// <param name="pszDeviceName">新しいMIDI入力デバイス名</param>
        /// <returns>
        /// 正常終了:新しいMIDI入力デバイスオブジェクトへのポインタ
        /// 異常終了:NULL
        /// </returns>
        [DllImport("MIDIIO.dll")]
        private static extern IntPtr MIDIIn_Reopen(IntPtr pMIDIIn, string pszDeviceName);

        /// <summary>
        /// MIDI入力デバイスを閉じる。
        /// この関数は、MIDI入力デバイスが確保したメモリを適切な順序で解放し、
        /// 安全にMIDI入力デバイスをクローズする。
        /// この関数は通常1を返すが、エラーが発生した場合0を返す。
        /// MIDIIn_Closeに失敗したMIDI入力デバイスは、
        /// もはや使用するべきでない。
        /// pMIDIInにNULLを渡した場合は何も起こらず1を返す。
        /// </summary>
        /// <param name="pMIDIIn">MIDI入力デバイスオブジェクトへのポインタ</param>
        /// <returns>
        /// 正常終了:1
        /// 異常終了:0
        /// </returns>
        [DllImport("MIDIIO.dll")]
        private static extern int MIDIIn_Close(IntPtr pMIDIIn);

        /// <summary>
        /// MIDI入力デバイスをリセットし、MIDI入力デバイスを開いた直後の状態に初期化する。
        /// 具体的には、入力バッファにたまっているMIDIメッセージをすべて削除し、
        /// 読み込み位置と書き込み位置を0に初期化する。
        /// </summary>
        /// <param name="pMIDIIn">MIDI入力デバイスオブジェクトへのポインタ</param>
        /// <returns>
        /// 正常終了:1
        /// 異常終了:0
        /// </returns>
        [DllImport("MIDIIO.dll")]
        private static extern int MIDIIn_Reset(IntPtr pMIDIIn);

        /// <summary>
        /// MIDIメッセージをひとつ取得し、実際に取得したMIDIメッセージのバイト数を返す。
        /// 取得するMIDIメッセージがなかった場合は直ちに0を返す(MIDIメッセージの待機は行わない)。
        /// MIDIメッセージが複数個蓄積されている場合があるので、この関数が0を返すまでループ内で繰り返しこの関数を使う必要がある。
        /// この関数は、通常のMIDIチャンネルメッセージのほか、
        /// システムエクスクルーシヴメッセージ・システムリアルタイムメッセージをも取得しうる。
        /// そのため、lLenは256バイト用意しておくべきである。
        /// もしpMessageにMIDIメッセージが入りきれなかった場合、
        /// 入りきれなかった部分のMIDIメッセージは切り捨てられ、二度と取得できなくなる。
        /// </summary>
        /// <param name="pMIDIIn">MIDI入力デバイスへのポインタ</param>
        /// <param name="pMessage">取得するMIDIメッセージを格納するバッファへのポインタ</param>
        /// <param name="lLen">バッファの長さ[バイト]かつ取得するMIDIメッセージの最大バイト数</param>
        /// <returns>実際に取得したMIDIメッセージのバイト数</returns>
        [DllImport("MIDIIO.dll")]
        private static extern int MIDIIn_GetMIDIMessage(IntPtr pMIDIIn, ref IntPtr pMessage, int lLen);

        /// <summary>
        /// このMIDI入力デバイスの名前を調べる。
        /// pMIDIInは既に開かれているデバイスでなければならない。
        /// なお、MIDI入力デバイスの名前は通常32文字以下(ヌル文字含む)であるので、
        /// バッファとしては32文字のTCHAR型配列を用意しておけば十分である。
        /// この関数は現在使用しているデバイス名をINIファイルなどに保存しておくのに便利である。
        /// </summary>
        /// <param name="pMIDIIn">MIDI入力デバイスへのポインタ</param>
        /// <param name="pszDeviceName">MIDI入力デバイス名を格納するバッファへのポインタ</param>
        /// <param name="lLen">MIDI入力デバイス名を格納するバッファの長さ[文字]</param>
        /// <returns>
        /// 正常終了:このMIDI入力デバイス名の文字数[文字]
        /// 異常終了:0
        /// </returns>
        [DllImport("MIDIIO.dll")]
        private static extern int MIDIIn_GetThisDeviceName(IntPtr pMIDIIn, StringBuilder pszDeviceName, int lLen);

        #endregion DLLインポート

        /// <summary>
        /// 指定の名前のMIDIデバイスを開きオブジェクトを初期化します。
        /// </summary>
        public MIDIIN(string deviceName)
        {
            Open(deviceName);
        }

        /// <summary>
        /// MIDI入力デバイスオブジェクト
        /// </summary>
        private IntPtr MIDIInDevice;

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
            if (index >= GetDeviceNum())
            {
                throw new IndexOutOfRangeException();
            }
            StringBuilder deviceName = new StringBuilder(256);
            int res = MIDIIn_GetDeviceName(index, deviceName, deviceName.Capacity);
            if (res == 0)
            {
                throw new MIDIIOException("MIDI入力デバイスの取得に失敗しました。");
            }
            return deviceName.ToString();
        }

        /// <summary>
        /// MIDI入力デバイスを開きます。
        /// </summary>
        /// <param name="deviceName">MIDI入力デバイス名</param>
        public void Open(string deviceName)
        {
            if (!MIDIInDevice.IsZero())
            {
                throw new MIDIIOException("MIDIデバイスはすでに開かれています。");
            }

            MIDIInDevice = MIDIIn_Open(deviceName);
            if (MIDIInDevice.IsZero())
            {
                throw new MIDIIOException("MIDIデバイスが開けませんでした。\n他のアプリケーションを終了してから再試行してください。");
            }
        }

        /// <summary>
        /// 現在使用しているMIDI入力デバイスを閉じ、新しいMIDI入力デバイスを開きます。
        /// </summary>
        /// <param name="deviceName">MIDI入力デバイス名</param>
        public void Reopen(string deviceName)
        {
            if (MIDIInDevice.IsZero())
            {
                throw new MIDIIOException("MIDIデバイスが開かれていません。");
            }

            MIDIInDevice = MIDIIn_Reopen(MIDIInDevice, deviceName);
            if (MIDIInDevice.IsZero())
            {
                throw new MIDIIOException("MIDIデバイスが閉じられなかったか、開けませんでした。");
            }
        }

        /// <summary>
        /// MIDI入力デバイスを閉じます。
        /// </summary>
        public void Close()
        {
            //開かれていない場合、何もしない。
            if (MIDIInDevice.IsZero())
            {
                return;
            }

            int res = MIDIIn_Close(MIDIInDevice);
            MIDIInDevice = IntPtr.Zero;
            if (res == 0)
            {
                throw new MIDIIOException("MIDIデバイスのクローズに失敗しました。このデバイスはもはや使用するべきでない。");
            }

        }

        /// <summary>
        /// MIDI入力デバイスをリセットし、MIDI入力デバイスを開いた直後の状態に初期化する。
        /// 具体的には、入力バッファにたまっているMIDIメッセージをすべて削除し、
        /// 読み込み位置と書き込み位置を0に初期化する。
        /// </summary>
        public void Reset()
        {
            //開かれていない場合、何もしない。
            if (MIDIInDevice.IsZero())
            {
                return;
            }

            int res = MIDIIn_Reset(MIDIInDevice);
            if (res == 0)
            {
                throw new MIDIIOException("MIDIデバイスのリセットに失敗しました。");
            }
        }

        public byte[] GetMIDIMessage()
        {
            if (MIDIInDevice.IsZero())
            {
                throw new MIDIIOException("MIDIデバイスが開かれていません。");
            }

            //バッファサイズ
            int messageSize = 256;
            IntPtr midiMessagePtr = IntPtr.Zero;
            try
            {
                //メモリ確保が必ず実行され、midiMessagePtr変数へ必ず代入されるための呪文
                System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions();
                try { }
                finally
                {
                    //メモリ確保
                    midiMessagePtr = Marshal.AllocCoTaskMem(messageSize);
                }

                //C言語関数呼び出し
                int messageNum = MIDIIn_GetMIDIMessage(MIDIInDevice, ref midiMessagePtr, messageSize);
                //コピー先配列
                byte[] midiMessage = new byte[messageNum];
                //MIDIメッセージ取得できたら
                if (messageNum > 0)
                {
                    //配列にコピー
                    Marshal.Copy(midiMessagePtr, midiMessage, 0, messageNum);
                }
                return midiMessage;

            }
            finally
            {
                if (midiMessagePtr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(midiMessagePtr);
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。
                //MIDIデバイスを閉じる
                this.Close();

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        ~MIDIIN()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(false);
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}