using MetadataExtractor;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace MetaParserForms
{
   
    public partial class Form1 : Form
    {
        const string SCREEN_CONFIG_FILE = "window.cfg";
        const string CONFIG_FILE = "app.cfg";
        protected static string CurrentFile;
        private string _screenConfigFile;
        private string _appConfigFile;

        public class WindowSettings
        {
            public Rectangle DesktopBounds;
            public bool Maximized;
            public bool Minimized;
        }

        public class AppSettings
        {
            public bool SingleInstance;
            public string BGColor1;
            public string BGColor2;
            public string TextColor;
            public string SelectionColor;
        }

        public Form1()
        {
            _screenConfigFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), SCREEN_CONFIG_FILE);
            _appConfigFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), CONFIG_FILE);
            InitializeComponent();
            ReadConfig();
            LoadWindowPosition();
            SetupDataView();
            SetupWindowProperties();

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                string filename = args[1];
                if (File.Exists(filename) == true)
                {
                    CurrentFile = filename;
                    Parse();
                }
            }
            L_copy.SendToBack();
        }

        private void SetupWindowProperties()
        {
            TopMost = true;
            KeyPreview = true;
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);
            this.FormClosing += new FormClosingEventHandler(OnFormClosing);
        }

        private void SetupDataView()
        {
            dataGridView1.Columns[1].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.ShowCellToolTips = false;
        }

        private void ReadConfig()
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
            BackColor =  dataGridView1.DefaultCellStyle.BackColor;
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

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            CurrentFile = files[0];
            Parse();
        }


        private void Parse()
        {
            if (File.Exists(CurrentFile) == false)
            {
                Console.WriteLine("File does not exist!");
                return;
            }
            dataGridView1.Rows.Clear();
            Image image = new Bitmap(CurrentFile);
            ParseBitMap(image);
        }

        private void ParseBitMap(Image image)
        {
            PropertyItem[] propItems = image.PropertyItems;

            var directories = ImageMetadataReader.ReadMetadata(CurrentFile);

            var tag = directories[1].Tags.FirstOrDefault(x => x.Name == "Textual Data");
            string prompt, negPrompt;
            if (tag.Description.Contains("Negative prompt:"))
            {
                prompt = GetInbetweenString(tag.Description, "parameters:", "Negative prompt:").Trim();
                negPrompt = GetInbetweenString(tag.Description, "Negative prompt:", "Steps:").Trim();
            }
            else
            {
                prompt = GetInbetweenString(tag.Description, "parameters:", "Steps:").Trim();
                negPrompt = " ";
            }
            if (string.IsNullOrWhiteSpace(prompt) == false || string.IsNullOrWhiteSpace(negPrompt) == false)
            {
                dataGridView1.Rows.Add("Prompt", prompt);
                dataGridView1.Rows.Add("Negative prompt", negPrompt);
            }

            string rest = tag.Description.Substring(tag.Description.IndexOf("Steps:"));
            foreach (string unit in rest.Split(","))
            {
                var argName = unit.Split(":")[0].Trim();
                var value = unit.Split(":")[1].Trim();
                if (argName == "Size")
                {
                    dataGridView1.Rows.Add("Width", value.Split("x")[0]);
                    dataGridView1.Rows.Add("Height", value.Split("x")[1]);
                }
                else
                {
                    dataGridView1.Rows.Add(argName, value);
                }
            }
        }
        private string GetInbetweenString(string str, string first, string last)
        {
            int pFrom = str.IndexOf(first) + first.Length;
            int pTo = str.LastIndexOf(last);
            if (pTo - pFrom <= 0) return string.Empty;
            return str.Substring(pFrom, pTo - pFrom);
        }
     

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var cell = dataGridView1.Rows[e.RowIndex].Cells[1];
            string result = string.Empty;
            if (cell !=null)
                result = cell.Value.ToString();

            Clipboard.SetText(result);
            ShowLabel(L_copy);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }

            if (e.Control && e.KeyCode == Keys.V)
            {
                Image image = Clipboard.GetImage();
                if (image != null)
                {
                    ParseBitMap(image);
                    return;
                }
                string fileName = string.Empty;

                if (Clipboard.ContainsFileDropList())
                {
                    var filesArray = Clipboard.GetFileDropList();
                    fileName = filesArray[0];
                }
                else if (Clipboard.ContainsText())
                {
                    fileName = Clipboard.GetText();
                }
                if (string.IsNullOrEmpty(fileName))
                    return;
                if (File.Exists(fileName))
                {
                    if (fileName.EndsWith(".png"))
                    {
                        CurrentFile = fileName;
                        Parse();
                    }
                }
            }
        }
          
        private bool _currentlyFlashing = false;
        private async void ShowLabel(Label label)
        {
            if (_currentlyFlashing) return;
            float duration = 250; // milliseconds
            _currentlyFlashing = true;
            label.BringToFront();
            await Task.Delay((int)duration);
            label.SendToBack();
            _currentlyFlashing = false;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            SaveWindowPosition();
        }

        

        private void LoadWindowPosition()
        {
            if (File.Exists(_screenConfigFile) == false) return;
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
        }
        private void SaveWindowPosition()
        {
            WindowSettings windowSettings = new WindowSettings();
            windowSettings.DesktopBounds = this.DesktopBounds;
            windowSettings.Minimized = WindowState == FormWindowState.Minimized;
            windowSettings.Maximized = WindowState == FormWindowState.Maximized;
            WriteToXmlFile<WindowSettings>(_screenConfigFile, windowSettings);
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

