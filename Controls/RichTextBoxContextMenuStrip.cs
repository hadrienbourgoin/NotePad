using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NotePad.Controls
{
    internal class RichTextBoxContextMenuStrip : ContextMenuStrip
    {
        private RichTextBox richtextBox;

        public RichTextBoxContextMenuStrip(RichTextBox richTextBox)
        {
            richtextBox = richTextBox;

            var cut = new ToolStripMenuItem("Cut");
            var copy = new ToolStripMenuItem("Copy");
            var paste = new ToolStripMenuItem("Paste");
            var selectAll = new ToolStripMenuItem("Select all");

            cut.Click += (s, e) => richtextBox.Cut();
            copy.Click += (s, e) => richtextBox.Copy();
            paste.Click += (s, e) => richtextBox.Paste();
            selectAll.Click += (s, e) => richtextBox.SelectAll();

            Items.AddRange(new ToolStripItem[] { cut, copy, paste, selectAll });
        }
    }
}
