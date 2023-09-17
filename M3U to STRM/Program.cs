using System.Text.RegularExpressions;

class Program
{
    static async Task Main(string[] args)
    {
        bool deleteFolders = true;

        if (deleteFolders)
        {
            Console.WriteLine("Delete all folders!");
            DeleteAllFolders("Filme");
            DeleteAllFolders("Serien");
            Console.WriteLine("All folders were deleted!");
        }
        string baseUrl;
        try
        {
            baseUrl = File.ReadAllText("url.config").Trim();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error url.config was not found: {ex.Message}");
            throw ex;
        }

        int seriesCount = await DownloadAndParsePlaylist("Serien", baseUrl + "&key=series");
        int moviesCount = await DownloadAndParsePlaylist("Filme", baseUrl + "&key=movie");

        if (seriesCount > 0 || moviesCount > 0)
        {
            Console.WriteLine("M3U files successfully downloaded and processed.");
        }
        else
        {
            Console.WriteLine("No movies or series found.");
        }

        Console.WriteLine($"Found series: {seriesCount}");
        Console.WriteLine($"Found Movies: {moviesCount}");
    }

    static async Task<int> DownloadAndParsePlaylist(string folderName, string m3uUrl)
    {
        int itemCount = 0;

        using (HttpClient client = new HttpClient())
        {
            try
            {
                string playlist = await client.GetStringAsync(m3uUrl);
                Console.WriteLine($"M3U files downloaded successfully: {folderName}");
                string[] lines = playlist.Split('\n');

                string currentGroupName = "";
                string currentTvgName = "";
                string currentSeriesFolder = "";

                foreach (string line in lines)
                {
                    if (line.StartsWith("#EXTINF:"))
                    {
                        Match matchGroup = Regex.Match(line, "group-title=\"(.*?)\"");
                        if (matchGroup.Success)
                        {
                            currentGroupName = matchGroup.Groups[1].Value;

                            if (currentGroupName.Contains("[C1]") && !currentGroupName.Contains("3D") && !currentGroupName.Contains("4K") && !currentGroupName.Contains("EXYU") && !currentGroupName.Contains("IT") && !currentGroupName.Contains("TR") && !currentGroupName.Contains("XXX"))
                            {
                                currentTvgName = GetTvgNameFromLine(line);
                                currentSeriesFolder = RemoveInvalidPathChars(currentTvgName); ;
                                if (IsSeries(currentTvgName))
                                {
                                    currentSeriesFolder = RemoveEpisodeAndSeason(currentSeriesFolder);
                                }
                            }
                            else
                            {
                                currentSeriesFolder = "";
                            }
                        }
                    }
                    else if (line.StartsWith("http://") && !string.IsNullOrEmpty(currentSeriesFolder))
                    {
                        string title = RemoveInvalidPathChars(currentTvgName);
                        string directoryPath = Path.Combine(folderName, currentSeriesFolder);
                        string filePath = Path.Combine(directoryPath, title + ".strm");

                        // Create the folder for the series if it does not exist
                        Directory.CreateDirectory(directoryPath);

                        // Create the .strm file and write the stream URL into it
                        File.WriteAllText(filePath, line);
                        itemCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading the M3U file: {ex.Message}");
            }
        }

        return itemCount;
    }

    static string GetTvgNameFromLine(string line)
    {
        Match matchTvgName = Regex.Match(line, "tvg-name=\"(.*?)\"");
        if (matchTvgName.Success)
        {
            return matchTvgName.Groups[1].Value;
        }
        return "";
    }

    static bool IsSeries(string name)
    {
        // Check if the series name has a typical pattern for series names
        return Regex.IsMatch(name, "S\\d+E\\d+");
    }

    static string RemoveInvalidPathChars(string path)
    {
        // Remove invalid characters from the path, including the question mark (?) and the three dots (...)
        char[] invalidChars = Path.GetInvalidFileNameChars();
        invalidChars = invalidChars.Concat(new[] { ':', '?', '.' }).ToArray();
        return new string(path.Where(c => !invalidChars.Contains(c)).ToArray()).Trim();
    }

    static string RemoveEpisodeAndSeason(string name)
    {
        // Use a regular expression to remove the episode and season number
        return Regex.Replace(name, "S\\d+E\\d+", "").Trim();
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
