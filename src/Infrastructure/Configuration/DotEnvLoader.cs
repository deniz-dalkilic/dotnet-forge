namespace Template.Infrastructure.Configuration;

public static class DotEnvLoader
{
    public static void LoadFromContentRoot(string contentRootPath)
    {
        if (string.IsNullOrWhiteSpace(contentRootPath))
        {
            return;
        }

        var candidates = new[]
        {
            Path.Combine(contentRootPath, ".env"),
            Path.Combine(contentRootPath, ".env.local")
        };

        foreach (var path in candidates)
        {
            LoadFile(path);
        }
    }

    private static void LoadFile(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        foreach (var rawLine in File.ReadLines(path))
        {
            var line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                // Keep already configured environment variables as source of truth.
                continue;
            }

            var value = line[(separatorIndex + 1)..].Trim();
            if (value.Length >= 2 && value.StartsWith('"') && value.EndsWith('"'))
            {
                value = value[1..^1];
            }

            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
