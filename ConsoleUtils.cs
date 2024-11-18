static public class ConsoleUtils
{
    //************** CONSOLE OUTPUT FUNCTIONS **************//

    //*** -help message ***//
    static public void ShowHelp()
    {
        Console.WriteLine("Usage: ");
        Console.WriteLine("\t{-d | -diff} <deploy_dir> <newversion_dir> {<output_dir> | <diff_dir> <backup_dir> <log_dir>}");
        Console.WriteLine("\t\tCalculate the differences and create a copy of the new changes (diff), a backup of the files to be changed, and a log file.");
        Console.WriteLine("\t{-c | -commit} <deploy_dir> {<output_dir> | <diff_dir> <backup_dir> <log_dir>}");
        Console.WriteLine("\t\tCommit a previously calculated diff (it needs the diff, backup and log directories/files produced by the commit).");
        Console.WriteLine("\t{-r | -rollback} <deploy_dir> {<output_dir> | <diff_dir> <backup_dir> <log_dir>}");
        Console.WriteLine("\t\tRollback a previous commit (it needs the diff,backup and log directories/files produced by the commit).");
        Console.WriteLine("\t{-h | -help}");
        Console.WriteLine("\t\tShow the usage informations.");
    }

    //*** Error Message ***//
    static public void ShowError()
    {
        Console.WriteLine("Error, command not valid.");
        Console.WriteLine("Use \"-help\" for the usage");
    }
}
