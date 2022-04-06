namespace AvroCompiler.Core.Storage;

public class TempFiles
{
    public static Lazy<TempFiles> Current { get; } = new Lazy<TempFiles>(() => new TempFiles(), true);
    private HashSet<string> tempFiles;
    private object locker;
    private TempFiles()
    {
        tempFiles = new HashSet<string>();
        locker = new object();

    }
    public string GetTempFileName()
    {
        lock (locker)
        {
            string tempFileName = Guid.NewGuid().ToString();
            File.Create(tempFileName).Close();
            tempFiles.Add(tempFileName);
            return tempFileName;
        }
    }
    public void DeleteAll()
    {
        lock (locker)
        {
            foreach (var file in tempFiles)
            {
                File.Delete(file);
            }
            tempFiles.Clear();
        }
    }
}