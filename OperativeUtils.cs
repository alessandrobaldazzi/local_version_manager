using Microsoft.Extensions.FileSystemGlobbing;

public static class OperativeUtils
{
    //************** OPERATIVE FUNCTIONS **************//

    //*** Produces the output of the -diff section, prints the log to console and gives the option to continue with the commit or stop ***//
    static public void Diff(string deployPath, string newVersionPath, string diffPath, string backupPath, string logPath, Matcher matcher)
    {
        var changeFiles = FileUtils.SearchAndCopyDir(deployPath, newVersionPath, diffPath, backupPath, matcher).ToString().Trim();
        var changesStream = File.CreateText(logPath + "\\changes.txt");
        changesStream.WriteLine(changeFiles);
        changesStream.Close();
        if (string.IsNullOrEmpty(changeFiles.Trim()) || changeFiles.Trim().Equals("\n"))
        {
            Console.WriteLine("There are no new changes, terminating...");
            return;
        }
        var orderedList = changeFiles.Split('\n').Order().Select(o => o.Trim()).ToList();
        Console.WriteLine("There are " + orderedList.Count + " new changes:");
        foreach (var item in orderedList)
        {
            string currentStr;
            if (item[0] == 'A')
                currentStr = "Added";
            else if (item[0] == 'C')
                currentStr = "Changed";
            else
                currentStr = "Deleted";
            Console.WriteLine(currentStr + item.Remove(0, 1));
        }
        Console.WriteLine("");
        Commit(deployPath, diffPath, backupPath, logPath, orderedList);
    }

    //*** Apply the changes contained in the log and gives the possibility to con un rollback dei cambiamenti ***//
    static public void Commit(string deployPath, string diffPath, string backupPath, string logPath, List<string>? orderedList = null)
    {
        orderedList ??= FileUtils.GetListFromLog(logPath);

        if (orderedList.Count == 0)
        {
            Console.WriteLine("There are no new changes, terminating...");
            return;
        }

        Console.WriteLine('\n' + "Do you want to commit the " + orderedList.Count + " changes? [Y/N]");
        var commitResponse = Console.ReadLine()?.ToUpper().Trim();
        while (commitResponse != "Y" && commitResponse != "N")
        {
            Console.WriteLine("The response must be Y for yes or N for no");
            commitResponse = Console.ReadLine()?.ToUpper().Trim();
        }
        if (commitResponse.Equals("N"))
        {
            Console.WriteLine("Commit not executed, adios");
            return;
        }

        foreach (var item in orderedList)
        {
            string startPath;
            if (item[0] == 'A' || item[0] == 'C')
            {
                startPath = diffPath + item.Remove(0, 2);
                var startInfo = new FileInfo(startPath);
                FileUtils.CopyFile(startInfo, deployPath, item.Remove(0, 2));
            }
            else
            {
                startPath = deployPath + item.Remove(0, 2);
                File.Delete(startPath);
            }
        }
        Console.WriteLine("Operation completed successfully, keep the changes?");
        commitResponse = Console.ReadLine()?.ToUpper().Trim();
        while (commitResponse != "Y" && commitResponse != "N")
        {
            Console.WriteLine("The response must be Y for yes or N for no");
            commitResponse = Console.ReadLine()?.ToUpper().Trim();
        }
        if (commitResponse.Equals("Y"))
        {
            Console.WriteLine("Commit completed, terminating...");
            return;
        }
        Rollback(deployPath, backupPath, logPath, orderedList);
    }

    //*** Annulla i cambiamenti fatti su deploypath descritti dal log ***//
    static public void Rollback(string deployPath, string backupPath, string logPath, List<string>? orderedList = null)
    {
        Console.WriteLine("Restoring previous version...");

        orderedList ??= FileUtils.GetListFromLog(logPath);

        if (orderedList.Count == 0)
        {
            Console.WriteLine("Log file empty, terminating...");
            return;
        }

        foreach (var item in orderedList)
        {
            string startPath;
            if (item[0] == 'D' || item[0] == 'C')
            {
                startPath = backupPath + item.Remove(0, 2);
                var startInfo = new FileInfo(startPath);
                FileUtils.CopyFile(startInfo, deployPath, item.Remove(0, 2));
            }
            else
            {
                startPath = deployPath + item.Remove(0, 2);
                File.Delete(startPath);
            }
        }
        Console.WriteLine("Operation complete, exiting...");
    }

}
