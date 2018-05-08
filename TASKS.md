# Issues/Bugs

## `JuvoProcess`
* `Program`:
  * replace hardcoded paths in `GetSystemInfo()`
  * set paths for OSX in `GetSystemInfo()`
* `JuvoClient`:
  * `LoadPlugins()` is using a hard-coded path to get/load `System.Runtime.dll` reference
  * `LoadPlugins()` should verify a plugin's command list doesn't override a built-in
  * `set culture` using an invalid language tag causes an __expected__ error, but seems to send strange commands to IRC 
  * Discord bots connect even when Discord is marked as disabled in the config

## `JuvoProcess.Bots`
* `DiscordBotFactory`: `Create()` is using concrete class in the `DiscordBot` constructor
* `IrcBot`: `Connect()` only uses the first server/port in the config

## `JuvoProcess.Net.Discord`
* `DiscordClient`
  * GUILD_CREATE unhandled
  * GUILD_DELETE unhandled: {"t":"GUILD_DELETE","s":21,"op":0,"d":{"unavailable":true,"id":"376517576907292675"}}

## `JuvoProcess.Net.Irc`
* `IrcClient`: assumes configured nick worked in `OnConnected`


# Planned Changes

## `JuvoProcess`
* `Program`: Accept command line arguments in the process to override behavior and config file

## `JuvoProcess.Configuration`
* Generate a config file if one doesn't exist (from embedded resource?)

## `JuvoProcess.Bots`
* Move classes to protocol-specific directories (even if not namespaced)


# Planned Additions

## `JuvoProcess.*`
* Add unit tests for everything we can

## `JuvoClient`
* Allow configurable option to summon the but with 'bot-nick[ ,.:-] \<command\>'

## `JuvoProcess.Net.Irc`
* Add ability to connect with SSL


# Long-Term

## `JuvoProcess.Bots.*`
* cross-protocol communication?
