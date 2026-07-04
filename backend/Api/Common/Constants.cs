namespace DocuMind.Api.Common
{
    public static class Constants
    {
        public const string NoFilesProvidedErrorMessage = "No files provided for upload.";
        public const string MaxFileUploadLimitErrorMessage = "You can upload a maximum of 5 files at a time.";
        public const string EmptyFileErrorMessage = "One or more files are empty.";
        public const string UnsupportedFileFormatErrorMessage = "Only PDF files are allowed.";
        public const string FileSizeExceededErrorMessage = "File size exceeds the maximum limit of 10 MB.";


        public const int NoFilesProvidedErrorCode = 10001;
        public const int MaxFileUploadLimitErrorCode = 10002;
        public const int EmptyFileErrorCode = 10003;
        public const int UnsupportedFileFormatErrorCode = 10004;
        public const int FileSizeExceededErrorCode = 10005;
    }
}