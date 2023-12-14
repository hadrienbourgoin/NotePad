using NotePad.Controls;
using NotePad.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NotePad
{
    public partial class MainForm : Form
    {
        public RichTextBox CurrentRtb;
        public TextFile CurrentFile;
        public TabControl MainTabControl;
        public Session Session;

        public MainForm()
        {
            InitializeComponent();

            var menuStrip = new MainMenuStrip();
            MainTabControl = new MainTabControl();

            Controls.AddRange(new Control[] { MainTabControl, menuStrip });

            InitializeFile();
        }

        private async void InitializeFile()
        {
            Session = await Session.Load();

            if (Session.TextFiles.Count == 0)
            {
                var file = new TextFile("No title 1");

                MainTabControl.TabPages.Add(file.SafeFileName);

                var tabPage = MainTabControl.TabPages[0];
                var rtb = new CustomRichTextBox();
                tabPage.Controls.Add(rtb);
                rtb.Select();

                Session.TextFiles.Add(file);

                CurrentFile = file;
                CurrentRtb = rtb;
            }
            else
            {
                var activeIndex = Session.ActiveIndex;

                foreach (var file in Session.TextFiles)
                {
                    if (File.Exists(file.FileName) || File.Exists(file.BackupFileName))
                    {
                        var rtb = new CustomRichTextBox();
                        var tabCount = MainTabControl.TabCount;

                        MainTabControl.TabPages.Add(file.SafeFileName);
                        MainTabControl.TabPages[tabCount].Controls.Add(rtb);

                        rtb.Text = file.Contents;

                        Text = $"{file.FileName} - Notepad.NET";
                    }
                }
                try
                {
                    CurrentFile = Session.TextFiles[activeIndex];
                    CurrentRtb = MainTabControl.TabPages[activeIndex].Controls.Find("RtbTextFileContents", true).First() as CustomRichTextBox;
                    CurrentRtb.Select();
                    MainTabControl.SelectedIndex = activeIndex;
                    if (File.Exists(CurrentFile.FileName))
                    {
                        Text = $"{CurrentFile.FileName} - Notepad.NET";
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private void MainForm_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            Session.ActiveIndex = MainTabControl.SelectedIndex;
            Session.Save();

            foreach (var file in Session.TextFiles)
            {
                var fileindex = Session.TextFiles.IndexOf(file);
                try
                {
                    var rtb = MainTabControl.TabPages[fileindex].Controls.Find("RtbTextFileContents", true).First();
                    if (file.FileName.StartsWith("No title"))
                    {
                        file.Contents = rtb.Text;
                        Session.BackupFile(file);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
