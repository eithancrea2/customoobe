using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace CustomOOBE.Services
{
    public class KeyboardBlocker
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private LowLevelKeyboardProc? _proc;
        private IntPtr _hookID = IntPtr.Zero;
        private bool _isBlocking = false;

        public void StartBlocking()
        {
            if (_isBlocking) return;

            _proc = HookCallback;
            _hookID = SetHook(_proc);
            _isBlocking = true;

            Debug.WriteLine("Bloqueo de teclado activado");
        }

        public void StopBlocking()
        {
            if (!_isBlocking) return;

            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }

            _isBlocking = false;
            Debug.WriteLine("Bloqueo de teclado desactivado");
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule?.ModuleName ?? ""), 0);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                var key = KeyInterop.KeyFromVirtualKey(vkCode);

                // Bloquear combinaciones peligrosas
                if (IsKeyBlocked(key))
                {
                    Debug.WriteLine($"Tecla bloqueada: {key}");
                    return (IntPtr)1; // Bloquear la tecla
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private bool IsKeyBlocked(Key key)
        {
            // Obtener el estado de las teclas modificadoras
            bool isCtrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            bool isAltPressed = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
            bool isShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            bool isWinPressed = Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin);

            // Bloquear tecla Windows
            if (key == Key.LWin || key == Key.RWin)
                return true;

            // Bloquear Ctrl+Alt+Del (esto es difícil de bloquear completamente por seguridad de Windows)
            // Pero podemos intentar bloquear algunas combinaciones
            if (isCtrlPressed && isAltPressed && key == Key.Delete)
                return true;

            // Bloquear Alt+F4
            if (isAltPressed && key == Key.F4)
                return true;

            // Bloquear Ctrl+Shift+Esc (Administrador de tareas)
            if (isCtrlPressed && isShiftPressed && key == Key.Escape)
                return true;

            // Bloquear Alt+Tab
            if (isAltPressed && key == Key.Tab)
                return true;

            // Bloquear Ctrl+Tab
            if (isCtrlPressed && key == Key.Tab)
                return true;

            // Bloquear Ctrl+W (cerrar ventana)
            if (isCtrlPressed && key == Key.W)
                return true;

            // Bloquear teclas de función que podrían ser problemáticas
            if (key >= Key.F1 && key <= Key.F12)
            {
                // Permitir F5 para refresh si es necesario
                if (key == Key.F5)
                    return false;

                return true; // Bloquear otras teclas F
            }

            return false;
        }

        ~KeyboardBlocker()
        {
            StopBlocking();
        }
    }

    public class TaskManagerBlocker
    {
        [DllImport("user32.dll")]
        private static extern int FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);

        private const uint WM_CLOSE = 0x0010;
        private System.Threading.Timer? _timer;

        public void StartBlocking()
        {
            _timer = new System.Threading.Timer(CheckAndCloseTaskManager, null, 0, 1000);
            Debug.WriteLine("Bloqueador de Administrador de tareas activado");
        }

        public void StopBlocking()
        {
            _timer?.Dispose();
            _timer = null;
            Debug.WriteLine("Bloqueador de Administrador de tareas desactivado");
        }

        private void CheckAndCloseTaskManager(object? state)
        {
            try
            {
                // Buscar y cerrar el Administrador de tareas
                int hwnd = FindWindow("TaskManagerWindow", string.Empty);
                if (hwnd != 0)
                {
                    SendMessage(hwnd, WM_CLOSE, 0, 0);
                    Debug.WriteLine("Administrador de tareas cerrado");
                }

                // También buscar la versión de Windows 10/11
                hwnd = FindWindow(string.Empty, "Task Manager");
                if (hwnd != 0)
                {
                    SendMessage(hwnd, WM_CLOSE, 0, 0);
                }

                hwnd = FindWindow(string.Empty, "Administrador de tareas");
                if (hwnd != 0)
                {
                    SendMessage(hwnd, WM_CLOSE, 0, 0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al bloquear Task Manager: {ex.Message}");
            }
        }

        ~TaskManagerBlocker()
        {
            StopBlocking();
        }
    }
}
