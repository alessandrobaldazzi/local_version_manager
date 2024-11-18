using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

using System.Text;

static public class FileUtils
{
    //************** FILE FUNCTIONS **************//

    //*** Copy the file in the specified directory and create it if it doesn't exist ***//
    static public void CopyFile(FileInfo f, string path, string partial)
    {
        var newDirectoryPath = path + partial.Substring(0, partial.LastIndexOf(f.Name));
        if (!Directory.Exists(newDirectoryPath))
        {
            Directory.CreateDirectory(newDirectoryPath);
        }
        File.Copy(f.FullName, path + partial, true);
    }

    //*** Checks if the two files are different and act differently depending on it ***//
    //TODO: check if using an hash is better -> collisions are a problem, even if very rare
    static public string FileAct(FileInfo deploy, FileInfo newVersion, string diff, string backup, string partial)
    {
        //Extreme case -> Some psyco put the same path in input and output
        if (string.Equals(deploy.FullName, newVersion.FullName, StringComparison.OrdinalIgnoreCase)) 
            return "";

        // 1) The file has been deleted -> Copy in the backup and return the name
        if (deploy.Exists && !newVersion.Exists)
        {
            CopyFile(deploy, backup, partial);
            return "D " + partial;
        }

        // 2) The file is new -> Copy into teh differences
        if (!deploy.Exists && newVersion.Exists)
        {
            CopyFile(newVersion, diff, partial);
            return "A " + partial;
        }

        // 3) The file has changed -> Copy into the backup and in the differences
        if (deploy.Length != newVersion.Length)
        {
            CopyFile(deploy, backup, partial);
            CopyFile(newVersion, diff, partial);
            return "C " + partial;
        }

        using FileStream depStream = deploy.OpenRead();
        using FileStream newStream = newVersion.OpenRead();

        for (int i = 0; i < depStream.Length; i++)
        {
            //Case 3, but same length, only way is to confront byte by byte
            if (depStream.ReadByte() != newStream.ReadByte())
            {
                CopyFile(deploy, backup, partial);
                CopyFile(newVersion, diff, partial);
                return "C " + partial;
            }
        }
        return "";
    }

    //*** Confronts the two directories and create a difference tree, a backup and returns the list of the changes ***//
    static public StringBuilder SearchAndCopyDir(string deploy, string newVersion, string diff, string backup, Matcher matcher)
    {
        StringBuilder result = new();

        var deployMatch = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(deploy + "\\")));
        var newFilesMatch = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(newVersion + "\\")));

        IEnumerable<string> deployFiles = deployMatch.Files.Select(m => "\\" + m.Path.Replace("/", "\\"));
        IEnumerable<string> newFiles = newFilesMatch.Files.Select(m => "\\" + m.Path.Replace("/", "\\"));

        foreach (string file in deployFiles)
        {
            var depInfo = new FileInfo(deploy + file);
            var newInfo = new FileInfo(newVersion + file);
            var str = FileAct(depInfo, newInfo, diff, backup, file);
            if (newInfo.Exists)
                newFiles = newFiles.Where(f => !f.Equals(file));
            if (!string.IsNullOrEmpty(str))
                result.AppendLine(str);
        }

        foreach (string file in newFiles)
        {
            var depInfo = new FileInfo(deploy + file);
            var newInfo = new FileInfo(newVersion + file);
            var str = FileAct(depInfo, newInfo, diff, backup, file);
            if (!string.IsNullOrEmpty(str))
                result.AppendLine(str);
        }
        return result;
    }

    //*** Checks if the output folders are empty and gives you options otherwise ***//
    static public string CheckStartingDirectory(string type, string path)
    {
        if (Directory.Exists(path) && (Directory.GetFiles(path).Length != 0 || Directory.GetDirectories(path).Length != 0))
        {
            Console.WriteLine(type + " folder already exists and it's not empty, do you want to overwrite it [O], change destination [D] or abort [A]?");
            var currentResponse = Console.ReadLine()?.ToUpper();
            while (currentResponse != "O" && currentResponse != "D" && currentResponse != "A")
            {
                Console.WriteLine("Select a correct response");
                currentResponse = Console.ReadLine()?.ToUpper();
            }
            if (currentResponse.Equals("O"))
            {
                Directory.Delete(path, true);
                Directory.CreateDirectory(path);
                return path;
            }
            else if (currentResponse.Equals("D"))
            {
                Console.WriteLine("Select new destination");
                var newPath = Console.ReadLine()!;
                return CheckStartingDirectory(type, newPath);
            }
            else
            {
                Console.WriteLine("Aborting execution, sayonara");
                Environment.Exit(0);
            }
        }
        Directory.CreateDirectory(path);
        return path;
    }

    //************** UTILITY FUNCTIONS **************//

    //*** Open the log and generate an ordered list ***//
    static public List<string> GetListFromLog(string logPath)
    {
        var completeString = File.ReadAllText(logPath + "\\changes.txt", Encoding.UTF8).Trim();
        if (string.IsNullOrEmpty(completeString))
        {
            return [];
        }
        return completeString.Split('\n').Order().Select(o => o.Trim()).ToList();
    }
}
