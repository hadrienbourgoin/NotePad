using NotePad.Objects;
using NotePad.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NotePad.Controls
{
    internal class MainMenuStrip : MenuStrip
    {
        private const string MENU_NAME = "MainMenuStrip";
        private FontDialog fontDialog;
        private MainForm currentForm;
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;

        public MainMenuStrip()
        {
            Name = MENU_NAME;
            Dock = DockStyle.Top;

            fontDialog = new FontDialog();
            openFileDialog = new OpenFileDialog();
            saveFileDialog = new SaveFileDialog();

            FileDropDownMenu();
            EditDropDownMenu();
            FormatDropDownMenu();
            ViewDropDownMenu();

            HandleCreated += (s, e) =>
            {
                currentForm = FindForm() as MainForm;
            };
        }

        public void FileDropDownMenu()
        {
            var fileDropDownMenu = new ToolStripMenuItem("File");

            var newFileMenu = new ToolStripMenuItem("New", null, null, Keys.Control | Keys.N);
            var openMenu = new ToolStripMenuItem("Open...", null, null, Keys.Control | Keys.O);
            var saveMenu = new ToolStripMenuItem("Save", null, null, Keys.Control | Keys.S);
            var saveAsMenu = new ToolStripMenuItem("Save as...", null, null, Keys.Control | Keys.Shift | Keys.N);
            var quitMenu = new ToolStripMenuItem("Exit", null, null, Keys.Alt | Keys.F4);

            newFileMenu.Click += (s, e) =>
            {
                var tabControl = currentForm.MainTabControl;
                var tabPagesCount = tabControl.TabPages.Count;

                var fileName = $"No title {tabPagesCount + 1}";
                var file = new TextFile(fileName);
                var rtb = new CustomRichTextBox();

                tabControl.TabPages.Add(file.SafeFileName);

                var newTabPage = tabControl.TabPages[tabPagesCount];

                newTabPage.Controls.Add(rtb);
                currentForm.Session.TextFiles.Add(file);
                tabControl.SelectedTab = newTabPage;

                currentForm.CurrentFile = file;
                currentForm.CurrentRtb = rtb;
            };

            openMenu.Click += async (s, e) =>
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var tabControl = currentForm.MainTabControl;
                    var tabPagesCount = tabControl.TabCount;

                    var file = new TextFile(openFileDialog.FileName);
                    var rtb = new CustomRichTextBox();

                    currentForm.Text = $"{file.FileName} - Notepad.NET";

                    using (StreamReader reader = new StreamReader(file.FileName))
                    {
                        file.Contents = await reader.ReadToEndAsync();
                    }

                    rtb.Text = file.Contents;

                    tabControl.TabPages.Add(file.SafeFileName);
                    tabControl.TabPages[tabPagesCount].Controls.Add(rtb);

                    currentForm.Session.TextFiles.Add(file);
                    currentForm.CurrentRtb = rtb;
                    currentForm.CurrentFile = file;
                    tabControl.SelectedTab = tabControl.TabPages[tabPagesCount];
                }
            };

            saveMenu.Click += async (s, e) =>
            {
                var currentFile = currentForm.CurrentFile;
                var currentRtbText = currentForm.CurrentRtb.Text;

                if (currentFile.Contents != currentRtbText)
                {
                    if (File.Exists(currentFile.FileName))
                    {
                        currentFile.Contents = currentRtbText;
                        try
                        {
                            using (StreamWriter sw = File.CreateText(currentFile.FileName))
                            {
                                await sw.WriteAsync(currentFile.Contents);
                            }
                            currentForm.MainTabControl.SelectedTab.Text = currentFile.SafeFileName;
                            currentForm.Text = $"{currentFile.FileName} - Notepad.NET";
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Could not Save File", "Error", MessageBoxButtons.OK);
                        }
                    }
                    else
                    {
                        MessageBox.Show("File does not exists", "Error", MessageBoxButtons.OK);
                        saveAsMenu.PerformClick();
                    }
                }
            };

            saveAsMenu.Click += async (s, e) =>
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var newFileName = saveFileDialog.FileName;
                    var alreadyExists = false;

                    foreach (var file in currentForm.Session.TextFiles)
                    {
                        if (file.FileName == newFileName && ($"{file.FileName} - Notepad.NET") != currentForm.Text)
                        {
                            MessageBox.Show("This file is open in Notepad.NET.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            alreadyExists = true;
                            break;
                        }
                    }

                    // If file does not already exists in Notepad.NET
                    if (!alreadyExists)
                    {
                        var file = new TextFile(newFileName) { Contents = currentForm.CurrentRtb.Text };

                        var oldFile = currentForm.Session.TextFiles.Where(x => x.FileName == currentForm.CurrentFile.FileName).First();

                        currentForm.Session.TextFiles.Replace(oldFile, file);

                        try
                        {
                            using (StreamWriter sw = File.CreateText(file.FileName))
                            {
                                await sw.WriteAsync(file.Contents);
                            }
                            currentForm.MainTabControl.SelectedTab.Text = file.SafeFileName;
                            currentForm.Text = $"{file.FileName} - Notepad.NET";
                            currentForm.CurrentFile = file;
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Could not Save File", "Error", MessageBoxButtons.OK);
                        }
                    }
                }
            };

            quitMenu.Click += (s, e) => 
            {
                Application.Exit(); 
            };

            fileDropDownMenu.DropDownItems.AddRange(new ToolStripItem[] { newFileMenu, openMenu, saveMenu, saveAsMenu, quitMenu });

            Items.Add(fileDropDownMenu);
        }

        public void EditDropDownMenu()
        {
            var editDropDownMenu = new ToolStripMenuItem("Edit");

            var undoMenu = new ToolStripMenuItem("Undo", null, null, Keys.Control | Keys.Z);
            var redoMenu = new ToolStripMenuItem("Redo", null, null, Keys.Control | Keys.Y);

            undoMenu.Click += (s, e) => { if (currentForm.CurrentRtb.CanUndo) currentForm.CurrentRtb.Undo(); };
            redoMenu.Click += (s, e) => { if (currentForm.CurrentRtb.CanRedo) currentForm.CurrentRtb.Redo(); };

            editDropDownMenu.DropDownItems.AddRange(new ToolStripItem[] { undoMenu, redoMenu });

            Items.Add(editDropDownMenu);
        }

        public void FormatDropDownMenu()
        {
            var formatDropDownMenu = new ToolStripMenuItem("Format");

            var fontMenu = new ToolStripMenuItem("Font...");

            fontMenu.Click += (s, e) =>
            {
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    currentForm.CurrentRtb.Font = fontDialog.Font;
                }
            };

            formatDropDownMenu.DropDownItems.AddRange(new ToolStripItem[] { fontMenu });

            Items.Add(formatDropDownMenu);
        }

        public void ViewDropDownMenu()
        {
            var viewDropDownMenu = new ToolStripMenuItem("View");
            var alwaysOnTopMenu = new ToolStripMenuItem("Always on Top");

            var zoomDropDownMenu = new ToolStripMenuItem("Zoom");
            var zoomInMenu = new ToolStripMenuItem("Zoom in", null, null, Keys.Control | Keys.Add);
            var zoomOutMenu = new ToolStripMenuItem("Zoom out", null, null, Keys.Control | Keys.Subtract);
            var zoomResetMenu = new ToolStripMenuItem("Restore default zoom", null, null, Keys.Control | Keys.Divide);

            zoomInMenu.ShortcutKeyDisplayString = "Ctrl+Num +";
            zoomOutMenu.ShortcutKeyDisplayString = "Ctrl+Num -";
            zoomResetMenu.ShortcutKeyDisplayString = "Ctrl+Num /";

            alwaysOnTopMenu.Click += (s, e) =>
            {
                if (alwaysOnTopMenu.Checked)
                {
                    alwaysOnTopMenu.Checked = false;
                    Program.MainForm.TopMost = false;
                }
                else
                {
                    alwaysOnTopMenu.Checked = true;
                    Program.MainForm.TopMost = true;
                }
            };

            zoomInMenu.Click += (s, e) =>
            {
                if (currentForm.CurrentRtb.ZoomFactor < 3F)
                {
                    currentForm.CurrentRtb.ZoomFactor += 0.3F;
                }
            };

            zoomOutMenu.Click += (s, e) =>
            {
                if (currentForm.CurrentRtb.ZoomFactor > 0.7F)
                {
                    currentForm.CurrentRtb.ZoomFactor -= 0.3F;
                }
            };

            zoomResetMenu.Click += (s, e) => { currentForm.CurrentRtb.ZoomFactor = 1F; };

            zoomDropDownMenu.DropDownItems.AddRange(new ToolStripItem[] { zoomInMenu, zoomOutMenu, zoomResetMenu });

            viewDropDownMenu.DropDownItems.AddRange(new ToolStripItem[] { alwaysOnTopMenu, zoomDropDownMenu });

            Items.Add(viewDropDownMenu);
        }
    }
}
