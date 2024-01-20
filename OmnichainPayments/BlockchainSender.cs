using Cila.Models;
using Nethereum.Model;
using Nethereum.Web3;
using Nethereum.HdWallet;
using NBitcoin;

namespace Cila
{

    public class BlockchainSender
{
    private Web3 _web3;
    private string _contractAddress = "0x7F3776104f6aD3EF1D8DC211b3B03FD6B55d03AD"; // Your contract's address
    private string _mnemonic; // Your mnemonic

    public BlockchainSender(IConfiguration configuration)
    {
        var mnemonic = configuration["MNEMONIC"];
        var wallet1 = new Wallet(Wordlist.AutoDetect(_mnemonic));
        var wallet = new Account(_mnemonic, accountIndex: 0);
        var account = wallet.GetAccount(0);

        _web3 = new Web3(account, "https://rpc.sepolia.org/");
    }

    public async Task SendToBlockchain(List<TransferData> transferDataList)
    {
        try
        {
            var contract = _web3.Eth.GetContract(contractABI, _contractAddress);
            var processCmdsFunction = contract.GetFunction("processCmds");

            var serializedData = transferDataList.Select(d => d.EncodedData).ToList();

            var gas = await processCmdsFunction.EstimateGasAsync(serializedData);
            var gasPrice = await _web3.Eth.GasPrice.SendRequestAsync();

            var result = await processCmdsFunction.SendTransactionAndWaitForReceiptAsync(_web3.TransactionManager.Account.Address, new HexBigInteger(gas), new HexBigInteger(gasPrice), new HexBigInteger(0), null, serializedData);

            Console.WriteLine("Transaction result: " + result.TransactionHash);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Failed to send to blockchain: " + ex.Message);
        }
    }
}

}