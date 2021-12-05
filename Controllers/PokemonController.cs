using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace blob.api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PokemonController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public PokemonController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        public async Task PostAsync()
        {
            var fileName = "pikacu.jpg";
            var filePath = Path.Combine(_env.WebRootPath, fileName);

            var pokeContainerClient = GetContainer("pokemons");
            await pokeContainerClient.CreateIfNotExistsAsync();

            var blockBlobClient = pokeContainerClient.GetBlockBlobClient(fileName);

            var stagedBlocks = new HashSet<string>();
            using (Stream st = System.IO.File.OpenRead(filePath))
            {
                byte[] buffer = new byte[2048];
                while (st.Read(buffer, 0, buffer.Length) > 0)
                {
                    string blockId = Guid.NewGuid().ToString("N");
                    string base64BlockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(blockId));

                    var stagedBlock = await blockBlobClient.StageBlockAsync(base64BlockId, new MemoryStream(buffer, true));

                    stagedBlocks.Add(base64BlockId);
                }
            }

            await blockBlobClient.CommitBlockListAsync(stagedBlocks);
        }

        private BlobContainerClient GetContainer(string containerName)
        {
            var client = new BlobServiceClient("conStr");
            return client.GetBlobContainerClient(containerName);
        }
    }
}