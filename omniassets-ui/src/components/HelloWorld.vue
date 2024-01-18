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
import erc20Abi from './erc20Abi.json'; // Ensure you have the ERC-20 ABI

export default {
  data() {
    return {
      web3: null,
      account: null,
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
      // Additional code to handle submission
    },
    async updateBalances() {
      if (!this.web3 || !this.account) {
        console.error('Web3 is not initialized or account not set.');
        return;
      }
      // Replace with actual contract addresses
      const tokenContract1 = new this.web3.eth.Contract(erc20Abi, '0xc4bF5CbDaBE595361438F8c6a187bDc330539c60');
      const tokenContract2 = new this.web3.eth.Contract(erc20Abi, '0xfa11f07672304a4e95615ea09569864a9ddf3d8e');

      try {
        const balance1 = await tokenContract1.methods.balanceOf(this.account).call();
        const balance2 = await tokenContract2.methods.balanceOf(this.account).call();

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
