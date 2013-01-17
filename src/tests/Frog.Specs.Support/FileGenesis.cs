using System.IO;
using Frog.Support;

namespace Frog.Specs.Support
{
    public class FileGenesis
    {
        readonly string root;
        readonly bool should_auto_clean;

        public string Root
        {
            get { return root; }
        }

        public FileGenesis()
        {
            root = OSHelpers.GetMeAWorkingFolder();
            should_auto_clean = true;
        }

        ~FileGenesis()
        {
            if (should_auto_clean)
                OSHelpers.NukeDirectory(root);
        }

        public FileGenesis(string root){
            this.root = root;
        }
		
        public FileGenesis File(string name, string content){
            System.IO.File.WriteAllText(Path.Combine(root, name), content);
            return this;
        }

        public FileGenesis Folder(string folderName)
        {
            var combine = Path.Combine(root, folderName);
            Directory.CreateDirectory(combine);
            return new FileGenesis(combine);
        }

        public FileGenesis Up()
        {
            return new FileGenesis(Directory.GetParent(root).ToString());
        }
    }
}