using System;
using System.IO;
using System.Text;

namespace Clover.Core.Save
{
    /// <summary>Save files in one folder on the disk (<c>Application.persistentDataPath</c> in the game).</summary>
    public sealed class DiskSaveFiles : ISaveFiles
    {
        private static readonly Encoding Utf8 = new UTF8Encoding(false);
        private readonly string folder;

        public DiskSaveFiles(string folder)
        {
            this.folder = folder ?? "";
        }

        public string Folder => folder;

        public bool Exists(string name) => File.Exists(PathOf(name));

        public string Read(string name) => File.ReadAllText(PathOf(name), Utf8);

        public void Write(string name, string text)
        {
            if (folder.Length > 0) Directory.CreateDirectory(folder);
            using (var stream = new FileStream(PathOf(name), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var bytes = Utf8.GetBytes(text ?? "");
                stream.Write(bytes, 0, bytes.Length);
                // Through the system's cache to the disk, so the swap that follows never puts a half-written file in place.
                stream.Flush(true);
            }
        }

        public void Swap(string from, string into)
        {
            string source = PathOf(from), target = PathOf(into);
            if (!File.Exists(target))
            {
                File.Move(source, target);
                return;
            }
            try
            {
                File.Replace(source, target, null);
            }
            catch (PlatformNotSupportedException)
            {
                // Some platforms have no one-step replace: the old file goes first, so a crash between the two leaves
                // the new one under the temporary name, which SaveStore reads back.
                File.Delete(target);
                File.Move(source, target);
            }
        }

        private string PathOf(string name) => Path.Combine(folder, name);
    }
}
