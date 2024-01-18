const express = require('express');
const mongoose = require('mongoose');
const TransferData = require('./models/TransferData'); // The Mongoose model

const app = express();
app.use(express.json());

mongoose.connect('mongodb://localhost:27017/yourDatabase', { useNewUrlParser: true });

app.post('/submit-transfer', async (req, res) => {
  const transferData = new TransferData(req.body);
  await transferData.save();
  res.status(200).send('Transfer data saved');
});

app.listen(3000, () => {
  console.log('Server is running on port 3000');
});
