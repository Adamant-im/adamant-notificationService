# ADAMANT Notification Service (ANS)

The first of [ADAMANT Services](https://medium.com/adamant-im/adamant-is-working-on-blockchain-messaging-platform-and-push-notifications-service-765972cce50e), implemented to make secure instant notifications for ADAMANT iOS application.

Note: ADAMANT Blockchain and Messenger apps are fully functional without ANS and other Services. The goal of ADAMANT Services and ANS is to provide features that can not be implemented on the Blockchain. More on [adamant.im](https://adamant.im).

## How it works

To deliver notification **privately and secure**, 4 parties are involved:

1. User's device (i. e., iPhone)
2. ADAMANT's blockchain
3. Apple Push Notification Service (APNS)
4. This application, ADAMANT Notification Service (ANS)

A workflow runs as:

- User sends an encrypted signal message with a unique token to an ADAMANT blockchain node. Recipient is ANS's ADAMANT address. See [AIP-6: Signal Messages](https://aips.adamant.im/AIPS/aip-6).
- ANS polls the blockchain and decrypts the user's token
- ANS polls the blockchain and filter transactions, where user's ADM address is the recipient. ANS asks APNS to deliver these transactions (they holds encrypted messages) to a user's device, specified by unique token.
- APNS notifies a user's device
- User's device has a secret key and decrypts messages

This way a user's device never communicate with ANS, and ANS don't know its IP or other identities. They communicate through a blockchain nodes.

## ANS application

This application, ANS, consists of two main parts:

**ANSSignalsRegistration** — console application that polls ADAMANT blockchain nodes for new service signals to get device tokens. Message payload must be serialized in JSON and encrypted as other chat transactions.

Payload format:

```json
{
    "token": "DeviceToken",
    "provider": "apns",
    "action": "add"
}
```

- `token`: user's device token
- `provider`: push service provider
  - apns: for release builds
  - apns-sandbox: for debug builds (not yet supported)
- `action` (optional): signal action
  - add (default): register new devise
  - remove: unregister device

**ANSPollingWorker** — console application that polls ADAMANT nodes for new transactions and checks for registered devices of receivers. If there is a registered device for the recipient of the transaction—sends a notification.

## QA

### Device token? What about security?

You can read about Apple Push Notification Service (APNS) and security on [Apple's docs](https://developer.apple.com/library/content/documentation/NetworkingInternet/Conceptual/RemoteNotificationsPG/APNSOverview.html).

In short:

- We do not use third party services to send notifications. Your tokens and addresses doesn't fly around the Internet.
- It's technically impossible to read a message contents from a transaction for ANS and it is impossible to include decrypted message into push contents. To decrypt a message, secret key is needed. And only user device holds it.
- Starting from version 0.4, ANS includes `txn-id` param in the push content with the transaction id. A client app can get the transaction from a blockchain node and decrypt the message on the device, using a locally stored secret key. This is handled by [NotificationServiceExtension](https://developer.apple.com/documentation/usernotifications/unnotificationserviceextension), and a passphrase (secret key) is stored securely in a [Keychain](https://developer.apple.com/documentation/security/keychain_services).
- Your device token is unique for each application on your device. We can't find your facebook page with your device token, generated for the ADAMANT app.
- New device token is generated each time you reinstall an app, or just re-enable notifications. You can just disable notifications for ADAMANT app, and the device token in ANS database becomes useless. Next time ANS will try to send a push notification, Apple will say the token is broken.

### iOS App Badge?

In iOS, app's badge number is sent to you by a server as a part of a push notification, it's not handled by an application, as application can be even terminated and unloaded from memory at the moment. ANS doesn't know how many messages you haven't read. That's why ANS shows `1` badge, if there any unread messages. Alternative solution is to handle it locally on a device by the NotificationServiceExtension—an app extension, that can modify notification content.

## Installation

### .NET Core version

APNS requires HTTP/2 connection. dotnet core 2.1 and 2.2 **does not support it**.
The app is build with is 3.0.100-preview5-011568, and it supports HTTP/2. You can create a self-contained build for a machine without 3.0 runtime on a machine with 3.0 SDK.

`dotnet publish -c Release -r linux-64 -o {output path} -f netcoreapp3.0`

[more](https://docs.microsoft.com/ru-ru/dotnet/core/rid-catalog) about -r, [more](https://docs.microsoft.com/ru-ru/dotnet/standard/frameworks) about -f, [download](https://dotnet.microsoft.com/download) dotnet core SDK.

### Want to try it out?

1. You gonna need a dotnet.core runtime to launch ANS. Go to [Microsoft.com](https://www.microsoft.com/net/learn/get-started) and download SDK for your platform.
2. Clone or download this repository
3. Open terminal/console/cmd and type `dotnet restore` in the Solution's folder, or just open the Solution in [Visual Studio](https://www.visualstudio.com). VS will automatically restore NuGet dependencies.
4. Grab sample config file at Solution's root, edit your connection strings, nodes, delays, certificates, and save it to `{UserHomeDirectory}/.ans/config.json`. See [Configuration](#configuration).
5. At first launch, the application will auto-upgrade your database.
6. To launch **ANSPollingWorker**, you need your Apple Push certificate, you can grab it from Apple Developer's center. Place it in `{UserHomeDirectory}/.ans/`, make sure you've specified correct path and certificate's password in the config. Go to terminal, `cd ANSPollingWorker`, `dotnet run`.
7. To launch **ANSRegistrationService**, type in your ANS account in the config. Go to terminal, `cd ANSRegistrationService`, `dotnet run`.
8. You can run `dotnet publish -c Release` to create compiled archives. More about dotnet core, and what to do with this 'compiled archives' you can read on [Microsoft.com](https://docs.microsoft.com/ru-ru/dotnet/core/tools/dotnet-publish).

*You will need a certificate to send a push notifications to APNS, which you can get from your Apple Developer account.*

## My own iOS app and ANS server

If you are building your own iOS ADAMANT application and want to use your own ANS server, you will need to:

1. Register ADAMANT account for ANS. Just a regular 'U' account.
2. In iOS source code, type your ANS account's address and public key in `AdamantResources` struct.
3. In ANS config, type in your ANS's ADM account address and private key. See [Configuration](#configuration).
4. To create .pfx certificate with ECDSA private key. First, create a key and download it from your [Apple Developer page](https://developer.apple.com/account/ios/authkey/). Put it in some folder. Open Terminal, navigate to this folder, and type:

```bash
openssl req -new -x509 -key key.p8 -out selfsigned.cer
openssl pkcs12 -export -in selfsigned.cer -inkey key.p8 -out cert.pfx
```

Put the .pfx certificate in `~/.ans`, and update the config.

5. Done. iOS application will send device tokens to your ANS ADM account, **ANSRegistrationService** will poll signal transactions and register tokens, and **ANSPollingService** will poll new messages/transactions and notify registered devices.

## Configuration

Sample configuration file is located in the Solution's root directory. Both Polling ans Signal registration services loads config from `~/.ans/config.json`, so you have one config file for ANS.

### Config sections

- `Database` (optional): Section for database configuration. Params:
  - `ConnectionString` (optional, default: `devices`). ConnectionString name. Strings is specified in `ConnectionStrings` section, see bellow.
  - `Provider` (optional): Database connection provider. Two providers are supported:
    - `sqlite`
    - `mysql` (default)

- `ConnectionStrings`: a standard dotnet section for connection strings. Active connection string name specified in `Database:ConnectionString` param, default is `devices`.

- `Api`: ADAMANT node settings.
  - `Server[]`: node addresses. Properties:
    - `ip` (string): node address (or ip)
    - `protocol` (string, optional, default: `https`)
    - `port` (int, optional)

- `PollingWorker`: Polling settings. Properties:
  - `Delay` (milliseconds as int, optional, default: `2000`): interval between two requests to retrieve new messages
  - `NlogConfig` (string, optional, default: `nlog.config`): path to NLog configuration file
  - `Startup` (enum, optional, default: `database`): Startup mode. Options:
    - `database`: Try to load saved last blockchain height from database, and start from this value. If failed or no value saved, switch to `network` mode.
    - `network`: Try to get last transaction from network and use its height as last height value. If failed or no transaction received, go to `initial` mode.
    - `initial`: Start from blockchain height 0.

- `SignalsRegistration`: Signals polling & registration settings. Properties:
  - `Delay` (milliseconds as int, optional, default: 2000): interval between two requests to retrieve new signal transactions
  - `NlogConfig` (string, optional, default: `nlog.config`): path to NLog configuration file
  - `Address` (string, required): ANS's ADM account address to poll signals
  - `PrivateKey` (string, required): ANS's ADM account private key to decrypt signal transactions
  - `Startup` (enum, optional, default: `database`): Startup mode. Same options as for `PollingWorker:Startup`.

- `ApplePusher`: APNS settings. Sections:
  - `Keys`. Properties:
    - `keyId` (string): Your developer key id. Created and obtained at your [Auth Keys page](https://developer.apple.com/account/ios/authkey/).
    - `teamId` (string): Your app developer team id. Obtained at your Apple Dev [Membership Details](developer.apple.com/account/#/membership/).
    - `bundleAppId` (string): Your application bundle id
    - `pfxPath` (string): Path to self-signed *.pfx certificate. Certificate must contain ECDSA private key
    - `pfxPassword` (string): Certificate's password
  - `Certificate`. Properties:
    - `path` (string): Path to APNS *.p12 certificate
    - `pass` (string): Certificate's password
  - `Payload[]`. Apple push notifications payload. Properties:
    - `transactionType`: `0` for ADM token transfer, `8` for chat transactions and coin transfers
    - `title`
    - `body`
    - `sound`
