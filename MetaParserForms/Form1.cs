using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetaParserForms
{
   
    public partial class Form1 : Form
    {
        const string SCREEN_CONFIG_FILE = "window.cfg";
        const string CONFIG_FILE = "app.cfg";

        public Form1()
        {
            SetUpConfigNames();
            InitializeComponent();
            ReadConfig();
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

        private void SetupWindowProperties()
        {
            TopMost = true;
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

