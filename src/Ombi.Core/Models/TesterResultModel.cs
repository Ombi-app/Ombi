﻿namespace Ombi.Core.Models
{
    public class TesterResultModel
    {
        public bool IsValid { get; set; }
        public string Version { get; set; }
        public string ExpectedSubDir { get; set; }
        public string AdditionalInformation { get; set; }
    }
}
