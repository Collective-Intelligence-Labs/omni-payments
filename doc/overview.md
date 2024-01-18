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

On the aggregation server, all the commands (aka omnichain transaction payloads) are aggregated in database.

Every period period of time (paramenter: aggregation_timespan) all the aggregated cmds are sent to the execution layer in batches taking into account optimal batch size (paramenter: current_optimal_batch_size). Optimal batch size can vary depends on the network load. 

If rduing aggregation_timespan there are already aggreageted number of commands exceeeds current_optimal_batch_size - then it send the batch imiddiatedly, not waiting for the end of the aggregation_timespan . Therefor maximum time for the aggregation exptected to not exceed aggregation_timespan. 

When the cmd is sent succefully to the blockchain, the commands is marked as sent to blockchain and can be archived.


## Smart-contract

The enterance point to all the omnichain transaction is processCmd method of processor contract. Even deposit of funds to omnichain smart contract is done through the same method. It has thee types of commands to process with the follusing id of the types:

    uint public constant TRANSFER = 1;
    uint public constant DEPOSIT = 2;
    uint public constant WITHDRAW = 3;

for every command processor validates the signature of the commands payload by calculating the hash of the payload in this way

    keccak256(abi.encodePacked(transferData.data.cmd_id, transferData.data.cmd_type, transferData.data.amount, transferData.data.from, transferData.data.to, transferData.data.fee, transferData.data.deadline))


and then validatinf signature

    ecrecover(dataHash, v, r, s);

then it executes the logic based on the type of the command and trigger a corresponding event


NOTE: digital hash can be potentially calculated the same way as in permit method of targeted ERC-20 contract
and just use different cmdtype 

    keccak256(abi.encodePacked(
    hex"1901",
    DOMAIN_SEPARATOR,
    keccak256(abi.encode(
                keccak256("Permit(address owner,address spender,uint256 value,uint256 nonce,uint256 deadline)"),
                owner,
                spender,
                value,
                nonce,
                deadline))
    ))

The Processor smart contract keeps the internal state of all the balances in internalERC20 smart contract. With the transfer is done from the balance that is in internal to internace addres - smart contract processes TRANSFER command. When the transfer is done from from external to internal - it processes DEPOSIT command. When the transfer is done from internal to external - it process WITHDRAW command.