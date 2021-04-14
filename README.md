# WSChat

## Design Decisions

The chat service is implemented on it's own module that the main program calls to, passing
the Nickname the user wants, and the connection string that the service will use, keeping
all the configuration outside of the core logic module.

The ChatService module serves as the orchestrator that decides if the program will run as
server or as client. It stores common constants and enums common to the whole service,
manages the control-flow deciding if the program should exit based on the initial parameter
that specifies which control command will be used to exit, by default "cheerio", and it
receives the configurable port number via a command line argument.

This program utilizes the MessagePack library to send compact and fast data packages between
all the clients. Part of the reason to use this, is so that we can actually send complex
messages, in this case we send structs with Author, Message and Timestamp, so that we can
show all this metadata during chat, so as to have the minimum functions that are expected.

The ChatService implements the logic to Encode / Decode the messages, and pass around the
actual byte array / Stream that gets sent / received.

The Chat Server and Client components use dependency injection, to get their main chat
service instances.

The Chat Server is implemented using the Fleck library which works for the purposes of
this demo but is pretty limited on it's API, and the project has no documentation. So
I wouldn't recommend it for a real project.

The server runs it's own event loop async, into which we hook our message parsing and
delivering logic, and once the server is connected with run our own loop to get user input
to send through the admin channel.

The Chat Client uses the WebSockets from .NET to connect to the server. Checking for new
messages is handled from the Chat Service itself which runs a separate thread for the
message loop, implementing exiting on the configured command and also through an immediate
exit flag.

I also implemented the very basis of support for Admin message passing so client instances
can pass information meant specific for the Server, for instance user bans, Nickname changes,
etc.

### Functionality Summary

- First instance runs as server
- User chooses Nickname on startup
- Once chat has started user can quit typing "cheerio" followed by return
- Server Admin monitors all messages
- Server Admin can send messages to all users
- Server Admin gets notified on Connection / Disconnection of users
- All users are notified when a new user joins / leaves

### Nice to haves

A more complete implementation would also check uniqueness of the Nickname to avoid duplicates.

Flow:  Client -> Send Nick -> Server Validates -> Responds on Admin Channel with Accepted /
       Rejected, and a status code, those, Admin channel responses would be run through a pattern
       matcher to check the response codes, following the line of how the Joined / Left messages
       work. For error duplicate Nick, we'd either print the error and restart, or redirect the
       program flow to step 1 for the user to add a new Nick.

Another nice to have thing would be to implement on the client's DisplayMessage code a configurable
logger, so that chat histories can be saved / loaded.

And ability to send direct messages to users by @. Which would not be hard to implement based on the
current architecture.

### Testability

This solution includes a testing project that verifies that both server connecting and the client
side connect properly. In a more complete application, we'd also implement tests encapsulating
the server sockets so test the message passing between clients, and the console inputs work.

### Deployability

This project uses .NET 5's ability to output self-contained executables, to provide a
single file binary, that can be deployed stand-alone, and includes all the dependencies.
