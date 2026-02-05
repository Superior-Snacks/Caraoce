using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Caraoce;

public static class SongRepository
{
    // We cache the songs here so we don't read the file every time we click a button
    private static List<KaraokeSong> _cachedSongs = new List<KaraokeSong>();

    public static async Task<List<KaraokeSong>> GetAllSongsAsync()
    {
        // If we already loaded the songs, just return them!
        if (_cachedSongs.Count > 0)
            return _cachedSongs;

        try
        {
            // 1. Open the JSON file
            using var stream = await FileSystem.OpenAppPackageFileAsync("songs.json");
            using var reader = new StreamReader(stream);

            // 2. Read the text
            var contents = await reader.ReadToEndAsync();

            // 3. Convert JSON text -> C# List
            _cachedSongs = JsonSerializer.Deserialize<List<KaraokeSong>>(contents);
        }
        catch (Exception ex)
        {
            // If something breaks (like a typo in the JSON), return an empty list or log it
            System.Diagnostics.Debug.WriteLine($"Error loading songs: {ex.Message}");
            _cachedSongs = new List<KaraokeSong>();
        }

        return _cachedSongs;
    }
}