
namespace InventorySystem.ServerScripts;

public class FileReader
{
    public static async Task<ReadOnlyMemory<byte>> ReadFile(string path)
    {
        if (!File.Exists(path))
        {
            Logger.Instance.Log(LogLevel.Error, $"File dosen't exist or can't be reached {path}");
            return ReadOnlyMemory<byte>.Empty;
        }

        try
        {
            byte[] data = await File.ReadAllBytesAsync(path);
            return data;
        }
        catch (Exception e)
        {
            Logger.Instance.Log(LogLevel.Error, $"Can't access {path} Reason: {e.Message}");
            return ReadOnlyMemory<byte>.Empty;
        }

    }
}