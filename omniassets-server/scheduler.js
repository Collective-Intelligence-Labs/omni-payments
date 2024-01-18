const cron = require('node-cron');
const web3 = require('web3');
const TransferData = require('./models/TransferData'); // The Mongoose model

cron.schedule('* * * * *', async () => {
  const transferDataList = await TransferData.find({});

    // Logic to send to Ethereum blockchain
    // e.g., using web3.eth.Contract and your contract's processCmds method

    // After successful transaction, remove these entries from MongoDB
    await TransferData.deleteMany({ _id: { $in: transferDataList.map(data => data._id) } });
  
});
