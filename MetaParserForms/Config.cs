using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace MetaParserForms
{
    public partial class Form1 : Form
    {
        private string _screenConfigFile;
        private string _appConfigFile;

        public class WindowSettings
        {
            public Rectangle DesktopBounds;
            public bool Maximized;
            public bool Minimized;
            public bool AlwaysOnTop;
        }

        public class AppSettings
        {
            public bool SingleInstance;
            public string BGColor1;
            public string BGColor2;
            public string TextColor;
            public string SelectionColor;
        }

        public void SetUpConfigNames()
        {
            _screenConfigFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), SCREEN_CONFIG_FILE);
            _appConfigFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), CONFIG_FILE);
        }

        protected void ReadConfig()
        {
            if (File.Exists(_appConfigFile) == false)
            {
                CreateConfig();
            }
            AppSettings appSettings = ReadFromXmlFile<AppSettings>(_appConfigFile);
            if (appSettings.SingleInstance)
            {
                Process currentProcess = Process.GetCurrentProcess();
                Process[] pname = Process.GetProcessesByName(currentProcess.ProcessName);
                if (pname.Length > 1)
                {
                    pname.Where(p => p.Id != Process.GetCurrentProcess().Id)?.First().Kill();
                }
            }
            dataGridView1.DefaultCellStyle.ForeColor = ColorTranslator.FromHtml(appSettings.TextColor);
            dataGridView1.DefaultCellStyle.BackColor = ColorTranslator.FromHtml(appSettings.BGColor1);
            dataGridView1.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml(appSettings.SelectionColor);
            dataGridView1.BackgroundColor = dataGridView1.DefaultCellStyle.BackColor;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = ColorTranslator.FromHtml(appSettings.BGColor2);
            dataGridView1.AlternatingRowsDefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml(appSettings.SelectionColor);
            BackColor = dataGridView1.DefaultCellStyle.BackColor;
            ForeColor = ColorTranslator.FromHtml(appSettings.TextColor);
        }

        private void CreateConfig()
        {
            AppSettings appSettings = new AppSettings();
            appSettings.SingleInstance = true;
            appSettings.TextColor = "#FFFFFF";
            appSettings.BGColor1 = "#101010";
            appSettings.BGColor2 = "#202020";
            appSettings.SelectionColor = "#2176a2";
            WriteToXmlFile<AppSettings>(_appConfigFile, appSettings);
        }


        private void LoadWindowPosition()
        {
            if (File.Exists(_screenConfigFile) == false)
            {
                TopMost = true;
                return;
            }
            WindowSettings windowSettings = ReadFromXmlFile<WindowSettings>(_screenConfigFile);

            this.StartPosition = FormStartPosition.Manual;

            if (windowSettings.Maximized)
            {
                WindowState = FormWindowState.Maximized;
                this.DesktopBounds = windowSettings.DesktopBounds;
            }
            else if (windowSettings.Minimized)
            {
                WindowState = FormWindowState.Minimized;
            }
            else
            {
                this.DesktopBounds = windowSettings.DesktopBounds;
            }
            TopMost = windowSettings.AlwaysOnTop;
            CheckAlwaysOnTopMenuEntry();
        }
        private void SaveWindowPosition()
        {
            WindowSettings windowSettings = new WindowSettings();
            windowSettings.DesktopBounds = this.DesktopBounds;
            windowSettings.Minimized = WindowState == FormWindowState.Minimized;
            windowSettings.Maximized = WindowState == FormWindowState.Maximized;
            windowSettings.AlwaysOnTop = TopMost;
            WriteToXmlFile<WindowSettings>(_screenConfigFile, windowSettings);
        }

        private void CheckAlwaysOnTopMenuEntry()
        {
            IntPtr MenuHandle = GetSystemMenu(this.Handle, false);
            int value;
            value = TopMost ? MF_CHECKED : MF_UNCHECKED;
            CheckMenuItem(MenuHandle, ALWAYSONTOPMENU, value);
        }

        public static void WriteToXmlFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            TextWriter writer = null;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                writer = new StreamWriter(filePath, append);
                serializer.Serialize(writer, objectToWrite);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        public static T ReadFromXmlFile<T>(string filePath) where T : new()
        {
            TextReader reader = null;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                reader = new StreamReader(filePath);
                return (T)serializer.Deserialize(reader);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }
    }
}
