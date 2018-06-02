# Issues/Bugs

## `JuvoProcess`
* `JuvoClient`:
  * `LoadPlugins()` is using a hard-coded path to get/load `System.Runtime.dll` reference
  * `LoadPlugins()` should verify a plugin's command list doesn't override a built-in
  * `set culture` using an invalid language tag causes an __expected__ error, but seems to send strange commands to IRC 

## `JuvoProcess.Bots`
* `IrcBot`: `Connect()` only uses the first server/port in the config

## `JuvoProcess.Net.Discord`
* Move all enums to Enums.cs
* `DiscordClient`
  * Receiving `The WebSocket has already been started.` when trying to reconnect
  * GUILD_DELETE unhandled: {"t":"GUILD_DELETE","s":21,"op":0,"d":{"unavailable":true,"id":"376517576907292675"}}

## `JuvoProcess.Net.Irc`
* `IrcClient`: assumes configured nick worked in `OnConnected`

## `JuvoProcess.Net.Slack`
* Calling `die` command will cause several exceptions before the bot terminates

# Planned Changes

## `JuvoProcess`
* `JuvoClient`: Allow configurable option to summon the but with `bot-nick[ ,.:-] \<command\>`
* `Program`: Accept command line arguments in the process to override behavior and config file
* Add unit tests for everything we can

## `JuvoProcess.Configuration`
* Generate a config file if one doesn't exist (from embedded resource?)

## `JuvoProcess.Bots`
* Move classes to protocol-specific directories (even if not namespaced)

## `JuvoProcess.Net.Irc`
* Add ability to connect with SSL


# Long-Term

## `JuvoProcess.Bots.*`
* cross-protocol communication?
