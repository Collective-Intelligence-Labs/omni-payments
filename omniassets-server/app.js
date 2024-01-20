const express = require('express');
const mongoose = require('mongoose');
const cors=require('cors');
const TransferData = require('./models/TransferData'); // The Mongoose model


const app = express();
app.use(express.json());
app.use(cors())
mongoose.connect('mongodb://localhost:27017/omniassets', { useNewUrlParser: true });

app.post('/submit-transfer', async (req, res) => {
  const cmds = req.body.cmds;
    if (!cmds || !Array.isArray(cmds)) {
        return res.status(400).send('Invalid or missing cmds array');
    }

  for (let index = 0; index < cmds.length; index++) {
    const encodedData = cmds[index];

    const transferData = new TransferData({
      encodedData: encodedData
    });
    await transferData.save();
    
  }
  res.status(200).send('Transfer data saved');
});


app.listen(3000, () => {
  console.log('Server is running on port 3000');
});

