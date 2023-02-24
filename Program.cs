using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;
using Climan.Model;
using System.Text.RegularExpressions;
using System.Text;

namespace Climan;
internal class Repository
{
    public string? Name { get; set; }
    public List<Command>? Commands { get; set; }
}

internal class Command
{
    public string? Template { get; set; }
    public bool ContainArgument { get; set; }
}

internal class Program
{
    static void Main(string[] args)
    {
        List<string>? repositoryOptions = new List<string>();
        List<string>? commandOptions = new List<string>();
        Dictionary<int, Command[]>? repositoryDictionary = new Dictionary<int, Command[]>();
        List<Repository>? repositoryList = new List<Repository>();
        List<Command>? commandList = new List<Command>();
        IRepositoryMenuPrinter repositoryMenuPrinter = new RepositoryMenuPrinter();
        IMainMenuPrinter mainMenuPrinter = new MainMenuPrinter();

        #region Repository Management
        if (args.Length > 0 && args[0] == "repo")
        {
            try
            {
                // Read from climan.json file if it exists, if not, create a new climan.json file, then create repositoryList from the file
                FileHelper.CreateFileIfNotExist();
                repositoryList = FileHelper.ReadRepositoryFileToList();
                if (repositoryList == null) return;

                int option;
                while (true)
                {
                    option = repositoryMenuPrinter.PrintRepositoryMenu(true);

                    if (option == -1)
                    {
                        Environment.Exit(0);
                    }

                    switch (option)
                    {
                        case 0:
                            string? repositoryName;
                            do
                            {
                                Console.WriteLine();
                                Console.Write("Please enter the CLI name: ");
                                repositoryName = Console.ReadLine()?.Trim();
                            }
                            while (string.IsNullOrEmpty(repositoryName) || string.IsNullOrWhiteSpace(repositoryName));

                            string? template;
                            do
                            {
                                Console.Write($"Please enter the command for ");
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.Write($"{repositoryName} CLI: ");
                                Console.ResetColor();
                                template = Console.ReadLine()?.Trim();
                            }
                            while (string.IsNullOrEmpty(template) || string.IsNullOrWhiteSpace(template));

                            bool isExists = repositoryList.Find(r => r.Name == repositoryName) != null ? true : false;
                            char[] argumentSpecifiers = new char[2] { '<', '>' };
                            bool isContainsArguments = argumentSpecifiers.Any(a => template.Contains(a)) == true ? true : false;

                            if (!isExists)
                            {
                                repositoryList.Add(new Repository
                                {
                                    Name = repositoryName,
                                    Commands = new List<Command>
                                {
                                    new Command
                                    {
                                        ContainArgument = isContainsArguments,
                                        Template = template
                                    }
                                }
                                });
                            }
                            else
                            {
                                Repository? repository = repositoryList.Find(r => r.Name == repositoryName);
                                if (repository != null && repository.Commands != null)
                                {
                                    repository.Commands.Add(new Command
                                    {
                                        ContainArgument = isContainsArguments,
                                        Template = template
                                    });
                                }
                            }

                            FileHelper.WriteRepositoryListToFile(repositoryList);
                            break;
                        case 1:
                            new Process
                            {
                                StartInfo = new ProcessStartInfo(FileHelper.DefaultRepositoryFile)
                                {
                                    UseShellExecute = true
                                }
                            }.Start();
                            Environment.Exit(0);
                            break;
                        case 2:
                            Console.WriteLine();
                            commandList = repositoryList
                                .SelectMany(o => o.GetType().GetProperties()
                                .Where(p => p.PropertyType == typeof(List<Command>))
                                .SelectMany(p =>
                                {
                                    List<Command>? commands = (List<Command>?)p.GetValue(o);

                                    if (commands == null)
                                    {
                                        return Enumerable.Empty<Command>();
                                    }

                                    return commands;
                                }))
                                .ToList();

                            if (commandList.Count <= 0)
                            {
                                Console.BackgroundColor = ConsoleColor.Blue;
                                Console.Write("INF");
                                Console.ResetColor();
                                Console.Write(" ");
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.Write("Run the program again with argument 'repo' to create new repositories.");
                                Console.ResetColor();
                                return;
                            }

                            commandOptions = new List<string>();
                            foreach (var command in commandList)
                            {
                                if (command != null && command.Template != null)
                                {
                                    commandOptions.Add(command.Template);
                                }
                            }

                            option = mainMenuPrinter.PrintMainMenu(true, commandOptions.ToArray());

                            if (option == -1)
                            {
                                Environment.Exit(0);
                            }

                            Repository? repositoryToDelete = repositoryList.Where(r => r.Commands.Contains(commandList.ElementAt(option))).FirstOrDefault();
                            if (repositoryToDelete == null || repositoryToDelete.Commands == null) return;

                            repositoryToDelete.Commands.Remove(commandList.ElementAt(option));
                            foreach (Repository repository in repositoryList)
                            {
                                if (repository.Name == repositoryToDelete.Name)
                                {
                                    repository.Commands = repositoryToDelete.Commands;
                                }
                            }

                            FileHelper.WriteRepositoryListToFile(repositoryList);
                            break;
                        default:
                            Environment.Exit(0);
                            break;
                    }

                    Console.Write("\nPress <Enter> to continue.");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception: {ex.Message}");
            }
        }
        #endregion

        #region Command Executer
        while (true)
        {
            try
            {
                // Read from climan.json file if it exists, if not, print guidance message
                repositoryList = FileHelper.ReadRepositoryFileToList();

                if (repositoryList == null || repositoryList.Count <= 0)
                {
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.Write("INF");
                    Console.ResetColor();
                    Console.Write(" ");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("Run the program again with argument 'repo' to create new repositories.");
                    Console.ResetColor();
                    return;
                }

                for (int i = 0; i < repositoryList.Count; i++)
                {
                    if (repositoryList[i].Name != null)
                    {
                        repositoryOptions.Add(repositoryList[i].Name!);
                        if (repositoryList[i].Commands != null)
                        {
                            repositoryDictionary.Add(i, repositoryList[i].Commands!.ToArray());
                        }
                    }
                }

                int optionNumber;
                string selectedCommand;
                while (true)
                {
                    optionNumber = mainMenuPrinter.PrintMainMenu(true, repositoryOptions.ToArray());

                    if (optionNumber == -1)
                    {
                        Environment.Exit(0);
                    }

                    if (repositoryDictionary[optionNumber] == null || repositoryDictionary[optionNumber].Length <= 0)
                    {
                        Console.WriteLine();
                        Console.BackgroundColor = ConsoleColor.Blue;
                        Console.Write("INF");
                        Console.ResetColor();
                        Console.Write(" ");
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write("Run the program again with argument 'repo' to create new commands for this repository.");
                        Console.ResetColor();
                        return;
                    }
                    else
                    {
                        foreach (Command command in repositoryDictionary[optionNumber])
                        {
                            if (command.Template != null)
                            {
                                commandOptions.Add(command.Template);
                            }
                        }
                    }

                    optionNumber = mainMenuPrinter.PrintMainMenu(true, commandOptions.ToArray());
                    selectedCommand = commandOptions[optionNumber];
                    Console.WriteLine();

                    if (selectedCommand.Contains("<"))
                    {
                        string? argument;
                        MatchCollection paramMatches = Regex.Matches(selectedCommand, @"<[^>]*>");
                        string[]? arguments = new string[paramMatches.Count];
                        for (int i = 0; i < paramMatches.Count; i++)
                        {
                            {
                                do
                                {
                                    Console.WriteLine();
                                    Console.Write($"Please enter ");
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.Write($"{paramMatches[i]}: ");
                                    Console.ResetColor();
                                    argument = Console.ReadLine()?.Trim();
                                } while (string.IsNullOrEmpty(argument) || string.IsNullOrWhiteSpace(argument));
                                arguments[i] = argument;
                            }
                        }
                        for (int j = 0; j < paramMatches.Count; j++)
                        {
                            selectedCommand = selectedCommand.Replace(paramMatches[j].Value, arguments[j]);
                        }
                        Console.WriteLine(ExecuteCommand(selectedCommand));
                    }
                    else
                    {
                        Console.WriteLine(ExecuteCommand(selectedCommand));
                    }

                    commandOptions.Clear();
                    selectedCommand = "";
                    Console.Write("\nPress <Enter> to continue.");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            finally
            {
                Environment.Exit(1);
            }
        }
        #endregion
    }

    static string ExecuteCommand(string arguments)
    {
        var process = new Process();
        string stdError;

        if (!string.IsNullOrEmpty(arguments))
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c {arguments}";
            }
            else
            {
                process.StartInfo.FileName = "/bin/bash";
                process.StartInfo.Arguments = $"-c \"{arguments}\"";
            }
        }

        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardOutput = true;

        var stdOutput = new StringBuilder();
        process.OutputDataReceived += (sender, args) => stdOutput.AppendLine(args.Data);

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            stdError = process.StandardError.ReadToEnd();
            process.WaitForExit();
        }
        catch (Exception ex)
        {
            throw new Exception($"Exception: OS error while executing '{process.StartInfo.FileName} {arguments}': {ex.Message}");
        }

        if (process.ExitCode == 0)
        {
            return stdOutput.ToString();
        }
        else
        {
            var message = new StringBuilder();

            if (!string.IsNullOrEmpty(stdError))
            {
                message.AppendLine(stdError);
            }

            if (stdOutput.Length != 0)
            {
                message.AppendLine("Std output:");
                message.AppendLine(stdOutput.ToString());
            }

            throw new Exception($"Exception: '{process.StartInfo.FileName} {arguments}' finished with exit code = {process.ExitCode}: {message}");
        }
    }
}