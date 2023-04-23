using Aliyun.OSS;
using Aliyun.OSS.Util;
using CoolapkUWP.Models.Upload;
using System.IO;
using System.Text;

namespace CoolapkUWP.Helpers
{
    public static class OSSUploadHelper
    {
        private const string callbackVar = "eyJ4OnZhcjEiOiJmYWxzZSJ9";
        private const string callback = "eyJjYWxsYmFja0JvZHlUeXBlIjoiYXBwbGljYXRpb25cL2pzb24iLCJjYWxsYmFja0hvc3QiOiJhcGkuY29vbGFway5jb20iLCJjYWxsYmFja1VybCI6Imh0dHBzOlwvXC9hcGkuY29vbGFway5jb21cL3Y2XC9jYWxsYmFja1wvbW9iaWxlT3NzVXBsb2FkU3VjY2Vzc0NhbGxiYWNrP2NoZWNrQXJ0aWNsZUNvdmVyUmVzb2x1dGlvbj0wJnZlcnNpb25Db2RlPTIxMDIwMzEiLCJjYWxsYmFja0JvZHkiOiJ7XCJidWNrZXRcIjoke2J1Y2tldH0sXCJvYmplY3RcIjoke29iamVjdH0sXCJoYXNQcm9jZXNzXCI6JHt4OnZhcjF9fSJ9";

        public static string OssUpload(UploadPrepareInfo prepareInfo, UploadFileInfo fileInfo, Stream stream, string contentType)
        {
            OssClient oss = new OssClient(
                prepareInfo.EndPoint.Replace("https://", ""),
                prepareInfo.AccessKeyID,
                prepareInfo.AccessKeySecret,
                prepareInfo.SecurityToken);

            ObjectMetadata metadata = new ObjectMetadata
            {
                ContentMd5 = OssUtils.ComputeContentMd5(stream, stream.Length),
                ContentType = contentType
            };

            metadata.AddHeader(HttpHeaders.Callback, callback);
            metadata.AddHeader(HttpHeaders.CallbackVar, callbackVar);

            PutObjectRequest request = new PutObjectRequest(
                prepareInfo.Bucket,
                fileInfo.UploadFileName,
                stream,
                metadata);

            PutObjectResult putResult = oss.PutObject(request);

            string response = GetCallbackResponse(putResult);
            return response;
        }

        private static string GetCallbackResponse(PutObjectResult putObjectResult)
        {
            string callbackResponse = null;
            using (Stream stream = putObjectResult.ResponseStream)
            {
                byte[] buffer = new byte[4 * 1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                callbackResponse = Encoding.Default.GetString(buffer, 0, bytesRead);
            }
            return callbackResponse;
        }
    }
}
