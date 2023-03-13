using MetadataExtractor;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetaParserForms
{
    public partial class Form1 : Form
    {
        protected static string CurrentFile;
        public Form1()
        {
            InitializeComponent();
            TopMost = true;
            KeyPreview = true;
            dataGridView1.Columns[1].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.ShowCellToolTips = false;
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);

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

        void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        void Form1_DragDrop(object sender, DragEventArgs e)
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
            dataGridView1.Rows.Add("Prompt", prompt);
            dataGridView1.Rows.Add("Negative prompt", negPrompt);

            string rest = tag.Description.Substring(tag.Description.IndexOf("Steps:"));
            foreach(string unit in rest.Split(","))
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
        }


          
        private bool CurrentlyFlashing = false;
        private async void ShowLabel(Label label)
        {
            if (CurrentlyFlashing) return;
            float duration = 250; // milliseconds
            CurrentlyFlashing = true;
            label.BringToFront();
            await Task.Delay((int)duration);
            label.SendToBack();
            CurrentlyFlashing = false;
        }

    }
}

