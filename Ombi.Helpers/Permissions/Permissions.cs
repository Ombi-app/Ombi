#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: Permissions.cs
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
using System.ComponentModel.DataAnnotations;

namespace Ombi.Helpers.Permissions
{
    [Flags]
    ////
    //// NOTE if any are added, make sure we change the UserManagementHelper
    //// 
    public enum Permissions
    {
        [Display(Name = "Access Administration Settings")]
        Administrator = 1,

        [Display(Name = "Request Movie")]
        RequestMovie = 2,

        [Display(Name = "Request TV Show")]
        RequestTvShow = 4,

        [Display(Name = "Request Music")]
        RequestMusic = 8,

        [Display(Name = "Report Issue")]
        ReportIssue = 16,

        [Display(Name = "Read Only User")]
        ReadOnlyUser = 32,

        [Display(Name = "Auto Approve Movie Requests")]
        AutoApproveMovie = 64,

        [Display(Name = "Auto Approve TV Show Requests")]
        AutoApproveTv = 128,

        [Display(Name = "Auto Approve Album Requests")]
        AutoApproveAlbum = 256,

        [Display(Name = "Manage Requests")]
        ManageRequests = 512,

        [Display(Name = "Users can only view their own requests")]
        UsersCanViewOnlyOwnRequests = 1024,

        [Display(Name = "Users can only view their own issues")]
        UsersCanViewOnlyOwnIssues = 2048,

        [Display(Name = "Bypass the request limit")]
        BypassRequestLimit = 4096
    }
}