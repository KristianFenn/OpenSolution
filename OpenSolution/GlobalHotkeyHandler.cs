using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace OpenSolution
{
    public class GlobalHotkeyHandler : IDisposable
    {
        public static class Modifiers
        {
            public const uint MOD_NONE = 0x0000;
            public const uint MOD_ALT = 0x0001;
            public const uint MOD_CTRL = 0x0002;
            public const uint MOD_NOREPEAT = 0x4000;
            public const uint MOD_SHIFT = 0x0004;
            public const uint MOD_WIN = 0x0008;
        }

        public static class VirtualKey
        {
            public const uint O = 0x4F;
        }

        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey(
            [In] IntPtr hWnd,
            [In] int id,
            [In] uint fsModifiers,
            [In] uint vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        private HwndSource _source;
        private int _hotkeyId;
        public event EventHandler OnHotkeyPressed;
        private static Random _random;
        private Window _sourceWindow;

        static GlobalHotkeyHandler()
        {
            _random = new Random();
        }

        public GlobalHotkeyHandler(Window window, uint modifier, uint vk)
        {
            _hotkeyId = _random.Next(0, 100000);
            _sourceWindow = window;
            var helper = new WindowInteropHelper(_sourceWindow);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);
            RegisterHotKey(modifier, vk);
        }

        public void Dispose()
        {
            _source.RemoveHook(HwndHook);
            _source = null;
            UnregisterHotKey();
        }

        private void RegisterHotKey(uint modifier, uint vk)
        {
            var helper = new WindowInteropHelper(_sourceWindow);
            RegisterHotKey(helper.Handle, _hotkeyId, modifier, vk);
        }

        private void UnregisterHotKey()
        {
            var helper = new WindowInteropHelper(_sourceWindow);
            UnregisterHotKey(helper.Handle, _hotkeyId);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    if (wParam.ToInt32() == _hotkeyId)
                    { 
                        OnHotKeyPressed();
                        handled = true;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void OnHotKeyPressed()
        {
            this.OnHotkeyPressed?.Invoke(this, new EventArgs());
        }
    }
}
