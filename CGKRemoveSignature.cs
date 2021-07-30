using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using System.Security.Cryptography.Pkcs;

namespace CGK.RemoveSignature
{
    public static class RemoveSign
    {
        [FunctionName("RemoveSign")]
        public static async Task<IActionResult> RemoveSignature(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string base64String = data.base64;
            string fileName = data.fileName;
            string fileType = data.fileType;
            string fileExt = data.fileExt;
            string contentType = fileType;
            byte[] fileBytes = Convert.FromBase64String(base64String);
            SignedCms signedCms = new SignedCms();
            try
            {
                signedCms.Decode(fileBytes);
            }
            catch
            {
                try
                {
                    string fromUTF8ByteArray = Encoding.UTF8.GetString(fileBytes, 0, fileBytes.Length);
                    fileBytes = Convert.FromBase64String(fromUTF8ByteArray);
                    signedCms.Decode(fileBytes);
                }
                catch
                {
                    throw;
                }    
            }
            string responseMessage = Convert.ToBase64String(signedCms.ContentInfo.Content);
            return (ActionResult)new OkObjectResult(responseMessage);

            // return fileName != null
            //     ? (ActionResult)new OkObjectResult($"File {fileName} with extension {fileExt} Unsigned from Azure.")
            //     : new BadRequestObjectResult("Error on input parameter (object)");

        }
    }


}
