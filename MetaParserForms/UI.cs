using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetaParserForms
{
    public partial class Form1
    {
        private bool _currentlyFlashing = false;

        public void ShowInfo(string text)
        {
            ShowLabel(L_copy, text, System.Drawing.Color.SteelBlue, 250);
        }

        public void Error(string text)
        {
            ShowLabel(L_copy, text, System.Drawing.Color.OrangeRed, 500);
        }

        private async void ShowLabel(Label label, string text, Color color, float duration)
        {
            if (_currentlyFlashing) return;
            label.BackColor = color;
            label.Text = text;
            _currentlyFlashing = true;
            label.BringToFront();
            await Task.Delay((int)duration);
            label.SendToBack();
            _currentlyFlashing = false;
        }
    }
}
