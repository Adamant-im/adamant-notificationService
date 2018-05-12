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

## QA
#### Device token?! Are you insane!? My security!!
You can read about Apple Push Notification service (APNs) and security [here](https://developer.apple.com/library/content/documentation/NetworkingInternet/Conceptual/RemoteNotificationsPG/APNSOverview.html) and google more about it.

In short:
- We do not use third party services to send notifications. Your tokens and addresses do not fly around the Internet.
- It is technically impossible to read a message contents from a transaction. And yes, because of this, it is impossible to send a notification with a message preview. No.
- Your device token is unique for each application on your device. We can't magically find your facebook page with your device token, generated for the Adamant app.
- New device token generated each time you reinstall an app, or just reenable notifications. Yes, you can just disable notifications, and a device token in Service's database becomes useless. Next time Service will try to send a push notification, Apple will tell that the token is broken. That's all.
- We do have plans to implement 'auto-renew-token' feature on client-side. Later.
- Device tokens database will not be published. It's a "classic" centralised service, with open-source codebase and hidden production database. If you don't like this idea — you can use ADAMANT without "real" pushes, it's up to you. 
- We *may* change this later, if we find a better solution.

#### iOS App Badge?
In iOS, app's badge number is sent to you by a server as a part of a push notification, it's not handled by an application, as application can be even terminated and unloaded from memory at the moment. Adamant, and ANS in particular, does not know how many messages you haven't read. At the moment, it is impossible to show a real number on the app's badge. Workaround — just show '1'. Symbols like '\*' not supported by iOS, only integers.

We have plans and ideas how to implement this, stay tuned.

## Installation
Want to try it out? There is no windows or buttons, just console, so it's boring, only if you wanna bite some C#.
1. You gonna need a dotnet.core runtime to launch ANS. Go to [Microsoft.com](https://www.microsoft.com/net/learn/get-started) and download SDK for your platform.
2. Clone or download this repository.
3. Open terminal/console/cmd and type `dotnet restore` in solution's folder, or just open solution in [Visual Studio](https://www.visualstudio.com) (there is one for macOS now).
4. There are 3 appsettings.json: **ANSPollingWorker**, **ANSRegistrationService**, ans **ANSDataContext**. Open them all and edit ConnectionString.
4. Go back to the Terminal, `cd ANSDataContext`, `dotnet ef database update`. This will update your database.
5. To launch **ANSPollingWorker**, you will need to edit it's appsettings.json: **Api** section for your blockchain nodes, and **ApplePusher** section for your certifiate. Then `cd ANSPollingWorker` and `dotnet run`.
6. To launch **ANSRegistrationService**, configure your **HttpServer:Endpoints** section in it's appsettings.json. Then `cd ANSRegistrationService` and `dotnet run`.

You will need a certificate to send a push notifications to APNs, which you can get from your Apple Developer account.
