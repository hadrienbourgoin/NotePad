using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NotePad.Objects
{
    public class TextFile
    {
        /// <summary>
        /// File path and file name.
        /// </summary>
        [XmlAttribute(AttributeName = "FileName")]
        public string FileName { get; set; }

        /// <summary>
        /// File path and file name before it is saved.
        /// </summary>
        [XmlAttribute(AttributeName = "BackupFileName")]
        public string BackupFileName { get; set; }

        /// <summary>
        /// File name and extension only, no file path.
        /// </summary>
        [XmlIgnore()]
        public string SafeFileName { get; set; }

        /// <summary>
        /// File name and extension only, no file path, before it is saved.
        /// </summary>
        [XmlIgnore()]
        public string SafeBackupFileName { get; set; }

        /// <summary>
        /// File content.
        /// </summary>
        [XmlIgnore()]
        public string Contents { get; set; } = string.Empty;

        /// <summary>
        /// TextFile Class constructor.
        /// </summary>
        public TextFile()
        {

        }

        /// <summary>
        /// TextFile Class constructor.
        /// </summary>
        /// <param name="fileName">File path and file name.</param>
        public TextFile(string fileName)
        {
            FileName = fileName;
            SafeFileName = Path.GetFileName(fileName);

            if (FileName.StartsWith("No title"))
            {
                SafeBackupFileName = $"{FileName}@{DateTime.Now:dd-MM-yyyy-HH-mm-ss}";
                BackupFileName = Path.Combine(Session.BackUpPath, SafeBackupFileName);
            }
        }
    }
}
