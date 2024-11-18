using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileSystemGlobbing;

public class Settings
{
    public List<string>? Exclusions { get; set; }
}

class LocalVersionManager
{

    //************** MAIN FUNCTION **************//

    static void Main(string[] args)
    {

        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var settings = new Settings();
        config.Bind(settings);

        //*** Controllo degli input ***///
        string deployPath = "";
        string? newVersionPath = null;
        string? diffPath = null;
        string backupPath = "";
        string logPath = "";

        bool exit = false;
        if (args.Length == 0)
        {
            ConsoleUtils.ShowError();
            return;
        }


        string functionality = args[0].Remove(0, 1).ToLower();

        switch (functionality)
        {
            case "help":
            case "h":
                ConsoleUtils.ShowHelp();
                exit = true;
                break;
            case "diff":
            case "d":
            case "commit":
            case "c":
            case "rollback":
            case "r":
                break;
            default:
                ConsoleUtils.ShowError();
                exit = true;
                break;
        }
        if (exit)
        {
            Console.WriteLine("Terminating...");
            return;
        }

        if (functionality.StartsWith('d'))
        {
            switch (args.Length)
            {
                case 4:
                    deployPath = args[1];
                    newVersionPath = args[2];
                    diffPath = args[3] + "\\diff";
                    backupPath = args[3] + "\\backup";
                    logPath = args[3] + "\\log";
                    break;
                case 6:
                    deployPath = args[1];
                    newVersionPath = args[2];
                    diffPath = args[3];
                    backupPath = args[4];
                    logPath = args[5];
                    break;
                default:
                    ConsoleUtils.ShowError();
                    exit = true;
                    break;
            }
        }
        else if (functionality.StartsWith('c'))
        {
            switch (args.Length)
            {
                case 3:
                    deployPath = args[1];
                    diffPath = args[2] + "\\diff";
                    backupPath = args[2] + "\\backup";
                    logPath = args[2] + "\\log";
                    break;
                case 5:
                    deployPath = args[1];
                    diffPath = args[2];
                    backupPath = args[3];
                    logPath = args[4];
                    break;
                default:
                    ConsoleUtils.ShowError();
                    exit = true;
                    break;
            }
        }
        else
        {
            switch (args.Length)
            {
                case 3:
                    deployPath = args[1];
                    backupPath = args[2] + "\\backup";
                    logPath = args[2] + "\\log";
                    break;
                case 4:
                    deployPath = args[1];
                    backupPath = args[2];
                    logPath = args[3];
                    break;
                default:
                    ConsoleUtils.ShowError();
                    exit = true;
                    break;
            }
        }

        if (exit)
        {
            Console.WriteLine("Terminating...");
            return;
        }

        //*** Funzionalità effettive ***//

        var matcher = new Matcher();
        matcher.AddInclude("**");
        settings.Exclusions ??= [];
        foreach(var s in settings.Exclusions)
        {
            matcher.AddExclude(s);
        }

        if (functionality.StartsWith('d'))
        {
            diffPath = FileUtils.CheckStartingDirectory("Diff", diffPath!);
            backupPath = FileUtils.CheckStartingDirectory("Backup", backupPath);
            logPath = FileUtils.CheckStartingDirectory("Log", logPath);
            OperativeUtils.Diff(deployPath, newVersionPath!, diffPath!, backupPath, logPath, matcher);
        }
        else if (functionality.StartsWith('c'))
        {
            OperativeUtils.Commit(deployPath, diffPath!, backupPath, logPath);
        }
        else
        {
            OperativeUtils.Rollback(deployPath, backupPath, logPath);
        }
    }
}