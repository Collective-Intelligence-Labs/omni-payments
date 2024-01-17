// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

import "@openzeppelin/contracts/token/ERC20/IERC20.sol";
import "./OwnableERC20Token.sol";

contract Processor {
    OwnableERC20Token public processedToken;
    IERC20 public usdtToken;

    struct TransferData {
        bytes32 signature;
        bytes32 r;
        uint8 v;
        AssetTransfer data;
    }

    struct AssetTransfer {
        uint256 amount;
        address from;
        address to;
        uint256 fee;
        uint256 nonce;
        uint256 deadline;
    }

    mapping(uint256 nonce => uint256) private _nonces; 

    event TokensProcessed(address indexed sender, uint256 usdtAmount, uint256 processedTokenAmount, uint256 fee);
    event TokensWithdrawn(address indexed recipient, uint256 processedTokenAmount);
    event TokenDeposited(address indexed sender, address recipent, uint256 processedTokenAmount);

    constructor(address _targetTokenAddress, address _internalTokenAddress) {
        processedToken = OwnableERC20Token(_internalTokenAddress);
        usdtToken = IERC20(_targetTokenAddress);
    }

    function deposit(uint256 usdtAmount, address from, address to) external {
        require(usdtToken.transferFrom(from, address(this), usdtAmount), "ERC20 Token transfer failed");

        // Mint new tokens to the sender
        processedToken.mint(to, usdtAmount);

        emit TokenDeposited(msg.sender, to, usdtAmount);
    }

    function processTokens(TransferData[] memory jsonList) external {
        for (uint256 i = 0; i < jsonList.length; i++) {
            TransferData memory transferData = jsonList[i];
            if (transferData.data.amount == 0) {
                // Skip iteration if the transfer amount is zero
                continue;
            }

            if (_nonces[transferData.data.nonce] == 0)
            {
                continue;
            }

            if (transferData.data.deadline < block.timestamp)
            {
                continue;
            }


            // Ensure that the sender has enough balance to perform the transfer
            if (processedToken.balanceOf(transferData.data.from) < transferData.data.amount) {
                // Skip iteration if the sender has insufficient balance
                continue;
            }

            // Validate the signature
            if (!validateSignature(transferData.data.from, transferData.v, transferData.r, transferData.signature, keccak256(abi.encodePacked(transferData.data.amount, transferData.data.from, transferData.data.to, transferData.data.fee, transferData.data.nonce, transferData.data.deadline))))
            {   // Skip iteration if the signature is invalid
                continue;
            }

            // Perform the transfer
            processedToken.transferFrom(transferData.data.from, transferData.data.to, transferData.data.amount);
            // Perform the transfer fee
            processedToken.transferFrom(transferData.data.to, msg.sender, transferData.data.fee);

            _nonces[transferData.data.nonce] = block.timestamp;

            emit TokensProcessed(msg.sender, transferData.data.amount, transferData.data.amount,transferData.data.fee);
        }
    }

    function withdrawTokens(uint256 amount) external {
        require(processedToken.balanceOf(msg.sender) > amount);
        processedToken.burn(msg.sender, amount);
        usdtToken.transfer(msg.sender, amount);
        emit TokensWithdrawn(msg.sender, amount);
    }
    
    function validateSignature(address from,   uint8 v,  bytes32 r, bytes32 s, bytes32 dataHash) internal pure returns (bool) {
        address recoveredAddress = ecrecover(dataHash, v, r, s);
        return recoveredAddress == from;
    }
}
