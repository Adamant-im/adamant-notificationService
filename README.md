# ADAMANT Notification Service (ANS)
**ANS**! The first of upcoming ADAMANT Services, implemented to make your life with ADAMANT more functional and convenient.

ADAMANT Blockchain and Messenger are fully functional without ANS and other Services. The goal of ADAMANT Services and ANS is to provide features that can not be *(or can be, but it's a really bad idea to even try to)* implemented on the Blockchain.

More on [adamant.im](https://adamant.im). *(soon)*

## This is a Pre-release!
Project is in early development, so excpect bugs and bad code.
Right now only iOS pushes implemented, PWA will be the next one.

## Application
There are two main parts:

**ANSRegistrationService** — device sends its unique token to this Service, and the Service saves it into the database.

**ANSPollingWorker** — console application that polls ADAMANT nodes for new transactions and checks for registered devices of receivers. If there is a registered device for the recipient of the transaction — sends a notification.

**ANSSignalsRegistration** - console application that polls ADAMANT nodes for new service signals (transaction with chat asset, ChatType = 3) for device tokens. Message payload must be serialized in JSON and encrypted as other chat transactions.

Payload format:
```json
{
    "token": "DeviceToken",
    "provider": "apns"
}
```
*'apns' for Apple Push Notification service*

## QA
#### Device token? What about security?
You can read about Apple Push Notification service (APNs) and security [here](https://developer.apple.com/library/content/documentation/NetworkingInternet/Conceptual/RemoteNotificationsPG/APNSOverview.html) and google more about it.

In short:
- We do not use third party services to send notifications. Your tokens and addresses do not fly around the Internet.
- It is technically impossible to read a message contents from a transaction. And yes, because of this, it is impossible to send a notification with a message preview. No.
- Your device token is unique for each application on your device. We can't find your facebook page with your device token, generated for the Adamant app.
- New device token generated each time you reinstall an app, or just reenable notifications. You can just disable notifications for ADAMANT app, and the device token in ANS database becomes useless. Next time ANS will try to send a push notification, Apple will tell us that the token is broken. That's all.
- We do have plans to implement 'auto-renew-token' feature on client-side. Later.
- Device tokens database will not be published. It's a "classic" centralised service, with open-source codebase and hidden production database. If you don't like this idea — you can use ADAMANT without "real" pushes, it's up to you.

#### iOS App Badge?
In iOS, app's badge number is sent to you by a server as a part of a push notification, it's not handled by an application, as application can be even terminated and unloaded from memory at the moment. Adamant, and ANS in particular, does not know how many messages you haven't read. At the moment, it is impossible to show a real number on the app's badge. Workaround — just show '1'. Symbols like '\*' not supported by iOS, only integers.

We have plans and ideas how to implement this, stay tuned.

## Installation
Want to try it out? There is no windows or buttons, just console, so it's boring, only if you wanna bite some C#.
1. You gonna need a dotnet.core runtime to launch ANS. Go to [Microsoft.com](https://www.microsoft.com/net/learn/get-started) and download SDK for your platform.
2. Clone or download this repository.
3. Open terminal/console/cmd and type `dotnet restore` in solution's folder, or just open solution in [Visual Studio](https://www.visualstudio.com) (there is one for macOS now). VS will automatically restore NuGet dependencies.
4. Grab sample config file at solution's root, edit your connection strings, nodes, delays, certificates, and save it at {HomeDirectory}/.ans/config.json.
5. At first launch, applications will auto-upgrade your database.
6. To launch **ANSPollingWorker**, you need your Apple Push certificate, you can grab it from Apple Developer's center. Place it in {UserHomeDirectory}/.ans/, make sure you specified correct path and certificate's password in config. Go to terminal, `cd ANSPollingWorker`, `dotnet run`.
7. To launch **ANSRegistrationService**, type in your ANS account in config. Go to terminal, `cd ANSRegistrationService`, `dotnet run`.

## My own iOS app and ANS server
If you are building your own iOS ADAMANT application and want to use your own ANS server, you will need to:
1. Register ADAMANT account for ANS. Just a regular 'U' account.
2. In iOS source code, type your ANS account's address and public key in AdamantResources struct. It located in AppDelegate.swift
3. In ANS config, type in your ANS account's address and private key. See **Configuration** section bellow for more info.
4. Get your Apple Push certificate, convert it in '*.cert' type using Keychain, and place it in {UserHomeDirectory}/.ans/. Type in config cert name and password.
5. Done. iOS application will send device tokens to your ANS account, **ANSRegistrationService** will poll signals for your ANS account and register tokens, and **ANSPollingService** will poll new messages and transactions and notify registered devices.

You will need a certificate to send a push notifications to APNs, which you can get from your Apple Developer account.


## Configuration
Sample file is located in Solution root directory. Configuration file loaded from ~/.ans/config.json.

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
    + Warmup (bool, optional, default: true): Start with a warmup. If this set to true, Worker will try to fetch latest block height from ADAMANT, and then begins the loop with this height. If param set to false or warmup failed, Worker will start processing from 0, do not try this on live network.
    + Delay (milliseconds as int, optional, default: 2000): delay between two requests.
    + NlogConfig (string, optional, default: 'nlog.config'): path to NLog configuration file.

- SignalsRegistration: Signals polling & registration settings. Properties:
    + Warmup (bool, optional, default: true): Start with a warmup. If this set to true, Worker will try to fetch latest block height from ADAMANT, and then begins the loop with this height. If param set to false or warmup failed, Worker will start processing from 0, do not try this on live network.
    + Delay (milliseconds as int, optional, default: 2000): delay between two requests.
    + NlogConfig (string, optional, default: 'nlog.config'): path to NLog configuration file.
    + Address (string, required): ANS account address to poll signals.
    + PrivateKey (string, required): ANS account private key to decode signals.

- ApplePusher: APNS settings. Sections:
    + Certificate. Properties:
        * path (string): Your Apple push certificate path.
        * pass (string, optional): passphrase for certificate.
    + Payload[]. Apple push notifications payload. Properties:
        * transactionType:
            * 0: transfer
            * 8: chat message
        * title
        * body
        * sound

