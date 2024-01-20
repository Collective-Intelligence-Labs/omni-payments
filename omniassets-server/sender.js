require('dotenv').config();
const HDWalletProvider = require('@truffle/hdwallet-provider');
const web3 = require('web3');
const contractABI = require('./Processor.json'); // Load your contract ABI
const contractAddress = '0x7F3776104f6aD3EF1D8DC211b3B03FD6B55d03AD'; // Your contract's address


// Replace with your Ethereum node URL and mnemonic
const mnemonic = process.env["MNEMONIC"];
const provider = new HDWalletProvider(mnemonic, 'https://rpc.sepolia.org/');

async function sendToBlockchain(transferDataList) {
  try {

    
    const contract = new web3.eth.Contract(contractABI, contractAddress);

    const accounts = await web3.eth.getAccounts();
    const fromAddress = accounts[0]; // Using the first account based on the mnemonic

    // Serialize the transfer data
    const serializedData = transferDataList.map(doc => doc.encodedData);

    // Prepare and send the transaction
    const tx = contract.methods.processCmds(serializedData);
    const gas = await tx.estimateGas({ from: fromAddress });
    const gasPrice = await web3.eth.getGasPrice();
    
    const result = await tx.send({ from: fromAddress, gas, gasPrice });
    console.log('Transaction result:', result);
  } catch (error) {
    console.error('Failed to send to blockchain:', error);
  }
}

module.exports = { sendToBlockchain };
