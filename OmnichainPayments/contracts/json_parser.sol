// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

import "@openzeppelin/contracts/token/ERC20/IERC20.sol";
import "@openzeppelin/contracts/utils/cryptography/ECDSA.sol";
import "https://github.com/abdk-consulting/abdk-libraries-solidity/blob/master/ABDKMathQuad.sol";
import "https://github.com/ethereum/solidity/blob/develop/libsolidity/imports/multi/abiv2/abiEncoderV2.sol";

contract JsonParser {
    using ABDKMathQuad for bytes16;
    using ABIEncoderV2 for bytes;
    using ECDSA for bytes32;

    struct TransferData {
        bytes32 signature;
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

    function parseAndExecuteJsonList(string[] calldata jsonList) external returns (TransferData[] memory transfers) {
        transfers = new TransferData[](jsonList.length);

        for (uint256 i = 0; i < jsonList.length; i++) {
            transfers[i] = parseJson(jsonList[i]);
        }
    }

    function parseJson(string calldata jsonData) internal pure returns (TransferData memory) {
        (string memory signature, AssetTransfer memory data) = abi.decode(jsonData.bytesToBytes(), (string, AssetTransfer));

        return TransferData({
            signature: signature,
            data: data
        });
    }
}
