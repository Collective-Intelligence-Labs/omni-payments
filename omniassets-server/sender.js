const Web3 = require('web3');
const contractABI = require('./processor.abi.json'); // Load your contract ABI
const contractAddress = '0x4b8ee21b54e63913fbcc0a83e55232b62d33136c'; // Your contract's address

const web3 = new Web3('put Sepolia RPC here'); // Replace with your Ethereum node URL

// Ensure this account is unlocked and has sufficient balance for gas
const fromAddress = '0x8AeAB625b8c29A087158FB44215A6852277aB35b'; 

const privateKey = '';

async function sendToBlockchain(transferDataList) {
  try {
    const contract = new web3.eth.Contract(contractABI, contractAddress);

    // Serialize the transfer data
    const serializedData = transferDataList.map(data => ({
      s: data.s,
      r: data.r,
      s: data.s,
      data: [
        data.data.cmd_id,
        data.data.cmd_type,
        data.data.amount,
        data.data.from,
        data.data.to,
        data.data.fee,
        data.data.deadline
      ]
    }));

    // Send the transaction
    const tx = contract.methods.processCmds(serializedData);
    const gas = await tx.estimateGas({ from: fromAddress });
    const gasPrice = await web3.eth.getGasPrice();

    const result = await tx.send({ from: fromAddress, gas, gasPrice });
    console.log('Transaction result:', result);
  } catch (error) {
    console.error('Failed to send to blockchain:', error);
  }
}

// Example usage with dummy data
sendToBlockchain([/* array of TransferData objects */]);
