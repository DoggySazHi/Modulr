﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Modulr.Models
{
    // We can't hold dictionaries, so we're trying this out instead...
    public class TesterFiles
    {
        public List<string> FileNames { get; set; }
        public List<IFormFile> Files { get; set; }
        // If this is a file for JUnit (aka compile last)
        public List<bool> IsTester { get; set; }

        public bool IsLikelyValid()
        {
            var fileCount = FileNames.Count;
            if (Files.Count != fileCount || IsTester.Count != fileCount)
                return false;
            for (var i = 0; i < fileCount; i++)
                FileNames[i] = Path.GetFileName(FileNames[i]);
            var hasTester = IsTester.IndexOf(true);
            while (hasTester >= 0)
            {
                IsTester.RemoveAt(hasTester);
                var tempFile = Files[hasTester];
                var tempFileName = FileNames[hasTester];
                Files.RemoveAt(hasTester);
                FileNames.RemoveAt(hasTester);
                Files.Add(tempFile);
                FileNames.Add(tempFileName);
                IsTester.Add(false);
                hasTester = IsTester.IndexOf(true);
            }
            return true;
        }
    }
}