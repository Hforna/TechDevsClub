using FileTypeChecker;
using FileTypeChecker.Abstracts;
using FileTypeChecker.Extensions;
using FileTypeChecker.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application
{
    public interface IFileService
    {
        public (string ext, bool isValid) IsFileAsPdfOrTxt(Stream file);
    }

    public class FileService : IFileService
    {
        public (string ext, bool isValid) IsFileAsPdfOrTxt(Stream file)
        {
            (string ext, bool isValid) = ("", false);

            IFileType fileType = FileTypeValidator.GetFileType(file);

            if (fileType.Extension == "txt")
                (ext, isValid) = (".txt", true);
            if (fileType is PortableDocumentFormat)
                (ext, isValid) = (".pdf", true);

            file.Position = 0;

            return (ext, isValid);
        }
    }
}
