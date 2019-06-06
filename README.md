# ADAMANT Notification Service (ANS)
The first of [ADAMANT Services](https://medium.com/adamant-im/adamant-is-working-on-blockchain-messaging-platform-and-push-notifications-service-765972cce50e), implemented to make secure instant notifications for ADAMANT applications.

Note: ADAMANT Blockchain and Messenger apps are fully functional without ANS and other Services. The goal of ADAMANT Services and ANS is to provide features that can not be implemented on the Blockchain. More on [adamant.im](https://adamant.im).

## Application
There are two main parts:

**ANSPollingWorker** — console application that polls ADAMANT nodes for new transactions and checks for registered devices of receivers. If there is a registered device for the recipient of the transaction — sends a notification.

**ANSSignalsRegistration** — console application that polls ADAMANT nodes for new service signals (transaction with chat asset, ChatType = 3, see [AIP-6: Signal Messages](https://aips.adamant.im/AIPS/aip-6)) for device tokens. Message payload must be serialized in JSON and encrypted as other chat transactions.

Payload format:
```json
{
    "token": "DeviceToken",
    "provider": "apns",
    "action": "add"
}
```
- token: Your device token
- provider: Your push provider.
    + apns: Release builds
    + apns-sandbox: Debug builds. (not yet supported).
- action (optional): Signal action
    + add (default): register new devise
    + remove: unregister device

*'apns' stands for Apple Push Notification service*.

## QA
### Device token? What about security?
You can read about Apple Push Notification service (APNs) and security [here](https://developer.apple.com/library/content/documentation/NetworkingInternet/Conceptual/RemoteNotificationsPG/APNSOverview.html) and google more about it.

In short:
- We do not use third party services to send notifications. Your tokens and addresses doesn't fly around the Internet.
- It's technically impossible to read a message contents from a transaction for ANS and impossible to include message into push content.
- Starting from version 0.4, ANS will include 'txn-id' param in the push content with the transaction id. A client can get the transaction from a node and decode the message locally on the device, using a locally stored private key or passphrase. On iOS this is handled by [NotificationServiceExtension](https://developer.apple.com/documentation/usernotifications/unnotificationserviceextension), passphrase stored securely in [Keychain](https://developer.apple.com/documentation/security/keychain_services).
- Your device token is unique for each application on your device. We can't find your facebook page with your device token, generated for the ADAMANT app.
- New device token generated each time you reinstall an app, or just reenable notifications. You can just disable notifications for ADAMANT app, and the device token in ANS database becomes useless. Next time ANS will try to send a push notification, Apple will tell us that the token is broken.

### iOS App Badge?
In iOS, app's badge number is sent to you by a server as a part of a push notification, it's not handled by an application, as application can be even terminated and unloaded from memory at the moment. ANS doesn't know how many messages you haven't read. Alternative solution - handle it locally on a device by the NotificationServiceExtension - an app extension, that can modify notification content.

## Installation
### .NET Core version alert!
APNs requires HTTP/2 connection. dotnet core 2.1 and 2.2 **does not support it**. Version 2.0 supports it *on some operation systems*. HTTP/2 planned in .NET Core 3.0. The newest version at the moment is 3.0.100-preview5-011568, and it supports HTTP/2, so go for the preview releases. You can create a self-conteined build for a machine without 3.0 runtime on a machine with 3.0 SDK.
`dotnet publish -c Release -r linux-64 -o {output path} -f netcoreapp3.0`

[more](https://docs.microsoft.com/ru-ru/dotnet/core/rid-catalog) about -r, [more](https://docs.microsoft.com/ru-ru/dotnet/standard/frameworks) about -f, [download](https://dotnet.microsoft.com/download/dotnet-core/3.0) dotnet core 3.0 SDK.

Want to try it out?
1. You gonna need a dotnet.core runtime to launch ANS. Go to [Microsoft.com](https://www.microsoft.com/net/learn/get-started) and download SDK for your platform.
2. Clone or download this repository.
3. Open terminal/console/cmd and type `dotnet restore` in solution's folder, or just open solution in [Visual Studio](https://www.visualstudio.com) (there is one for macOS now). VS will automatically restore NuGet dependencies.
4. Grab sample config file at solution's root, edit your connection strings, nodes, delays, certificates, and save it at {UserHomeDirectory}/.ans/config.json.
5. At first launch, applications will auto-upgrade your database.
6. To launch **ANSPollingWorker**, you need your Apple Push certificate, you can grab it from Apple Developer's center. Place it in {UserHomeDirectory}/.ans/, make sure you specified correct path and certificate's password in config. Go to terminal, `cd ANSPollingWorker`, `dotnet run`.
7. To launch **ANSRegistrationService**, type in your ANS account in config. Go to terminal, `cd ANSRegistrationService`, `dotnet run`.
8. You can run `dotnet publish -c Release` to create compiled archives. More about dotnet core, and what to do with this 'compiled archives' you can read on [Microsoft.com](https://docs.microsoft.com/ru-ru/dotnet/core/tools/dotnet-publish).

*You will need a certificate to send a push notifications to APNs, which you can get from your Apple Developer account.*

## My own iOS app and ANS server
If you are building your own iOS ADAMANT application and want to use your own ANS server, you will need to:
1. Register ADAMANT account for ANS. Just a regular 'U' account.
2. In iOS source code, type your ANS account's address and public key in AdamantResources struct.
3. In ANS config, type in your ANS account's address and private key. See **Configuration** section bellow for more info.
4. To create pfx certificate with ECDsa private key, first, create a key and download it from your [Apple Developer page](https://developer.apple.com/account/ios/authkey/). Put it in some folder. Open Terminal, navigate to this folder, and type:
```bash
$ openssl req -new -x509 -key key.p8 -out selfsigned.cer
$ openssl pkcs12 -export -in selfsigned.cer -inkey key.p8  -out cert.pfx
```
Put pfx certificate in ~/.ans, and update config.

5. Done. iOS application will send device tokens to your ANS account, **ANSRegistrationService** will poll signals for your ANS account and register tokens, and **ANSPollingService** will poll new messages and transactions and notify registered devices.

## Configuration
Sample configuration file is located in Solution root directory. Booth Polling ans Signal registration services loads config from ~/.ans/config.json, so you can have one file for ANS.

#### Sections:
- Database (optional): Section for database configuration. Params:
    + ConnectionString (optional, default: devices). ConnectionString name. Strings specified in 'ConnectionStrings' section, see bellow.
    + Provider (optional): Database connection provider. Two providers supported:
        * sqlite
        * mysql (default)

- ConnectionStrings: standart dotnet section for connection strings. Active connection string name specified in Database:ConnectionString param, default is 'devices'.

- Api: ADAMANT node settings.
    + Server[]: node addresses. Properties:
        * ip (string): node address (or ip)
        * protocol (string, optional, default: https)
        * port (int, optional)

- PollingWroker: Polling settings. Properties:
    + Delay (milliseconds as int, optional, default: 2000): delay between two requests.
    + NlogConfig (string, optional, default: 'nlog.config'): path to NLog configuration file.
    + Startup (enum, optional, default: database): Startup mode. Options:
        * database: Try to load saved last height from database, and begin from this value. If failed or no value saved, go to 'network' mode.
        * network: Try to get last transaction from network and use it height as last height value. If failed or no transaction received, go to 'initial' mode.
        * initial: Start from height 0.

- SignalsRegistration: Signals polling & registration settings. Properties:
    + Delay (milliseconds as int, optional, default: 2000): delay between two requests.
    + NlogConfig (string, optional, default: 'nlog.config'): path to NLog configuration file.
    + Address (string, required): ANS account address to poll signals.
    + PrivateKey (string, required): ANS account private key to decode signals.
    + Startup (enum, optional, default: database): Startup mode. Options:
        * database: Try to load saved last height from database, and begin from this value. If failed or no value saved, go to 'network' mode.
        * network: Try to get last transaction from network and use it height as last height value. If failed or no transaction received, go to 'initial' mode.
        * initial: Start from height 0.

- ApplePusher: APNS settings. Sections:
    + Keys. Properties:
        * keyId (string): Your delevoper key id. Created and obtained at your [Auth Keys page](https://developer.apple.com/account/ios/authkey/).
        * teamId (string): Your app developer team id. Obtained at your Apple Dev [Membership Details](developer.apple.com/account/#/membership/).
        * bundleAppId (string): Your application bundle id.
        * pfxPath (string): Path to self-signed *.pfx certificate. Certificate must contain ECDsa private key.
        * pfxPassword (string): Certificate's password.
    + Payload[]. Apple push notifications payload. Properties:
        * transactionType:
            * 0: transfer
            * 8: chat message
        * title
        * body
        * sound

