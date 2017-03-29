//#region Copyright
//// /************************************************************************
////    Copyright (c) 2017 Jamie Rees
////    File: DiskTransferService.cs
////    Created By: Jamie Rees
////   
////    Permission is hereby granted, free of charge, to any person obtaining
////    a copy of this software and associated documentation files (the
////    "Software"), to deal in the Software without restriction, including
////    without limitation the rights to use, copy, modify, merge, publish,
////    distribute, sublicense, and/or sell copies of the Software, and to
////    permit persons to whom the Software is furnished to do so, subject to
////    the following conditions:
////   
////    The above copyright notice and this permission notice shall be
////    included in all copies or substantial portions of the Software.
////   
////    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
////    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
////    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
////    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
////    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
////    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
////    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////  ************************************************************************/
//#endregion
//namespace Ombi.Common.Disk
//{
//    public class DiskTransferService
//    {
//        private readonly IDiskProvider _diskProvider;
//        public int MirrorFolder(string sourcePath, string targetPath)
//        {
//            var filesCopied = 0;

//            _logger.Debug("Mirror [{0}] > [{1}]", sourcePath, targetPath);

//            if (!_diskProvider.FolderExists(targetPath))
//            {
//                _diskProvider.CreateFolder(targetPath);
//            }

//            var sourceFolders = _diskProvider.GetDirectoryInfos(sourcePath);
//            var targetFolders = _diskProvider.GetDirectoryInfos(targetPath);

//            foreach (var subDir in targetFolders.Where(v => !sourceFolders.Any(d => d.Name == v.Name)))
//            {
//                if (ShouldIgnore(subDir)) continue;

//                _diskProvider.DeleteFolder(subDir.FullName, true);
//            }

//            foreach (var subDir in sourceFolders)
//            {
//                if (ShouldIgnore(subDir)) continue;

//                filesCopied += MirrorFolder(subDir.FullName, Path.Combine(targetPath, subDir.Name));
//            }

//            var sourceFiles = _diskProvider.GetFileInfos(sourcePath);
//            var targetFiles = _diskProvider.GetFileInfos(targetPath);

//            foreach (var targetFile in targetFiles.Where(v => !sourceFiles.Any(d => d.Name == v.Name)))
//            {
//                if (ShouldIgnore(targetFile)) continue;

//                _diskProvider.DeleteFile(targetFile.FullName);
//            }

//            foreach (var sourceFile in sourceFiles)
//            {
//                if (ShouldIgnore(sourceFile)) continue;

//                var targetFile = Path.Combine(targetPath, sourceFile.Name);

//                if (CompareFiles(sourceFile.FullName, targetFile))
//                {
//                    continue;
//                }

//                TransferFile(sourceFile.FullName, targetFile, TransferMode.Copy, true, true);
//                filesCopied++;
//            }

//            return filesCopied;
//        }
//    }
//}