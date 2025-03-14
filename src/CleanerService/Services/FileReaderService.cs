namespace CleanerService.Services;

public class FileReaderService
{
    private readonly string _mailDir;

    public FileReaderService(string mailDir)
    {
        _mailDir = mailDir;
    }

    public IEnumerable<(string fileName, string content)> ReadEmails()
    {
        if (!Directory.Exists(_mailDir))
        {
            Console.WriteLine("Mail directory not found.");
            yield break;
        }

        var emailFiles = Directory.GetFiles(_mailDir, "*", SearchOption.AllDirectories);
        var emailList = new List<(string fileName, string content)>();

        foreach (var file in emailFiles)
        {
            try
            {
                string content = File.ReadAllText(file);
                string cleanedContent = CleanEmail(content);
                emailList.Add((file, cleanedContent));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading {file}: {ex.Message}");
            }
        }

        foreach (var email in emailList)
        {
            yield return email;
        }
    }

    private string CleanEmail(string content)
    {
        string[] lines = content.Split('\n');
        bool isBody = false;
        List<string> bodyLines = new List<string>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) isBody = true;
            if (isBody) bodyLines.Add(line);
        }

        return string.Join("\n", bodyLines).Trim();
    }
}