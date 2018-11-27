using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDIIOCSWrapper
{
    internal static class IntPtrExtension
    {
        #region IntPtr

        /// <summary>
        /// ゼロかどうかを示す値を取得します。
        /// </summary>
        /// <param name="self"><see cref="System.IntPtr"/> のインスタンス。</param>
        /// <returns>ゼロの場合はtrue。それ以外の場合はfalse。</returns>
        public static bool IsZero(this IntPtr self)
        {
            return self == IntPtr.Zero;
        }

        #endregion IntPtr
    }
}
