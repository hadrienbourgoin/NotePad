using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Windows.Forms;

namespace NotePad.Objects
{
    public class Session
    {
        private const string FILENAME = "session.xml";

        private static string _applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static string _applicationPath = Path.Combine(_applicationDataPath, "Notepad.NET");

        public static string BackUpPath = Path.Combine(_applicationDataPath, "Notepad.NET", "Backup");

        private readonly XmlWriterSettings _writterSettings;

        /// <summary>
        /// Path and file name representing the session.
        /// </summary>
        public static string FileName { get; } = Path.Combine(_applicationPath, FILENAME);

        [XmlAttribute(AttributeName = "ActiveIndex")]
        public int ActiveIndex { get; set; } = 0;

        [XmlElement(ElementName = "File")]
        public List<TextFile> TextFiles { get; set; }

        public Session()
        {
            TextFiles = new List<TextFile>();

            _writterSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = ("\t"),
                OmitXmlDeclaration = true
            };

            if (!Directory.Exists(_applicationPath))
            {
                Directory.CreateDirectory(_applicationPath);
            }
        }

        public static async Task<Session> Load()
        {
            var Session = new Session();
            if (File.Exists(FileName))
            {
                var serializer = new XmlSerializer(typeof(Session));
                var streamReader = new StreamReader(FileName);

                try
                {
                    Session = (Session)serializer.Deserialize(streamReader);
                    foreach (var file in Session.TextFiles)
                    {
                        var filename = file.FileName;
                        var backupfilename = file.BackupFileName;

                        file.SafeFileName = Path.GetFileName(filename);
                        // File exists on the disk.
                        if (File.Exists(filename))
                        {
                            using (StreamReader reader = new StreamReader(filename))
                            {
                                file.Contents = await reader.ReadToEndAsync();
                            }
                        }
                        // Backup file from backup folder.
                        if (File.Exists(backupfilename))
                        {
                            using (StreamReader reader = new StreamReader(backupfilename))
                            {
                                file.Contents = await reader.ReadToEndAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                streamReader.Close();
            }
            return Session;
        }

        public void Save()
        {
            var emptyNamespace = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var serializer = new XmlSerializer(typeof(Session));

            using (XmlWriter writer = XmlWriter.Create(FileName, _writterSettings))
            {
                serializer.Serialize(writer, this, emptyNamespace);
            }
        }

        public async void BackupFile(TextFile file)
        {
            if (!Directory.Exists(BackUpPath))
            {
                await Task.Run(() => Directory.CreateDirectory(BackUpPath));
            }
            if (file.FileName.StartsWith("No title"))
            {
                using (StreamWriter writer = File.CreateText(file.BackupFileName))
                {
                    await writer.WriteAsync(file.Contents);
                }
            }
        }
    }
}
