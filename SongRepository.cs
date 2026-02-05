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
                AudioFilename = "mysong.mp3",
                LrcFilename = "mysong.lrc",
                CoverImage = "music_note.png" // We'll use a default icon for now
            },
            new KaraokeSong
            {
                Title = "Another Hit",
                Artist = "Famous Artist",
                AudioFilename = "mysong.mp3", // Reusing files for testing
                LrcFilename = "mysong.lrc"
            }
        };
    }
}