
namespace ServerHead.Scripts;


public class DirectoryInMemory
{
    private readonly Dictionary<string, ReadOnlyMemory<byte>> _pages;
    private readonly string _directoryPath;
    private bool _ready;
    public bool Ready => _ready;

    public DirectoryInMemory(string sourceDirectoryPath)
    {
        _pages = new Dictionary<string, ReadOnlyMemory<byte>>();
        _directoryPath = sourceDirectoryPath;
        _ready = false;
    }

    public async Task LoadFilesIntoMemory()
    {
        _ready = false;
        if (!Directory.Exists(_directoryPath))
        {
            //can't find the directory 
            Logger.Instance.Log(LogLevel.Critical, $"Can't find {_directoryPath}");
            throw new Exception($"Can't find {_directoryPath}");
        }

        //find all files in the directory Path
        Queue<string> filePaths = new();
        Stack<string> directoriesToSearch = new();
        directoriesToSearch.Push(_directoryPath);
        try
        {
            do
            {
                string directory = directoriesToSearch.Pop();
                //Get directories
                string[] nextDirectories = Directory.GetDirectories(directory);
                foreach (string dir in nextDirectories)
                {
                    directoriesToSearch.Push(dir);
                }
                //Get file paths
                string[] nextFiles = Directory.GetFiles(directory);
                foreach (string file in nextFiles)
                {
                    filePaths.Enqueue(file);
                }
            } while (directoriesToSearch.Count > 0);
        }
        catch (Exception e)
        {
            //log and exit program
            Logger.Instance.Log(LogLevel.Critical, $"Can't access {_directoryPath} Reason: {e.Message}");
            throw new Exception($"Can't access {_directoryPath}");
        }

        //load all files found in the directory path into memory
        foreach (string filePath in filePaths)
        {
            string fileName = Path.GetFileName(filePath);

            //duplicate file name
            if(_pages.ContainsKey(fileName))
            {
                Logger.Instance.Log(LogLevel.Warning, $"Duplicate file name {filePath}");
                continue;
            }

            _pages[fileName] = await FileReader.ReadFile(filePath);

            //empty file won't add to memory
            if(_pages[fileName].IsEmpty)
            {
                Logger.Instance.Log(LogLevel.Warning, $"Empty file {filePath}");
                _pages.Remove(fileName);
            }
        }
        
        _ready = true; //unlock GetFile
    }

    public ReadOnlyMemory<byte> GetFile(string fileName)
    {
        if(!_ready)
        {
            //should never reach this code unless debugging
            Logger.Instance.Log(LogLevel.Error, "DirectoryInMemory is GetFile is being call before startup is complete");
            return ReadOnlyMemory<byte>.Empty;
        }
        if (_pages.TryGetValue(fileName, out ReadOnlyMemory<byte> fileContent))
        {
            return fileContent;
        }
        else
        {
            //File was not found
            return ReadOnlyMemory<byte>.Empty;
        }
    }
#if DEBUG
    internal async Task RefreshFilesInMemory()
    {
        _ready = false;
        _pages.Clear();
        await LoadFilesIntoMemory();
    }
#endif
}