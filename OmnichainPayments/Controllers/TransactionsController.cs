using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Nethereum.Signer.Crypto;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Web3.Accounts.Managed;

namespace EthereumTransactionAPI.Controllers
{
    public class TransactionEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string TransactionJson { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly Web3 _web3;

        private static readonly string ConnectionString = "your_mongodb_connection_string";
        private static readonly string DatabaseName = "your_database_name";
        private static readonly string CollectionName = "transactions";

        private static IMongoCollection<TransactionEntity> GetTransactionCollection()
        {
            var client = new MongoClient(ConnectionString);
            var database = client.GetDatabase(DatabaseName);
            return database.GetCollection<TransactionEntity>(CollectionName);
        }


        public TransactionsController()
        {
            // Ethereum node URL
            string nodeUrl = "https://mainnet.infura.io/v3/YOUR_INFURA_API_KEY"; // Replace with your Infura API key or use your own Ethereum node

            // Ethereum wallet private key
            string privateKey = "YOUR_WALLET_PRIVATE_KEY"; // Replace with your wallet's private key

            var account = new Account(privateKey);
            // Create a Web3 instance
            _web3 = new Web3(account, nodeUrl);
        }

        [HttpPost("process")]
        public IActionResult ProcessTransactions([FromBody] List<string> transactions)
        {
            try
            {
                // Validate and process transactions
                ProcessTransactionsAsync(transactions).Wait();
                return Ok("Transactions processed successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error processing transactions: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task ProcessTransactionsAsync(List<string> transactions)
        {
            // Send valid transactions to the Ethereum smart contract
            if (transactions.Any())
            {
                await SendTransactionsToSmartContractAsync(transactions);
            }
        }

        private async System.Threading.Tasks.Task SendTransactionsToSmartContractAsync(List<string> transactions)
        {
            // Ethereum smart contract address
            string contractAddress = "YOUR_CONTRACT_ADDRESS"; // Replace with the actual address of your smart contract

            // Get the contract instance
            var processorContract = _web3.Eth.GetContract(null, contractAddress);

            // Encode the function call for processTokens
            var processTokensFunction = processorContract.GetFunction("processTokens");
            var encodedData = processTokensFunction.GetData(transactions.ToArray());

            // Send the transaction
            var transactionInput = new Nethereum.RPC.Eth.DTOs.TransactionInput { To = contractAddress, Data = encodedData };
            var transactionHash = await _web3.Eth.Transactions.SendTransaction.SendRequestAsync(transactionInput);

            Console.WriteLine($"Transaction sent. Hash: {transactionHash}");
        }
    }
}
