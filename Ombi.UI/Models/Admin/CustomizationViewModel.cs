#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: CustomizationViewModel.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Ombi.Core.SettingModels;
using Ombi.UI.Models.UI;

namespace Ombi.UI.Models.Admin
{
    public class CustomizationViewModel
    {
        public CustomizationSettings Settings { get; set; }
        public List<Dropdown<Languages>> LanguageDropdown { get; set; } = new List<Dropdown<Languages>>();
        public List<Dropdown<FilterOptions>> FilterOptions { get; set; } =  new List<Dropdown<FilterOptions>>();
        public List<Dropdown<SortOptions>> SortOptions { get; set; } = new List<Dropdown<SortOptions>>();
    }

    public enum FilterOptions
    {
        [Display(Name = ".available-true", Description = "Available")]
        Available,
        [Display(Name = ".available-false", Description = "Not Available")]
        NotAvailable,
        [Display(Name = ".released-true", Description = "Released")]
        Released,
        [Display(Name = ".released-false", Description = "Not Released")]
        NotReleased
    }

    public enum SortOptions
    {
        [Display(Name = "requestorder:desc", Description = "Latest Requests")]
        LatestRequests,
        [Display(Name = "requestorder:asc", Description = "Oldest Requests")]
        OldestRequests,
        [Display(Name = "releaseorder:desc", Description = "Latest Releases")]
        LatestRelease,
        [Display(Name = "releaseorder:asc", Description = "Oldest Releases")]
        OldestRelease
    }

    public enum Languages
    {
        [Display(ResourceType = typeof(Resources.UI), Name = "Layout_English")]
        en,

        [Display(ResourceType = typeof(Resources.UI), Name = "Layout_French")]
        fr,
        [Display(ResourceType = typeof(Resources.UI), Name = "Layout_Dutch")]
        nl,
        [Display(ResourceType = typeof(Resources.UI), Name = "Layout_Spanish")]
        es,
        [Display(ResourceType = typeof(Resources.UI), Name = "Layout_German")]
        de,
        [Display(ResourceType = typeof(Resources.UI), Name = "Layout_Danish")]
        da,
        [Display(ResourceType = typeof(Resources.UI), Name = "Layout_Portuguese")]
        pt,
        [Display(ResourceType = typeof(Resources.UI), Name = "Layout_Swedish")]
        sv,
        [Display(ResourceType = typeof(Resources.UI), Name = "Layout_Italian")]
        it
    }
}