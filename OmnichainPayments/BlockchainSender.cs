using Cila.Models;
using Nethereum.Model;
using Nethereum.Web3;
using Nethereum.HdWallet;
using Nethereum.Hex.HexTypes;
using NBitcoin;

namespace Cila
{
    public class BlockchainSender
{
    private Web3 _web3;
    private string _contractAddress = "0x30d634235B5b3d07Faef206Ac23Db82340C5B412"; // Your contract's address
    private string contractABI;

    public BlockchainSender(IConfiguration configuration)
    {
        var mnemonic = File.ReadAllText(".mnemonic");
        Console.WriteLine(mnemonic);
        var wallet = new Wallet(mnemonic, null);
        var account = wallet.GetAccount(0);
        contractABI = File.ReadAllText("Processor.json");
         var parsedAbi = JArray.Parse(contractABI);
        _web3 = new Web3(account, "https://rpc.sepolia.org/");
    }

    public async Task SendToBlockchain(List<TransferData> transferDataList)
    {
        try
        {
            var contract = _web3.Eth.GetContract(contractABI, _contractAddress);
            var processCmdsFunction = contract.GetFunction("processCmds");

            var serializedData = transferDataList.Select(d => d.encodedData).ToList();

            var gas = await processCmdsFunction.EstimateGasAsync(serializedData);
            var gasPrice = await _web3.Eth.GasPrice.SendRequestAsync();

            var result = await processCmdsFunction.SendTransactionAndWaitForReceiptAsync(
                _web3.TransactionManager.Account.Address, 
                new HexBigInteger(gas), 
                new HexBigInteger(gasPrice), 
                new HexBigInteger(0), 
                new HexBigInteger(0), // This is likely the missing piece
                serializedData.ToArray()
            );
            Console.WriteLine("Transaction result: " + result.TransactionHash);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Failed to send to blockchain: " + ex.Message);
        }
    }
}

}