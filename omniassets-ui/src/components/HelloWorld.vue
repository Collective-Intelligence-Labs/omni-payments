<template>
  <div class="transfer-form">
    <h1>OmniAssets</h1>
    <form @submit.prevent="submitForm">
      <div class="form-group">
        <label for="account">Account</label>
        <input type="text" id="account" v-model="formData.account" readonly>
      </div>
      <div class="form-group">
        <label for="balance">Balance</label>
        <input type="text" id="balance" v-model="balance" readonly>
      </div>
      <div class="form-group">
        <label for="amount">Amount</label>
        <input type="number" id="amount" step="0.01" v-model="formData.amount">
      </div>
      <div class="form-group">
        <label for="fee">Fee</label>
        <input type="number" id="fee" step="0.01"   v-model="formData.fee">
      </div>
      <div class="form-group">
        <label for="to">Recipient</label>
        <input type="text" id="to" v-model="formData.to">
      </div>
      <div class="form-group">
        <label for="deadline">Deadline</label>
        <input type="date" id="deadline" v-model="formData.deadline">
      </div>
      <button type="submit">Send</button>
    </form>
  </div>
</template>

<script>
import Web3 from 'web3';
import erc20Abi from './ghoAbi.json'; // Ensure you have the ERC-20 ABI
import axios from 'axios';


