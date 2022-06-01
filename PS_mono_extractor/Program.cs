// References https://gist.github.com/SocraticBliss/37f913dd29969c11550d3fc6c46e2951

if (args.Length == 0)
{
    Console.WriteLine("Specify a path...");
    return;
}

// Config
string[] validExts = { ".dll.sprx", ".exe.sprx", ".sdll", ".sexe" };
string startInPath = Path.GetFullPath(args[0]).TrimEnd(Path.PathSeparator);
string extractedsuffix = $"{startInPath}_extracted";
byte[] magic = { 0x4D, 0x5A, 0x90, 0x00 };

// Find those binaries...
foreach (string file in Directory.EnumerateFiles(args[0], "*", SearchOption.AllDirectories))
{
    if (!ValidExt(file)) continue;

    byte[]? bytes = ExtractFile(file, magic);

    if (bytes is not null)
        WriteFile(file, bytes);
}

byte[]? ExtractFile(string path, byte[] pattern)
{
    try
    {
        Console.WriteLine($"Parsing:\n {path}");

        byte[] file = File.ReadAllBytes(path);

        // Search for the magic pattern
        int begin = IndexOfPattern(file, magic);

        if (begin == -1) return null;

        // Get number of sections
        ushort sections = BitConverter.ToUInt16(file, begin + 0x86);

        // Get size
        uint size = BitConverter.ToUInt32(file, begin + 0x188 + ((sections - 1) * 0x28))
            + BitConverter.ToUInt32(file, begin + 0x18C + ((sections - 1) * 0x28));

        return file[begin..(int)(begin + size)];
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error:\n {ex.Message}\n  {path}");
        return null;
    }
}

void WriteFile(string path, byte[] file)
{
    try
    {
        DirectoryInfo? dir = Directory.GetParent(path);
        if (dir is null) return;
        string newDirPath = dir.FullName.Replace(startInPath, extractedsuffix, StringComparison.OrdinalIgnoreCase);
        string newFileName = Path.GetFileNameWithoutExtension(path);
        string newFilePath = Path.Combine(newDirPath, newFileName);

        Directory.CreateDirectory(newDirPath);

        File.WriteAllBytes(newFilePath, file);
        Console.WriteLine($"Extracted:\n {newFilePath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error:\n {ex.Message}\n  {path}");
    }
}

bool ValidExt(string path)
{
    foreach (string ext in validExts)
        if (path.EndsWith(ext))
            return true;
    return false;
}

int IndexOfPattern(byte[] array, byte[] pattern)
{
    int i = 0;
loop:
    if (i + pattern.Length < array.Length)
    {
        for (int j = 0; j < pattern.Length; j++)
            if (array[i + j] != pattern[j])
            {
                i++;
                goto loop;
            };
        return i;
    }
    return -1;
}