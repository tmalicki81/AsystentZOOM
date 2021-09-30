using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace AsystentZOOM.GUI.Common.Mouse
{
    internal class MouseHelper
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public int X;
            public int Y;
        };

        public static Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }




        private static int _hHook;

        public static MouseWin32Api.HookProc _hProc;

        internal static int SetHook()
        {
            _hProc = new MouseWin32Api.HookProc(MouseHookProc);
            _hHook = MouseWin32Api.SetWindowsHookEx((int)Mouse.MouseEventEnum.WH_MOUSE_LL, _hProc, IntPtr.Zero, 0);
            return _hHook;
        }

        internal static void UnHook()
        {
            MouseWin32Api.UnhookWindowsHookEx(_hHook);
        }

        private static int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var MyMouseHookStruct = (MouseWin32Api.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseWin32Api.MouseHookStruct));
            if (nCode < 0)
                return MouseWin32Api.CallNextHookEx(_hHook, nCode, wParam, lParam);
            else
            {
                MouseEventEnum val = default;
                foreach (MouseEventEnum d in Enum.GetValues(typeof(MouseEventEnum)))
                {
                    if ((int)d == (int)wParam)
                    {
                        val = d;
                        break;
                    }
                }
                CallMouseEvent(new MouseEventArgs(val, MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y));
            }
            return MouseWin32Api.CallNextHookEx(_hHook, nCode, wParam, lParam);
        }

        public static event EventHandler<MouseEventArgs> MouseEvent;

        private static void CallMouseEvent(MouseEventArgs e)
            => MouseEvent?.Invoke(null, e);
    }
}
