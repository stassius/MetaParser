using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tulpep.NotificationWindow;

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
                 prompt = GetInbetweenString(tag.Description.ToLower(), "parameters:", "Steps:").Trim();
                 negPrompt = string.Empty ;
            }
            dataGridView1.Rows.Add("Prompt", prompt);
            dataGridView1.Rows.Add("Neg prompt", negPrompt);

            string rest = tag.Description.Substring(tag.Description.IndexOf("Steps:"));
            foreach(string unit in rest.Split(","))
            {
                dataGridView1.Rows.Add(unit.Split(":")[0].Trim(), unit.Split(":")[1].Trim());
            }
        }

        private string GetInbetweenString(string str, string first, string last)
        {
            int pFrom = str.IndexOf(first) + first.Length;
            int pTo = str.LastIndexOf(last);
            if (pTo - pFrom <= 0) return string.Empty;
            return str.Substring(pFrom, pTo - pFrom);
        }
     

        private void button2_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                CurrentFile = openFileDialog1.FileName;
                Parse();
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string result = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
            Clipboard.SetText(result);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }
        }


          
        private bool CurrentlyFlashing = false;
        private async void FlashControl(DataGridViewRow control)
        {
            
            Color flashColor = Color.Green;

            float duration = 100; // milliseconds
            int steps = 2;
            float interval = duration / steps;

            if (CurrentlyFlashing) return;
            CurrentlyFlashing = true;
            Color original = control.DefaultCellStyle.BackColor;

            float interpolant = 0.0f;
            while (interpolant < 1.0f)
            {
                control.Selected = !control.Selected;
                //Color c = InterpolateColour(flashColor, original, interpolant);
                //control.DefaultCellStyle.BackColor = c;
                await Task.Delay((int)interval);
                interpolant += (1.0f / steps);
            }

           // control.DefaultCellStyle.BackColor = original;

            CurrentlyFlashing = false;
        }

        public static Color InterpolateColour(Color c1, Color c2, float alpha)
        {
            float oneMinusAlpha = 1.0f - alpha;
            float a = oneMinusAlpha * (float)c1.A + alpha * (float)c2.A;
            float r = oneMinusAlpha * (float)c1.R + alpha * (float)c2.R;
            float g = oneMinusAlpha * (float)c1.G + alpha * (float)c2.G;
            float b = oneMinusAlpha * (float)c1.B + alpha * (float)c2.B;
            return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
        }
    }
}

