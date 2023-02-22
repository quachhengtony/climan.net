using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Climan;
internal static class FileHelper
{
    public const string DefaultRepositoryFile = "climan.json";

    public static void CreateFileIfNotExist()
    {
        if (!File.Exists(DefaultRepositoryFile) || File.ReadAllText(DefaultRepositoryFile) == string.Empty)
        {
            using (FileStream fs = File.Create(DefaultRepositoryFile))
            {
                byte[] info = new UTF8Encoding(true).GetBytes("[]");
                fs.Write(info, 0, info.Length);
            }
        }
    }

    public static void WriteRepositoryListToFile(List<Repository> repositoryList)
    {
        string jsonString = JsonSerializer.Serialize(repositoryList, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        File.WriteAllText(DefaultRepositoryFile, jsonString);
    }

    public static List<Repository>? ReadRepositoryFileToList()
    {
        List<Repository>? repositoryList;
        string? repositoryString = File.ReadAllText(DefaultRepositoryFile);

        if (repositoryString == null)
        {
            return default;
        }

        repositoryList = JsonSerializer.Deserialize<List<Repository>>(repositoryString, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        if (repositoryList == null)
        {
            return default;
        }

        return repositoryList;
    }
}

