using System.Reflection;

namespace Imagegram.WebApi
{
    public class ApplicationConstants
    {
        public struct ResponseStatus
        {
            public const int Pending = 0;
            public const int Success = 1;
            public const int Failed = 2;
        }

        public struct ResponseMessage
        {
            public const string Pending = "Pending";
            public const string Success = "Success";
            public const string Failed = "Failed";
        }

        public struct TableName
        {
            public const string Post = "Post-Test";
        }

        public struct Config
        {
            public const string BucketName = "imagegram-image";
            public const string S3BucketUrl = $"https://{BucketName}.s3.ap-southeast-1.amazonaws.com";
            public const int SizeRestriction = 100000000;
            public const string AllowedFileFormat = "jpg;png;bmp";
        }
    }
}
