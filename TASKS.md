# Issues/Bugs

## `JuvoProcess`
* Log files on __Linux__ seem to be using a back-slash instead of a forward-slash and are not being stored in a logs directory
* Build fails on __Linux__ when running from root source folder due to case-sensitivity issues
* `Program.GetSystemInfo()`:
  * replace hardcoded paths
  * set paths for OSX

## `JuvoProcess.Bots.DiscordBot`
* Shutdown properly when `Quit()` is called

## `JuvoProcess.Bots.DiscordBotFactory`
* `Create()` is using concrete classes in the `DiscordBot` constructor

## `JuvoProcess.Bots.SlackBot`
* Shutdown properly when `Quit()` is called

## `JuvoProcess.JuvoClient`
* `LoadPlugins()` is using a hard-coded path to get/load `System.Runtime.dll` reference
* `LoadPlugins()` should verify a plugin's command list doesn't override a built-in
* `set culture` using an invalid language tag causes an __expected__ error, but seems to send strange commands to IRC 
* Discord bots connect even when Discord is marked as disabled in the config

## `JuvoProcess.Modules.HackerNewsModule`
* 'Ask/Show/Etc. HN' entries display a blank URL, should show link to HN
* Log shows occasional unhandled exceptions when formatting an output string from a new story
* Output sometimes shows a blank story title AND URL

## `JuvoProcess.Modules.WeatherModule`
* commands not working if culture is `eo-001`

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
* Accept command line arguments in the process to override behavior and config file
* ~~Move `WebHost` refs/code from `Program` into `JuvoClient`~~
* ~~Move configuration code from `JuvoClient` into `Program` and allow it to be injected in~~

## `JuvoProcess.Configuration`
* Move the default config from GetDefaultConfig() into a resource file

## `JuvoProcess.Bots`
* Move classes to protocol-specific directories (even if not namespaced)

# Planned Additions

## `JuvoProcess.*`
* Add unit tests for everything we can

## `JuvoClient`
* All configurable option to summon the but with 'nick[ ,.:-]'

## `JuvoProcess.Net.Irc`
* Add ability to connect with SSL

# Long-Term

## `JuvoProcess.Net.Discord/Irc/Slack`
* cross-protocol communication?
