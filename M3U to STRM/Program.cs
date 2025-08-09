using System.Text.RegularExpressions;

class Program
{
    static async Task Main(string[] args)
    {
        string baseUrl;
        try
        {
            baseUrl = File.ReadAllText("url.config").Trim();
        }
        catch (Exception ex)
        {
            Log($"Error url.config was not found: {ex.Message}");
            return;
        }

        (int seriesCount, int moviesCount) = await DownloadAndParsePlaylist(baseUrl);

        Log("M3U-Datei wurde erfolgreich verarbeitet.");
        Log($"Gefundene Serien: {seriesCount}");
        Log($"Gefundene Filme: {moviesCount}");
    }

    static async Task<(int seriesCount, int moviesCount)> DownloadAndParsePlaylist(string m3uUrl)
    {
        int seriesCount = 0;
        int moviesCount = 0;

        using (HttpClient client = new HttpClient() { Timeout = Timeout.InfiniteTimeSpan })
        {
            try
            {
                Log("M3U-Datei wird heruntergeladen.");
                string playlist = await client.GetStringAsync(m3uUrl);
                Log("M3U-Datei erfolgreich heruntergeladen.");

                Log("Lösche alle Ordner!");
                DeleteAllFolders("Filme");
                DeleteAllFolders("Serien");
                Log("Alle Ordner wurden gelöscht!");

                string[] lines = playlist.Split('\n');

                string rawTvgName = "";
                string cleanTvgName = "";

                foreach (string line in lines)
                {
                    if (line.StartsWith("#EXTINF:"))
                    {
                        rawTvgName = GetTvgNameFromLine(line, false);
                        cleanTvgName = GetTvgNameFromLine(line, true);
                    }
                    else if ((line.StartsWith("http://") || line.StartsWith("https://")) && !string.IsNullOrEmpty(rawTvgName))
                    {
                        if (!rawTvgName.Contains("┃DE┃"))
                            continue;

                        string safeFolderName = "";
                        string filePath = "";

                        if (line.Contains("/series/"))
                        {
                            string folderName = RemoveEpisodeAndSeason(cleanTvgName);
                            safeFolderName = RemoveInvalidPathChars(folderName);
                            string currentFolder = Path.Combine("Serien", safeFolderName);
                            Directory.CreateDirectory(currentFolder);

                            string fileName = RemoveInvalidPathChars(cleanTvgName) + ".strm";
                            filePath = Path.Combine(currentFolder, fileName);
                            seriesCount++;
                        }
                        else if (line.Contains("/movie/"))
                        {
                            safeFolderName = RemoveInvalidPathChars(cleanTvgName);
                            string currentFolder = Path.Combine("Filme", safeFolderName);
                            Directory.CreateDirectory(currentFolder);
                            filePath = Path.Combine(currentFolder, safeFolderName + ".strm");
                            moviesCount++;
                        }

                        if (!string.IsNullOrEmpty(filePath))
                        {
                            File.WriteAllText(filePath, line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Fehler beim Verarbeiten der M3U-Datei: {ex.Message}");
            }
        }
        return (seriesCount, moviesCount);
    }

    static void Log(string message)
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
    }

    static string GetTvgNameFromLine(string line, bool clean)
    {
        Match matchTvgName = Regex.Match(line, "tvg-name=\"(.*?)\"");
        if (matchTvgName.Success)
        {
            string tvgName = matchTvgName.Groups[1].Value;

            if (clean)
            {
                tvgName = tvgName.Trim();
                tvgName = Regex.Replace(tvgName, @"^\s*┃[A-Z]{2,3}┃\s*", "");
                tvgName = tvgName.Trim();
            }

            return tvgName;
        }
        return "";
    }

    static string RemoveEpisodeAndSeason(string name)
    {
        return Regex.Replace(name, @"\s*S\d+\s*E\d+", "", RegexOptions.IgnoreCase).Trim();
    }

    static string RemoveInvalidPathChars(string path)
    {
        path = Regex.Replace(path, @"┃[A-Z]{2,3}┃\s*", "", RegexOptions.IgnoreCase);

        path = path.Replace("/", "-")
                   .Replace("\\", "-")
                   .Replace(":", "-")
                   .Replace("*", "")
                   .Replace("?", "")
                   .Replace("\"", "")
                   .Replace("<", "")
                   .Replace(">", "")
                   .Replace("|", "");

        char[] invalidChars = Path.GetInvalidFileNameChars();
        path = new string(path.Where(c => !invalidChars.Contains(c)).ToArray());

        path = path.TrimEnd('.');

        return path.Trim();
    }

    static void DeleteAllFolders(string folderName)
    {
        if (Directory.Exists(folderName))
        {
            string[] existingFolders = Directory.GetDirectories(folderName);
            foreach (string folder in existingFolders)
            {
                Directory.Delete(folder, true);
            }
        }
    }
}
