# Mage Duel sample service
 
Mage Duel is a TCG (Trading Card Game) implemented as a LINE Blockchain sample service. Players duel with each other, using their card deck. Mage Duel consists of a server and client. This repository only contains server-side code.
 
Visit [LINE Blockchain Docs](https://docs.blockchain.line.me/sample-services/Mage-duel) for more details.
 
## Development environment
### OS
* Windows
### Language
* C# (.Net Framework 4.7.2)
### Database
* MariaDB® 10.4.0
 
## Configuration
Mage Duel server needs a configuration file (`Server/TCGSampleServer/Config/TCGSampleSrvProgramSetting.xml`) with the information on servers and services for integration.
 
```xml
<?xml version="1.0" encoding="utf-8"?>                                   
<root>                                   
    <serverhostinfo id="1"          // Identifier for multi-server setup when the multi-server is configured through ServerID. Currently not used.
        host="0.0.0.0"             // TCP Server Socket host
        port="18282"               // TCP Server Socket port
        keepalivechecktime="1200"  // Effective duration of the connection. Cut the connection if the time is not updated after the designated time. (Unit: seconds)
        sendingcycle="100"         // Time interval of transferring KeepAlive Packet to the client (Unit: seconds)
    />                              
                                     
    <serverwebhostinfo host="0.0.0.0" port="28282"/>   // WebServer host and port to receive LINE Login callback
                                     
    <resourceinfo path="../DataTable"/>                // Resource Table (Excel File) path
                                     
    <mysqlinfo server="10.113.~~~~"                     // SQL DB Host
        uid="admin~~~~~"                               // DB Account
        pwd="admin~~~~"                                // DB Password
        database=""                                    // Name of the database to connect to (if blank, it is set as {processName}_{System.Environment.MachineName})
        sslmode="none"                                 // Default: none
        allow_user_variables="true"                    // Default: true
        database_create="false"                        // Flag whether to recreate the database table. Delete and recreate the existing database and table for true while access the established database for false.
    />                              
                                     
    <lbdinfo uri="https://test-api.blockchain.line.me"    // LINE Blockchain Developers API server URL
        dappid="6fee1e3c-~~~~~~~"                            // Service ID. Created at the LINE Blockchain Developers console.
        apikey="a17401a9-da4~~~~~~"                          // Service API Key. Created at the LINE Blockchain Developers console.
        apisecret="f5e4b764-~~~~~~~"                         // API Secret. Created at the LINE Blockchain Developers console.
        itemtoken_contractid="2d8~~~~"                       // Contract ID of item tokens. Created at the LINE Blockchain Developers console.
        servicetoken_contractid="f93~~~~~"                   // Contract ID of service tokens. Created at the LINE Blockchain Developers console.
        operaddr="tlink1u8~~~~~~~"                           // Address of the service wallet. Created at the LINE Blockchain Developers console.
        secretkey="O+S9xIqi~~~~~~~~"                         // SecretKey of the service wallet. Created at the LINE Blockchain Developers console.
    />                              
                                     
    <ldcinfo uri="https://api.line-beta.me"/>                  // LINE Login URL
</root>                                  
```
 
## Item tokenization
Mage Duel tokenizes in-game assets. Let’s take a look at how various LINE Blockchain tokens are used.
### Ruby
* Primary in-game currency
* Service token
* Players can use Rubies to purchase card packs or Coins at the shop.
 
### Coin
* Secondary in-game currency
* Fungible item token
* Players can use Coins to pay for card sales registration fee and purchase cards at the card exchange.
 
### Hero card
* Character card
* Non-fungible item token
* Primary card to create a deck
 
### Regular card
* Monster card/ equipment card/ magic card
* Non-fungible item token
* Regular cards that can be included in the deck
 
## Game features and blockchain
Here is which blockchain feature is used on each screen of Mage Duel.
### Deck builder
* Attaching or detaching, using composable tokens
* Registering up to 40 regular cards (child token) to the hero card (parent token)
 
### Shop
* Purchasing a card pack (Five random regular cards) with Rubies
  - Transferring Rubies from the user wallet to the service wallet
  - Minting 5 non-fungible item tokens
 
* Purchasing Coins with Rubies     
  - Transferring (or minting) Coins from the service wallet to the user wallet
  - Transferring Rubies from the user wallet to the service wallet 
 
### Card exchange
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

