# ADAMANT Notification Service (ANS)

ANS delivers Apple Push Notifications for the ADAMANT iOS app without ANS or Apple ever learning who is messaging whom. It was the first of the [ADAMANT Services](https://news.adamant.im/adamant-is-working-on-blockchain-messaging-platform-and-push-notifications-service-765972cce50e).

> **Status:** this service is legacy and is planned to be succeeded by [adamant-ns](https://github.com/Adamant-im/adamant-ns). Until that migration is complete, ANS keeps running in production, and this repository receives targeted maintenance rather than new features.

ADAMANT Blockchain and Messenger apps are fully functional without ANS and other Services — the goal of ADAMANT Services is to provide features that cannot be implemented on the blockchain itself. More on [adamant.im](https://adamant.im).

## How it works

Delivering a notification **privately and securely** involves 4 parties:

1. The user's device (e.g. an iPhone)
2. ADAMANT's blockchain
3. Apple Push Notification Service (APNs)
4. This application, ADAMANT Notification Service (ANS)

The workflow:

- The user's device sends an encrypted signal message with a unique device token to an ADAMANT blockchain node. The recipient is ANS's own ADM address. See [AIP-6: Signal Messages](https://aips.adamant.im/AIPS/aip-6).
- ANS polls the blockchain and decrypts the device token.
- ANS polls the blockchain and filters transactions where a registered device's address is the recipient. For each match, ANS asks APNs to deliver a notification to that device's token — the notification carries only a transaction id, never message contents.
- APNs notifies the user's device.
- The device holds the secret key needed to fetch and decrypt the actual message, entirely locally.

The user's device never communicates with ANS directly, and ANS never learns the device's IP or other network identity — all communication happens through blockchain nodes.

## Components

This repository builds two independent console workers plus a set of shared libraries:

- **ANSSignalsRegistration** — polls ADAMANT blockchain nodes for new service signal transactions and registers/unregisters device tokens. Signal payloads are JSON, encrypted the same way as any other chat transaction:

  ```json
  {
      "token": "DeviceToken",
      "provider": "apns",
      "action": "add"
  }
  ```

  - `token`: the device's push token
  - `provider`: push service provider — `apns` for release builds, `apns-sandbox` for debug builds (not yet supported)
  - `action` (optional): `add` (default) registers a device, `remove` unregisters it

- **ANSPollingWorker** — polls ADAMANT nodes for new transactions, checks whether the recipient has a registered device, and sends a push notification through APNs when it does.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download) (LTS) to build and run
- Ubuntu 22.04 LTS or newer (any platform .NET 8 supports works; Ubuntu 22.04+ is what production targets)
- MySQL or SQLite for the device-token database

## Installation

```sh
git clone https://github.com/Adamant-im/adamant-notificationService.git
cd adamant-notificationService
dotnet restore
```

1. Copy the sample `config.json` from the repository root, edit connection strings, nodes, delays, and certificate paths, and save it to `{UserHomeDirectory}/.ans/config.json`. See [Configuration](#configuration).
2. On first launch, each worker automatically applies pending database migrations.
3. To run **ANSPollingWorker**, you need an Apple Push Authentication Key from your Apple Developer account (see [Configuration](#configuration) for `ApplePusher:Keys`):

   ```sh
   cd ANSPollingWorker
   dotnet run
   ```

4. To run **ANSSignalsRegistration**, set your ANS account's address and private key in the config, then:

   ```sh
   cd ANSSignalsRegistration
   dotnet run
   ```

5. For a deployable build:

   ```sh
   dotnet publish -c Release -r linux-x64 --self-contained -o {output path}
   ```

   See Microsoft's docs on [runtime identifiers](https://learn.microsoft.com/dotnet/core/rid-catalog) and [`dotnet publish`](https://learn.microsoft.com/dotnet/core/tools/dotnet-publish) for other targets.

## Running your own ANS server

To point your own iOS ADAMANT build at your own ANS server:

1. Register a regular ADAMANT (`U`) account for ANS.
2. In the iOS app's source, set your ANS account's address and public key in the `AdamantResources` struct.
3. In the ANS config, set your ANS account's address and private key. See [Configuration](#configuration).
4. Create a `.pfx` certificate with an ECDSA private key from your [Apple Developer Auth Key](https://developer.apple.com/account/ios/authkey/):

   ```sh
   openssl req -new -x509 -key key.p8 -out selfsigned.cer
   openssl pkcs12 -export -in selfsigned.cer -inkey key.p8 -out cert.pfx
   ```

   Put the resulting `.pfx` in `~/.ans` and reference it from the config.
5. Done — the iOS app sends device tokens to your ANS account, **ANSSignalsRegistration** polls signal transactions and registers them, and **ANSPollingWorker** polls new transactions and notifies registered devices.

## Configuration

The sample configuration file lives at the repository root. Both workers load their config from `~/.ans/config.json`, so a single file covers both.

### Config sections

- `Database` (optional): database configuration.
  - `ConnectionString` (optional, default: `devices`): the name of the connection string to use, defined in `ConnectionStrings` below
  - `Provider` (optional, default: `mysql`): `sqlite` or `mysql`

- `ConnectionStrings`: standard .NET connection string section. The active entry is named by `Database:ConnectionString` (default `devices`).

- `Api`: ADAMANT node settings.
  - `Server[]`: node addresses
    - `ip` (string): node hostname or IP
    - `protocol` (string, optional, default: `https`)
    - `port` (int, optional)

- `PollingWorker`: polling settings for ANSPollingWorker.
  - `Delay` (int, milliseconds, optional, default: `2000`): interval between polling requests
  - `NlogConfig` (string, optional, default: `nlog.config`): path to the NLog configuration file
  - `Startup` (enum, optional, default: `database`): startup mode
    - `database`: resume from the last height stored in the database; falls back to `network` mode if none is stored
    - `network`: fetch the blockchain's current height and start from there
    - `initial`: start from height 0

- `SignalsRegistration`: settings for ANSSignalsRegistration.
  - `Delay` (int, milliseconds, optional, default: `2000`): interval between polling requests
  - `NlogConfig` (string, optional, default: `nlog.config`): path to the NLog configuration file
  - `Address` (string, required): ANS's own ADM account address, used to poll signal transactions
  - `PrivateKey` (string, required): ANS's own ADM account private key, used to decrypt signal transactions
  - `Startup` (enum, optional, default: `database`): same options as `PollingWorker:Startup`

- `ApplePusher`: APNs settings.
  - `Keys` — the token-based auth path actually used at runtime:
    - `keyId` (string): your developer key id, from the [Auth Keys page](https://developer.apple.com/account/ios/authkey/)
    - `teamId` (string): your Apple team id, from [Membership Details](https://developer.apple.com/account/#/membership/)
    - `bundleAppId` (string): your application's bundle id
    - `pfxPath` (string): path to a self-signed `.pfx` certificate containing an ECDSA private key
    - `pfxPassword` (string): the certificate's password
  - `Certificate` — legacy `.p12`-based path; present for historical reasons only, not read by the current build:
    - `path` (string): path to an APNs `.p12` certificate
    - `pass` (string): the certificate's password
  - `Payload[]`: push notification content, keyed by transaction type
    - `transactionType`: `0` for an ADM token transfer, `8` for chat transactions and coin transfers
    - `title`, `body`, `sound`

## Security and privacy

- ANS does not use third-party services to deliver notifications; tokens and addresses never leave ADAMANT infrastructure and Apple's own servers.
- ANS cannot read a transaction's message contents and cannot include decrypted content in a push payload — decryption requires a secret key that only the user's device holds.
- Since version 0.4, pushes carry only a `txn-id`. The client app fetches the transaction from a blockchain node and decrypts it on-device, using a passphrase stored in the [Keychain](https://developer.apple.com/documentation/security/keychain_services) and handled by a [`NotificationServiceExtension`](https://developer.apple.com/documentation/usernotifications/unnotificationserviceextension).
- Device tokens are unique per app install; they cannot be used to identify a user elsewhere. A new token is generated on every reinstall or notification re-enable, and a stale token in ANS's database simply stops working the next time APNs rejects it.
- The push badge is always `1` when there's any unread message, never an exact unread count — iOS badges are set by the server, and ANS deliberately doesn't track how many messages a device hasn't read.

For background on APNs itself, see [Apple's docs](https://developer.apple.com/library/content/documentation/NetworkingInternet/Conceptual/RemoteNotificationsPG/APNSOverview.html).

## Development

```sh
dotnet restore
dotnet build
dotnet test
```

## Links

- [adamant-ns — the planned successor to this service](https://github.com/Adamant-im/adamant-ns)
- [ADAMANT website](https://adamant.im/)
- [ADAMANT documentation](https://docs.adamant.im/)
- [ADAMANT Improvement Proposals](https://aips.adamant.im/all)
- [ADAMANT Node](https://github.com/Adamant-im/adamant)
- [ADAMANT iOS app](https://github.com/Adamant-im/adamant-iOS)
- [ADAMANT API schema](https://schema.adamant.im/)
- [ADAMANT schema source](https://github.com/Adamant-im/adamant-schema)
- [ADAMANT blockchain explorer](https://explorer.adamant.im/)
- [Issues and feature requests](https://github.com/Adamant-im/adamant-notificationService/issues)

## License

[GPL-3.0](./LICENSE)
