using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Cila.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Nethereum.Signer.Crypto;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Web3.Accounts.Managed;

namespace EthereumTransactionAPI.Controllers
{
[ApiController]
[Route("[controller]")]
public class TransferController : ControllerBase
{
    private readonly IMongoClient _mongoClient;

    public TransferController(IMongoClient mongoClient)
    {
        _mongoClient = mongoClient;
    }

    [HttpPost]
    [Route("submit-transfer")]
    public async Task<IActionResult> SubmitTransfer([FromBody] TransferDataRequest request)
    {
        if (request.Cmds == null || !request.Cmds.Any())
        {
            return BadRequest("Invalid or missing cmds array");
        }

        var database = _mongoClient.GetDatabase("omniassets");
        var collection = database.GetCollection<TransferData>("transferdatas");

        foreach (var cmd in request.Cmds)
        {
            var transferData = new TransferData { encodedData = cmd };
            await collection.InsertOneAsync(transferData);
        }

        return Ok("Transfer data saved");
    }
}

}
