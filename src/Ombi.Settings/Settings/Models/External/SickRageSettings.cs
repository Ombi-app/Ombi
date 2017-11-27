using System.Collections.Generic;

namespace Ombi.Settings.Settings.Models.External
{
    public class SickRageSettings : ExternalSettings
    {
        public bool Enabled { get; set; }
        public string ApiKey { get; set; }
        public string QualityProfile { get; set; }
        
        public List<DropDownModel> Qualities => new List<DropDownModel>
        {
            new DropDownModel("default", "Use Default"),
            new DropDownModel("sdtv", "SD TV"),
            new DropDownModel("sddvd", "SD DVD"),
            new DropDownModel("hdtv", "HD TV"),
            new DropDownModel("rawhdtv", "Raw HD TV"),
            new DropDownModel("hdwebdl", "HD Web DL"),
            new DropDownModel("fullhdwebdl", "Full HD Web DL"),
            new DropDownModel("hdbluray", "HD Bluray"),
            new DropDownModel("fullhdbluray", "Full HD Bluray"),
        };
    }

    public class DropDownModel
    {
        public DropDownModel(string val, string display)
        {
            Value = val;
            Display = display;
        }

        public DropDownModel()
        {
            
        }
        public string Value { get; set; }
        public string Display { get; set; }
    }
}