export default {
  data() {
    return {
      web3: null,
      account: null,
      targetTokenContract: null,
      omniTokenContract: null,
      processorContractAddress: "0x7F3776104f6aD3EF1D8DC211b3B03FD6B55d03AD",
      formData: {
        account: '0x0032s...30asdas',
        balance: '10000 USDT',
        amount: '0.00',
        fee: '0.01',
        to: '',
        deadline: this.getDefaultDeadline()
      },
      tokenBalances: {
        token1: '0',
        token2: '0'
      }
    }
  },
  computed: {
    // Computed property to calculate the sum of token balances
    balance() {
      const balance1 = parseFloat(this.tokenBalances.token1);
      const balance2 = parseFloat(this.tokenBalances.token2);
      return (balance1 + balance2).toFixed(2) + " USDT"; // Assuming 2 decimal places for ERC-20 tokens
    },
    // ... other computed properties if needed ...
  },
  methods: {
    async submitForm() {
      // Handle the form submission
      console.log('Form data:', this.formData);

     

      let cmds = [];
      let amountToTransfer =  Math.min(this.tokenBalances.token2, this.formData.amount);
      let amountToDeposit =  this.formData.amount - amountToTransfer;

      console.log(amountToTransfer + ": amountToTransfer");
      console.log(amountToDeposit + ": amountToDeposit");


      const transferData = {
          cmd_id: Web3.utils.hexToNumber(this.web3.utils.randomHex(32)),
          cmd_type: 2,
          amount: Web3.utils.toWei(amountToDeposit, "ether"),
          from: this.account,
          to: this.formData.to,
          fee: Web3.utils.toWei(this.formData.fee, "ether"),
          deadline: Math.floor(Date.now() / 1000) + 4200
        };

        console.log('Transfer data:', transferData);
      if (amountToDeposit > 0)
      {
        cmds.push(await this.encodeTransferData(transferData));
      }

      if (amountToTransfer > 0)
      {
        transferData.amount = Web3.utils.toWei(amountToTransfer, 'ether');
        transferData.cmd_type = 1;
        cmds.push(await this.encodeTransferData(transferData));
      }
      // Additional code to handle submission
console.log("CMDS: ")
console.log(cmds);
          const response = await axios.post('http://localhost:3000/submit-transfer', {cmds: cmds});
          console.log('Server response:', response.data);
     
    },
    async updateBalances() {
      if (!this.web3 || !this.account) {
        console.error('Web3 is not initialized or account not set.');
        return;
      }
      // Replace with actual contract addresses
      this.targetTokenContract = new this.web3.eth.Contract(erc20Abi, '0xc4bF5CbDaBE595361438F8c6a187bDc330539c60');
      this.omniTokenContract = new this.web3.eth.Contract(erc20Abi, '0xd5Db8bA2849237280ad2c43017fD5989E6f4CBFD');

      try {
        const balance1 = await this.targetTokenContract.methods.balanceOf(this.account).call();
        const balance2 = await this.omniTokenContract.methods.balanceOf(this.account).call();

        this.tokenBalances.token1 = this.web3.utils.fromWei(balance1, 'ether');
        this.tokenBalances.token2 = this.web3.utils.fromWei(balance2, 'ether');

        this.formData.balance = parseFloat(this.tokenBalances.token1) + parseFloat(this.tokenBalances.token2);
      } catch (error) {
        console.error('Error fetching balances:', error);
      }
    },
    getDefaultDeadline() {
      const tomorrow = new Date();
      tomorrow.setDate(tomorrow.getDate() + 1);
      return tomorrow.toISOString().split('T')[0]; // Format as YYYY-MM-DD
    },

    async signCmd(data, account){
      const hash = Web3.utils.keccak256(data);
      return await this.web3.eth.sign(hash, account);
    },
    async signPermit(
  owner,    // address of the token owner
  spender,  // address of the spender
  value,    // value to permit
  deadline  // deadline of the permit
) {
  const chainId = await this.web3.eth.getChainId();
  const nonce = await this.targetTokenContract.methods.nonces(owner).call();

  // Convert numeric values to strings
  const valueStr = value.toString();
  const deadlineStr = deadline.toString();
  const nonceStr = nonce.toString();
  const chainIdStr = chainId.toString();

  const domain = [
    { name: "name", type: "string" },
    { name: "version", type: "string" },
    { name: "chainId", type: "uint256" },
    { name: "verifyingContract", type: "address" },
  ];

  const permit = [
    { name: "owner", type: "address" },
    { name: "spender", type: "address" },
    { name: "value", type: "uint256" },
    { name: "nonce", type: "uint256" },
    { name: "deadline", type: "uint256" },
  ];
  const domainData = {
    name: await this.targetTokenContract.methods.name().call(),
    version: "1",
    chainId: chainIdStr,
    verifyingContract: this.targetTokenContract._address,
  };

  const message = {
    owner: owner,
    spender: spender,
    value: valueStr,
    nonce: nonceStr,
    deadline: deadlineStr,
  };

  const data = JSON.stringify({
    types: {
      EIP712Domain: domain,
      Permit: permit,
    },
    domain: domainData,
    primaryType: "Permit",
    message: message,
  });

  const accounts = await this.web3.eth.getAccounts();
  if (!accounts[0]) throw new Error("No account is connected");

  return new Promise((resolve, reject) => {
    this.web3.currentProvider.send(
      {
        method: "eth_signTypedData_v4",
        params: [accounts[0], data],
        from: accounts[0],
      },
      function (err, result) {
        if (err) return reject(err);
        resolve(result.result);
      }
    );
  });
},
    async encodeTransferData(transferData) {

      // Encode AssetTransfer data (pseudo-code, replace with your actual data encoding)
      const encodedData = this.web3.eth.abi.encodeParameters(
          ['uint256', 'uint256', 'uint256', 'address', 'address', 'uint256', 'uint256'], 
          [transferData.cmd_id, transferData.cmd_type, transferData.amount, transferData.from, transferData.to, transferData.fee, transferData.deadline]
      );

      // Sign the data
      let signature = null;

      if (transferData.cmd_type == 2) { // DEPOSIT case signature
          // sign the Permit type data with the deployer's private key
          signature = await this.signPermit(transferData.from, this.processorContractAddress, transferData.amount, transferData.deadline)
      } else {
        signature = await this.signCmd(encodedData, transferData.from);
      }

      // Construct the TransferData object
      const transferDataObject = {
          signature: signature,
          data: encodedData // Depending on how your contract expects this
      };

      console.log("Sig: " + signature);

      // Encode the entire TransferData object as bytes
      const transferDataEncoded = this.web3.eth.abi.encodeParameters(
          ['bytes', 'bytes'],
          [signature, transferDataObject.data]
      );

      return transferDataEncoded;
    },
  },
  mounted() {
    if (window.ethereum) {
      this.web3 = new Web3(window.ethereum);
      window.ethereum.request({ method: 'eth_requestAccounts' })
        .then(accounts => {
          this.account = accounts[0];
          this.formData.account = this.account;
          this.updateBalances();
        })
        .catch(error => {
          console.error('Error requesting accounts:', error);
        });
    } else {
      alert('Please install MetaMask to use this feature.');
    }
  }
}
</script>
<style>
.transfer-form {
  max-width: 500px;
  margin: 3rem auto;
  padding: 2rem;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
  font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif;
}

.transfer-form h1 {
  font-size: 2rem;
  color: #333;
  margin-bottom: 2rem;
}

.transfer-form input[type="text"],
.transfer-form input[type="number"],
.transfer-form input[type="date"] {
  width: calc(100% - 24px); /* Adjusts input width considering padding */
  padding: 12px;
  margin-bottom: 1rem;
  border: 1px solid #dfe3e9;
  border-radius: 6px;
  font-size: 1rem;
  box-sizing: border-box; /* Includes padding and border in the element's width and height */
}

.transfer-form input[type="text"]:read-only,
.transfer-form input[type="number"]:read-only {
  background-color: #f9f9f9;
  color: #bec3c9;
}

.transfer-form button[type="submit"] {
  width: 100%;
  padding: 15px 0;
  background-color: #3498db;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 1rem;
  cursor: pointer;
  transition: background-color 0.2s;
}

.transfer-form button[type="submit"]:hover {
  background-color: #2980b9;
}

@media (max-width: 768px) {
  .transfer-form {
    margin: 2rem;
    padding: 1.5rem;
  }
  .transfer-form h1 {
    margin-bottom: 1.5rem;
  }
}
</style>
