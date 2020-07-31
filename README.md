# Sample dApp Introduction

Mage Duel is a TCG (trading card game) implemented as a LINE Blockchain sample service. Players duel with each other, using their card deck. Mage Duel consists of a server and client. Client is developed with Unity 3D, providing screens for blockchain-based login, card purchase, card deck building and card trading. Client connects to LINE Blockchain via its game server. Note that actual card duel mode isn’t provided in this sample.

## System Requirement
### OS
* Windows
### Language
* C# (.Net Framework 4.7.2)
### Database 
* MariaDB 10.4.0

## Config_Setting
### TCGSampleSrvProgramSetting.xml  Setting
```bash
<?xml version="1.0" encoding="utf-8"?>									
<root>									
	<serverhostinfo id="1"	           // When the Multi Server is configured through ServerID, it is used as the server's separator. Currently not used.
		host="0.0.0.0"             // Tcp Server Socket Host
		port="18282"               // Tcp Server Socket Port
		keepalivechecktime="1200"  // If the Time Update is not performed so that the relevant time passes with the Check Time for the connected connection, the connection is forcibly terminated (Unit: Seconds)
		sendingcycle="100"         // Time to send KeepAlive Packet to Client (Unit: seconds)
	/>								
									
	<serverwebhostinfo host="0.0.0.0" port="28282"/>   // WebServer Host and Port to receive LINE Login CallBack
									
	<resourceinfo path="../DataTable"/>                // Resource Table (Excel File) path
									
	<mysqlinfo server="10.113.~~~~"      	               // Sql DB Host
		uid="admin~~~~~"                               // Db Account
		pwd="admin~~~~"                                // DB Password
		database=""                                    // Name of the database to connect to (if blank, it is set as {processName}_{System.Environment.MachineName})
		sslmode="none"                                 // Default: none
		allow_user_variables="true"                    // Default: true
		database_create="false"                        // If true, delete the existing database and table and create a new one to access. If False, access to the established Database.
	/>								
									
	<lbdinfo uri="https://test-api-blockchain.line-beta.me"  // LBD Open Api Parent url
		dappid="6fee1e3c-~~~~~~~"                            // DappId : Created by LBD Dev.Console
		apikey="a17401a9-da4~~~~~~"                          // ApiKey : Created by LBD Dev.Console
		apisecret="f5e4b764-~~~~~~~"                         // ApiSecret : Created by LBD Dev.Console
		itemtoken_contractid="2d8~~~~"                       // itemtoken ContractId : Created by LBD Dev.Console
		servicetoken_contractid="f93~~~~~"                   // ServiceToken ContractId : Created by LBD Dev.Console
		operaddr="tlink1u8~~~~~~~"                           // Operator Wallet : Created by LBD Dev.Console
		secretkey="O+S9xIqi~~~~~~~~"                         // Operator Wallet SecretKey : Created by LBD Dev.Console
	/>								
									
	<ldcinfo uri="https://api.line-beta.me"/>  	             // LINE Login url
</root>									
```

## Sample dApp item tokenization
### Mage Duel tokenizes in-game assets. Let’s take a look at how various LINE Blockchain tokens are used.
##### Ruby
* Primary in-game currency
* Fungible Service Token
* Players can use Rubies to purchase card packs or Coins at the shop.

##### Coin
* Secondary in-game currency.
* Fungible item token
* Players can use Coins to pay for card sales registration fee and purchase cards at the card exchange.

##### Hero card
* Character card. 
* Non-fungible item token
* Primary card to create a deck.

##### Regular card
* Monster card/ equipment card/ magic card 
* Non-fungible item token
* Monster card/ equipment card/ magic card that can be included in the deck.

## Game features and blockchain
### Here is which blockchain feature is used on each screen of Mage Duel.
##### Deck builder
* Attaching or detaching, using composable tokens
* Registering up to 40 regular cards (child token) to the hero card (parent token)

##### Shop
* Purchasing a card pack (Five random regular cards)with rubies
  - Transferring Rubies from the user wallet to the service wallet
  - Minting 5 non-fungible item tokens 

* Purchasing coins with rubies		
  - Transferring(or Minting) Coins from the service wallet to the user wallet 
  - Transferring Rubies from the user wallet to the service wallet	

##### Card exchange
* Registering card sales and deducting registration fee	
  - Transferring Coins from the user wallet to the service wallet
* Purchasing cards registered for sale by users
  - Transferring Coins between user wallets 
  - Transferring cards between user wallets

## How to contribute

See [CONTRIBUTING.md](CONTRIBUTING.md) for more details.

## License

```
Copyright 2020 LINE Corporation

LINE Corporation licenses this file to you under the Apache License,
version 2.0 (the "License"); you may not use this file except in compliance
with the License. You may obtain a copy of the License at:

  https://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
License for the specific language governing permissions and limitations
under the License.
```

See [LICENSE](LICENSE) for more details.

