using FluentAssertions;
using Xunit;
using Template.Infrastructure.Configuration;

namespace Template.UnitTests.Infrastructure.Configuration;

public sealed class DotEnvLoaderTests
{
    [Fact]
    public void LoadFromContentRoot_LoadsValuesWithoutOverridingExistingVariables()
    {
        var keyFromFile = $"DOTENV_LOADER_TEST_{Guid.NewGuid():N}_FILE";
        var existingKey = $"DOTENV_LOADER_TEST_{Guid.NewGuid():N}_EXISTING";
        var tempDirectory = Directory.CreateTempSubdirectory();

        try
        {
            File.WriteAllText(Path.Combine(tempDirectory.FullName, ".env"), $"{keyFromFile}=from-file\n{existingKey}=from-env\n");
            Environment.SetEnvironmentVariable(existingKey, "already-set");

            DotEnvLoader.LoadFromContentRoot(tempDirectory.FullName);

            Environment.GetEnvironmentVariable(keyFromFile).Should().Be("from-file");
            Environment.GetEnvironmentVariable(existingKey).Should().Be("already-set");
        }
        finally
        {
            Environment.SetEnvironmentVariable(keyFromFile, null);
            Environment.SetEnvironmentVariable(existingKey, null);
            tempDirectory.Delete(recursive: true);
        }
    }
}
