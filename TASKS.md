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
* ~~Log files on __Linux__ seem to be using a back-slash instead of a forward-slash and are not being stored in a logs directory~~
* ~~Build fails on __Linux__ when running from root source folder due to case-sensitivity issues~~

## `JuvoProcess.Bots`
* `DiscordBot`: Shutdown properly when `Quit()` is called
* `DiscordBotFactory`: `Create()` is using concrete classes in the `DiscordBot` constructor
* `IrcBot`: `Connect()` only uses the first server/port in the config
* `SlackBot`: Shutdown properly when `Quit()` is called

## `JuvoProcess.Net.Discord`
* `DiscordClient`
  * Occasional disconnection(?) occurs and recovery/reconnection fails causing an unhandled exception with `Listen()`
  * Implement `IDisposable` properly

## `JuvoProcess.Net.Irc`
* Messages are being truncated when they exceed the 484 limit and have to be broken up
* `IIrcClient`: reimplement method `(IrcChannelMode Mode, bool HasAddParam, bool HasRemParam) LookupChannelMode(char mode);`
* `IrcClient`: assumes configured nick worked in `OnConnected`


# Planned Changes

## `JuvoProcess`
* `Program`: Accept command line arguments in the process to override behavior and config file
* ~~Move `WebHost` refs/code from `Program` into `JuvoClient`~~
* ~~Move configuration code from `JuvoClient` into `Program` and allow it to be injected in~~

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
