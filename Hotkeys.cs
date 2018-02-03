using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ActiveKiller
{
	public static class Hotkeys
	{
		[DllImport("user32.dll")]
		public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
		[DllImport("user32.dll")]
		public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		[DllImport("user32.dll")]
		public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);
		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		public const int MOD_ALT = 0x0001;
		public const int MOD_CONTROL = 0x0002;
		public const int MOD_SHIFT = 0x0004;
		public const int WM_HOTKEY = 0x0312;


		public static string GetActiveWindowProcessName()
		{
			IntPtr hwnd = GetForegroundWindow();
			uint pid;

			GetWindowThreadProcessId(hwnd, out pid);
			Process p = Process.GetProcessById((int) pid);

			return p.ProcessName;
		}

	}
}
