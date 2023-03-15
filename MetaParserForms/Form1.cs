using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MetaParserForms
{
   
    public partial class Form1 : Form
    {
        const string SCREEN_CONFIG_FILE = "window.cfg";
        const string CONFIG_FILE = "app.cfg";

        public const Int32 WM_SYSCOMMAND = 0x112;
        public const Int32 MF_BYPOSITION = 0x400;
        private const int MF_CHECKED = 0x0008;
        private const int MF_UNCHECKED = 0x0000;
        public const Int32 ALWAYSONTOPMENU = 1000;
        public const Int32 MUMENU2 = 1001;

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        private static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int CheckMenuItem(IntPtr hMenu, int uIDCheckItem, int uCheck);

        public Form1()
        {
            SetUpConfigNames();
            InitializeComponent();
            ReadConfig();
            IntPtr MenuHandle = GetSystemMenu(this.Handle, false);
            InsertMenu(MenuHandle, 0, MF_BYPOSITION | MF_CHECKED, ALWAYSONTOPMENU, "Always On Top");
            LoadWindowPosition();
            SetupDataView();
            SetupWindowProperties();
            L_copy.SendToBack();

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                string filename = args[1];
                if (File.Exists(filename) == true)
                {
                    Parse(filename);
                }
            }
        }

        protected override void WndProc(ref Message msg)
        {
            if (msg.Msg == WM_SYSCOMMAND)
            {
                switch (msg.WParam.ToInt32())
                {
                    case ALWAYSONTOPMENU:
                        TopMost = !TopMost;
                        CheckAlwaysOnTopMenuEntry();
                        return;
                   
                    default:
                        break;
                }
            }
            base.WndProc(ref msg);
        }


        private void SetupWindowProperties()
        {
            KeyPreview = true;
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(OnDragEnter);
            this.DragDrop += new DragEventHandler(OnDragDrop);
            this.KeyDown += new KeyEventHandler(OnKeyDown);
            this.FormClosing += new FormClosingEventHandler(OnFormClosing);
        }

        private void SetupDataView()
        {
            dataGridView1.Columns[1].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.ShowCellToolTips = false;
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            Parse(files[0]);
        }

        private void Parse(string filename)
        {
            if (File.Exists(filename) == false)
            {
                Error("File does not exist!");
                return;
            }
            if (filename.ToLower().EndsWith(".png") == false)
            {
                Error("Not a PNG file!");
                return;
            }
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                dataGridView1.Rows.Clear();
                ParseStream(fs);
            }
            catch
            {
                Error("Error reading file!");
            }
        }

        private void ParseStream(Stream image)
        {
            var result = Parser.ParseBitMap(image);
            foreach (var entry in result)
            {
                dataGridView1.Rows.Add(entry.Key, entry.Value);
            }
        }


        private void OnCellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var cell = dataGridView1.Rows[e.RowIndex].Cells[1];
            string result = string.Empty;
            if (cell !=null)
                result = cell.Value.ToString();

            Clipboard.SetText(result);
            ShowInfo("Copied to clipboard");
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }

            if (e.Control && e.KeyCode == Keys.V)
            {
                ClipBoardParse();
            }
        }
          
        private void ClipBoardParse()
        {
            //Image image = Clipboard.GetImage();
            
            //if (image != null)
            //{
            //    ParseStream(image.ToStream());
            //    return;
            //}
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
            {
                Error("Error reading file!");
                return;
            }
            if (File.Exists(fileName))
            {
                if (fileName.EndsWith(".png"))
                {
                    Parse(fileName);
                }
                else
                {
                    Error("Not a PNG file!");
                }
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            SaveWindowPosition();
        }

    }
}

