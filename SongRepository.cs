using System;
using System.Collections.Generic;
using System.Text;

namespace Caraoce;

public static class SongRepository
{
    public static List<KaraokeSong> GetAllSongs()
    {
        return new List<KaraokeSong>
        {
            // Add all your songs here!
            new KaraokeSong
            {
                Title = "My Test Song",
                Artist = "Me",
                AudioFilename = "testSound3.mp3",
                LrcFilename = "testSound3.lrc",
                CoverImage = "music_note.png" // We'll use a default icon for now
            },
            new KaraokeSong
            {
                Title = "Another Hit",
                Artist = "Famous Artist",
                AudioFilename = "testSound1.mp3", // Reusing files for testing
                LrcFilename = "testSound1.lrc"
            }
        };
    }
}