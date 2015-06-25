cuddychat
=========
Chat Server &amp; Client

History - implemented Features
=======
- server and client are stable + hard coded smiley parsing of :D
- etter gui xaml code
- menu and statusbar in client and server
- fix smiley parsing (smileys folder with smileys in same folder where Client.exe is
- first security option -> only chat client can be connected to the server
- Show all Smileys Window
- second security option -> encrypted and decrypted messages over socket connection -> length of messages is limited to 80
- show incoming time from message (HH:ii:ss)
- fix insertion of standard smileys in binary
- send and receive messages over binarystream with rsa + rijndael - security level high enough atm
- parsing of hyperlinks in client
- logged in user list in client, atm with rightclick on list with info and msg box with clicked user name, more later
- send user info to server for info of user on right click in user list in client (login time, operating system)
- single instance of server
- private chats
- improved enter behaviour in textareas

- with lots of thanks to http://wpfanimatedgif.codeplex.com/ for animated gifs in client :-)


Roadmap
======
features in near future:
- add smiley window
- file transfer with private chats
- dynamic smiley add and parsing
- single instance of client

features in far away future:
- login window + registration with server
