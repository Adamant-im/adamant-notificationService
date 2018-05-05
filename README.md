# Adamant Notification Service
**ANS**! First of upcoming Adamant Services, created to make your life with Adamant easier.

Adamant Blockchain and Messenger are fully functional without ANS and other Services. The goal of Adamant Services and ANS is to provide features that can not be *(or can be, but it's a really bad idea to even try to)* implemented on a blockchain.

More on [adamant.im](https://adamant.im). *(soon)*

## This is a Pre-release!
Project in early development, so excpect bugs and bad code.
Right now only iOS pushes implemented, PWA is next.

## Application
There are two main parts:

**ANSRegistrationService** - devices send their tokens to this service, and service put them into db.

**ANSPollingWorker** - console application that polls Adamant nodes for new transactions and checks for registered devices of receivers. If there is a registered device for the transaction recipient - send notification.

## QA
#### Device token?! Are you insane!? My security!!
You can read about Apple Push Notification service (APNs) and security [here](https://developer.apple.com/library/content/documentation/NetworkingInternet/Conceptual/RemoteNotificationsPG/APNSOverview.html) and google more about it.

In short:
- We do not use third party services to send notifications. Your tokens and addresses does not fly around the Internet.
- It is technically impossible to read an actual message from transaction. And yes - because of this, it is impossible to send a notification with message preview. No.
- Your device token is unique for each application on your device. We can't magically find your facebook page with your device token.
- New device token generated each time you reinstall the app, or just reenable notifications. Yes, you can just disable notifications, and device token in our database becomes useless. Next time we try to send a push, Apple will tell us that the token is broken. That's all.
- We do have plans to implement 'auto-renew-token' feature on client-side. Later.
- Device tokens database will not be published. It's a "classic" centralised service, with open-source codebase and hidden production database. If you don't like this idea - you can use Adamant without "real" pushes, it's up to you.
- We *may* change this later, if we find a better solution.

#### iOS App Badge?
Yeah, nice iOS feature to show amount of unread messages. In iOS, this number sended to you by a server, it's not handled by an application, application can be even terminated and unloaded from memory at the moment. We do not know how many messages you haven't read. So right now, it is impossible to show a real number on a badge. Workaround - just show '1'. Symbols like '\*' not supported by iOS, only integers.

We have plans and ideas how to implement this, stay tuned.

## Installation
Want to try it out? There is no windows or buttons, just console, so it's boring, only if you wan't to bite some C#.
1. You gonna need a dotnet.core runtime to launch ANS. Go to [Microsoft.com](https://www.microsoft.com/net/learn/get-started) and download SDK for your platform.
2. Clone or download this repository.
3. Open terminal/console/cmd and type `dotnet restore` in solution's folder, or just open solution in [Visual Studio](https://www.visualstudio.com) (there is one for macOS now).
4. Later i will add here a step for initializing a database.
5. Launch ANSRegistrationService and ANSPollingWorker.
