const cron = require('node-cron');
const TransferData = require('./models/TransferData'); // The Mongoose model
const sender = require ('./sender')

cron.schedule('*/1 * * * *', async () => {
  try {
    const transferDataList = await TransferData.find({});

    if (transferDataList.length > 0) {
        await sender.sendToBlockchain(transferDataList);

        // Delete the processed data
        await TransferData.deleteMany({ _id: { $in: transferDataList.map(data => data._id) } });
    }
  } catch (error) {
      console.error('Error in scheduled task:', error);
  }
});
