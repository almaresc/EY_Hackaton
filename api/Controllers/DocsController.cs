using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocsController : ControllerBase
    {
    

        // GET api/<DocsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<DocsController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] string value)
        {
            try
            {
                await Upload(value);
                await OCR(value);

                return Ok("File uploaded successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        #region OCR
        private async Task<string> Upload(string fileName)
        {
            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient("");

            // Create the container and return a container client object
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("");

            // Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(fileName);


            using FileStream fileStream = System.IO.File.OpenRead(fileName);
            await blobClient.UploadAsync(fileStream, true);

            return "SAS URI";
        }

        private async Task<string> OCR(string url)
        {
    
            string endpoint = "YOUR_FORM_RECOGNIZER_ENDPOINT";
            string key = "YOUR_FORM_RECOGNIZER_KEY";
            AzureKeyCredential credential = new AzureKeyCredential(key);
            DocumentIntelligenceClient client = new DocumentIntelligenceClient(new Uri(endpoint), credential);

            // sample document
            Uri fileUri = new Uri("https://raw.githubusercontent.com/Azure-Samples/cognitive-services-REST-api-samples/master/curl/form-recognizer/sample-layout.pdf");

            Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-layout", fileUri);

            AnalyzeResult result = operation.Value;

            return result.Content;
        }

        #endregion
    }
}

