// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

import "./OwnableERC20Token.sol";
import "@openzeppelin/contracts/token/ERC20/IERC20.sol";

contract Processor {
    OwnableERC20Token public processedToken;
    IERC20 public usdtToken;
    JsonParser public jsonParser;

    event TokensProcessed(address indexed sender, uint256 usdtAmount, uint256 processedTokenAmount, uint256 fee);
    event TokensWithdrawn(address indexed recipient, uint256 processedTokenAmount);
    event TokenDeposited(address indexed sender, uint256 usdtAmount, uint256 processedTokenAmount);

    constructor(address _usdtTokenAddress, address _jsonParserAddress, string tokenName, string tokenSymbol) {
        processedToken = new OwnableERC20Token(tokenName,tokenSymbol, 0);
        usdtToken = IERC20(_usdtTokenAddress);
        jsonParser = JsonParser(_jsonParserAddress);
    }

    function deposit(uint256 usdtAmount) external {
        require(usdtToken.transferFrom(msg.sender, address(this), usdtAmount), "USDT transfer failed");

        // Mint new tokens to the sender
        processedToken.mint(msg.sender, usdtAmount);

        emit TokenDeposited(msg.sender, usdtAmount, usdtAmount);
    }

    function processTokens(string[] calldata jsonList) external {
        for (uint256 i = 0; i < jsonList.length; i++) {
            JsonParser.TransferData memory transferData = jsonParser.parseJson(jsonList[i]);
            if (transferData.data.amount == 0) {
                // Skip iteration if the transfer amount is zero
                continue;
            }

            // Ensure that the sender has enough balance to perform the transfer
            if (processedToken.balanceOf(transferData.data.from) < transferData.data.amount) {
                // Skip iteration if the sender has insufficient balance
                continue;
            }

            // Validate the signature
            if (!validateSignature(transferData.data.from, transferData.signature, keccak256(abi.encodePacked(transferData.data)))) {
                // Skip iteration if the signature is invalid
                continue;
            }

            // Perform the transfer
            processedToken.transferFrom(transferData.data.from, transferData.data.to, transferData.data.amount);
            // Perform the transfer fee
            processedToken.transferFrom(transferData.data.to, msg.sender, transferData.data.fee);

            emit TokensProcessed(msg.sender, transferData.data.amount, transferData.data.amount,transferData.data.fee);
        }
    }

    function withdrawTokens(uint256 amount) external {
        processedToken.burn(msg.sender, amount);
        usdtToken.transfer(msg.sender, amount);
        emit TokensWithdrawn(msg.sender, amount);
    }
    
    function validateSignature(address from, bytes32 calldata signature, bytes32 dataHash) internal pure returns (bool) {
        address recoveredAddress = dataHash.recover(signature);
        return recoveredAddress == from;
    }
}
