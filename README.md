# FehBot - FeedHenry Chat Bot  

This is a chat bot written in C# based on aerobot written by qmx.  

# Prerequisites  

* Mono 2.5 (ish)  
* Default configuration of MongoDB
* ???

# Usage

## System Setup
* Set the environment variables FEHBOT_IRC_SERVER, FEHBOT_USERNAME, FEHBOT_NICKNAME, FEHBOT_CHANNELS, and FEHBOT_NICKSERV_PASSWORD as appropriate.  FeHBot uses sane defaults.  FEHBOT_CHANNELS is a comma separated list.

## Available IRC Commands

### Factiods
#### Setting a Factoid
    ${FEHBOT_USERNAME}, ${factiod} is ${definition}
*ex*  
 
    fehbot, coding is https://media.giphy.com/media/l41m3vNpNTVHzp0di/giphy.gif  

#### Fetching a Factoid
    ?${factoid}
*ex*

    ?coding

#### Deleting a Factoid
    ${FEHBOT_USERNAME}, forget ${factoid}
*ex*
  
    fehbot, forget coding
### Karma
#### Add Karma
    ${recipient}++
*ex*
  
    summersp++
#### Remove Karma
    ${recipient}--
*ex*
  
    summersp--
### Online Messages
#### Sending a message to be delivered when someone comes back
    ${FEHBOT_USERNAME} tell ${recipient} ${message}
*ex*

    fehbot, tell summersp FehBot is awesome!

### Account Linking

FehBot Can generate OTP codes to be used to verify that an IRC nick and a remote account are controlled by the same person.

#### Verify an Account (after a code has been created with the HTTP API)
    ?code ${pin}
*ex*

    ?code 8675309

## HTTP APIs
### Account Linking
The Account Linking API provides a way to create a link between a third party service and an IRC nick through fehbot.  Basically you give fehbot an IRC nickname and an account identifier and FehBot will produce a one time code.  When a user with the nickname associated with the code uses the ?code command FehBot will create a link between the nick and the account identifier.

#### POST /link

**Request**
```json
{
    "nick":"${irc_nickname}",
    "remoteUserName":"${account_identifier}"
}
```
**Response**
```json
{
    "code":"${linking_code}"
}
```
  
#Libraries Used

* IrcDotNet
* Mongo Driver
