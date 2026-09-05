using Newtonsoft.Json;
using System.Collections.Generic;

namespace CoolapkUWP.Models.Upload
{
    public sealed class UploadPicturePrepareResult
    {
        [JsonProperty("fileInfo")]
        public IList<UploadFileInfo> FileInfo { get; set; }

        [JsonProperty(PropertyName = "uploadPrepareInfo", Required = Required.Default)]
        public UploadPrepareInfo UploadPrepareInfo { get; set; }
    }
}
