using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows.Forms;

namespace ActiveKiller
{
	public partial class Form1 : Form
	{
		public static bool serviceEnabled = true;
		public static List<string> ProcessWhiteList = new List<string>();
		public static string WhitelistContainerDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\whitelist.txt";

		public Form1()
		{
			InitializeComponent();
			if (!IsAdministrator())
			{
				MessageBox.Show("Please restart ActiveKiller as administrator to ensure full functionality!",
					"Run program as administrator", MessageBoxButtons.OK, MessageBoxIcon.Warning);

				serviceEnabled = false;
				enableServiceToolStripMenuItem.Text = "Enable Service";
				enableServiceToolStripMenuItem.Enabled = false;
			}

			if (!File.Exists(WhitelistContainerDirectory))
			{
				File.CreateText(WhitelistContainerDirectory);
			}

			string line;

			StreamReader file = new StreamReader(WhitelistContainerDirectory);
			while ((line = file.ReadLine()) != null)
			{
				if (!string.IsNullOrWhiteSpace(line))
				{
					ProcessWhiteList.Add(line.Trim());
				}
			}

			file.Close();

			WindowState = FormWindowState.Minimized;
			Visible = false;
			ShowInTaskbar = false;

			if (serviceEnabled) Hotkeys.RegisterHotKey(this.Handle, 1, Hotkeys.MOD_CONTROL + Hotkeys.MOD_ALT, (int) Keys.X);
			ShowBalloon();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			
		}

		private void enableServiceToolStripMenuItem_Click(object sender, EventArgs e)
		{
			serviceEnabled = !serviceEnabled;

			switch (serviceEnabled)
			{
				case true:
					Hotkeys.RegisterHotKey(this.Handle, 1, Hotkeys.MOD_CONTROL + Hotkeys.MOD_ALT, (int)Keys.X);
					enableServiceToolStripMenuItem.Text = "Disable Service";
					break;
				case false:
					Hotkeys.UnregisterHotKey(this.Handle, 1);
					enableServiceToolStripMenuItem.Text = "Enable Service";
					break;
			}
		}

		public static bool IsAdministrator()
		{
			var identity = WindowsIdentity.GetCurrent();
			var principal = new WindowsPrincipal(identity);
			return principal.IsInRole(WindowsBuiltInRole.Administrator);
		}
		public static void EndProcess(string processName)
		{
			if (processName == "") return;

			try
			{
				Process[] proc = Process.GetProcessesByName(processName.TrimEnd());
				proc[0].Kill();
			}
			catch (Exception)
			{
				Console.WriteLine("Could not kill process '" + processName + "', ignoring ...");
			}
		}

		public static bool isProcessInWhiteList(string processName)
		{
			return ProcessWhiteList.Any(x => x == processName);
		}

		public static void ShowBalloon()
		{
			var notification = new NotifyIcon()
			{
				Visible = true,
				Icon = System.Drawing.SystemIcons.Information,
				BalloonTipTitle = "ActiveKiller is running down here!",
				BalloonTipText = "ActiveKiller is active and running down here\n\nRight click me for more options!",
			};
			notification.ShowBalloonTip(1500);
			System.Threading.Thread.Sleep(2000);
			notification.Dispose();
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == Hotkeys.WM_HOTKEY && (int) m.WParam == 1)
			{
				//Console.WriteLine("Keycombination CTRL+ALT+X has been pressed!");
				string pName = Hotkeys.GetActiveWindowProcessName();

				if (!isProcessInWhiteList(pName)) EndProcess(pName);	
			}
				
			base.WndProc(ref m);
		}

		private void showListToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (ProcessWhiteList.Count < 1)
			{
				MessageBox.Show("No processes are in the whitelist!\n\nYou can add one by right clicking the tray icon!", "Whitelist Empty!", MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			string tmp = "";
			int i = 0;

			foreach (string pName in ProcessWhiteList)
			{
				if (string.IsNullOrWhiteSpace(pName)) continue;
				if (tmp == "")
				{
					tmp += (i + 1).ToString() +  ". " + pName;
				}
				else
				{
					tmp += Environment.NewLine + (i + 1).ToString() + ". " + pName;
				}

				i++;
			}

			MessageBox.Show("Current Whitelist:\n\n" + tmp, "Active Whitelist", MessageBoxButtons.OK,
				MessageBoxIcon.Exclamation);
		}

		private void whitelistNewProcessToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string input = Microsoft.VisualBasic.Interaction.InputBox("Enter a process name to whitelist (without extension):", "Process Whitelist - Add new", "Default", -1, -1);

			if (input == "") return;
			if (isProcessInWhiteList(input)) return;

			using (StreamWriter sw = File.AppendText(WhitelistContainerDirectory))
			{
				sw.WriteLine("\n" + input.Trim());
			}

			ProcessWhiteList.Clear();
			string line;

			StreamReader file = new StreamReader(WhitelistContainerDirectory);
			while ((line = file.ReadLine()) != null)
			{
				ProcessWhiteList.Add(line.Trim());
			}

			file.Close();
			MessageBox.Show("Process name '" + input + "' has been added to whitelist!", "Added new item successfully!",
				MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void exitProgramToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (serviceEnabled)
			{
				Hotkeys.UnregisterHotKey(this.Handle, 1);
			}

			Application.Exit();
		}

		private void clearWhitelistToolStripMenuItem_Click(object sender, EventArgs e)
		{
			File.Create(WhitelistContainerDirectory).Dispose();
			ProcessWhiteList.Clear();

			MessageBox.Show("Whitelist has been cleared out completely!", "Whitelist cleared!", MessageBoxButtons.OK,
				MessageBoxIcon.Information);
		}
	}
}
