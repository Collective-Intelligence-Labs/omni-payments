# The architecture of omnichain transactions/payments solution

## Abstract

Currently most of transaction in any blockchain are performed by paying a tranfer comission with a native token of the network. With the development of stablecoins there is an adoption of crypto payments in these stable coins pegged by USD or any other fiat currenct. Howevery in most network you still have to pay a transaction fee in the native currency which is in most of the cases is highl volatile. We are proposing a solution which not only allows to pay transaction fee in stable-coins, but also allow to signfically lower transaction fees and completely abstract user from knowing which blockchain network performs the transfer.


## Batching and Aggregation Layer

In order to reduce transaction cost and fully abstract user from the blockchain we create an additional layer between the application and the blockchain - an aggregation layer. Aggreation will not only be able to  charge user/application in any convininet way for the user for the usage of the network (both read and write operations) but will also batch all the transactions to the blockchain so they are not cobsuming suboptimal network fee and do not overload the network.

## Client side

On the client side there are multiple changes comparing to how regular interaction with ERC-20 happens. 

with OmniAssets (Omni ERC) the change is that the actual balance of the Omni Asset token is tracked on the other token (Internal Token) and all the transfers are perfomed there after token enterc ERC contract. The main change with the Inernal token contract can actually be perfomed within a single original Target ERC20 contract, converting it to Omni ERC20. Having a seperate token creates a possibility to merge various stable coins into a single pool - deversifying a risk of collapsing any of the assets.

Currently on the client side when user want to send the asset we use an integrated web3.js with any browseer extention to sign two transaction - one request permit payload for the target asset (for example USDT), the permit methos is a special method simmilar to allowTransfer but can be performed by third party. It's ideal for us, because we want to provide a user expirience when the user does not have to pay comission fee in native currentcy at all. 

The user needs to setup a number of fields for the Permit method - in general putting the total amout of tokens to be transferred to Omnichain address.

Also user needs to form an omnichain transfer payload:

        uint256 amount;
        address from;
        address to;
        uint256 fee;
        uint256 nonce;
        uint256 deadline;


amount - the total amount of assets to be transfered. 
address - from what address the transfer is performed.
to - the destination of the transfer.
fee - the fee to be paid to the router of this transaction.
nonce - a unique number for this commands (can be considered as omnichain transaction ID),
deadline - the expiration date for this CMD - if it becomes obsolte it's no longer can be commited to the blockchain,


if user has a nessesary balance in omnichain there is no need for additional permit/deposit transaction.

Withdraw should be performed by the user directly since the router has no inristic incentive to process withdraw transaction for the user and if user want to withdraw assets the user should have native currency for performing withdraw transaction.

## Aggregation

All the transactions are aggregated on the aggreagtion layer. So there is no need to perform any transaction directly to a blockchain. All the transaction including deposit to omnichain account, withdraw to native account, or transfer between omnichain transaction. 


## Smart-contract