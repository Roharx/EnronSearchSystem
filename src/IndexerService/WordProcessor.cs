using System.Text.RegularExpressions;

namespace IndexerService
{
    public class WordProcessor
    {
        public Dictionary<string, int> ExtractWords(string content)
        {
            var words = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var matches = Regex.Matches(content.ToLower(), @"\b[a-zA-Z0-9]{2,}\b");

            foreach (Match match in matches)
            {
                if (words.ContainsKey(match.Value))
                    words[match.Value]++;
                else
                    words[match.Value] = 1;
            }

            return words;
        }
    }
}