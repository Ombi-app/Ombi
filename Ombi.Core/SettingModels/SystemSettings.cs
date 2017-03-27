﻿#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: SystemSettings.cs
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
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Ombi.Core.Models;
using Ombi.Helpers;

namespace Ombi.Core.SettingModels
{
    public class SystemSettings : Settings
    {
        public Branches Branch { get; set; }
        public string Version { get; set; }
        public StatusModel Status { get; set; }
        [JsonIgnore]
        public List<BranchDropdown> BranchDropdown { get; set; }
    }

    public class BranchDropdown
    {
        public bool Selected { get; set; }
        public string Name { get; set; }
        public Branches Value { get; set; }
    }

    public enum Branches
    {
        [Branch(DisplayName= "Stable", BranchName = "master")]
        Stable,

        [Branch(DisplayName = "Early Access Preview", BranchName = "eap")]
        EarlyAccessPreview,

        [Branch(DisplayName = "Development", BranchName = "dev")]
        Dev,   
    }


}