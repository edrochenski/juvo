# Issues/Bugs

## `JuvoProcess`
* Log files on __Linux__ seem to be using a back-slash instead of a forward-slash and are not being stored in a logs directory
* Build fails on __Linux__ when running from root source folder due to case-sensitivity issues

## `JuvoClient`
* `LoadPlugins()` is using a hard-coded reference to get load `System.Runtime.dll`
* `LoadPlugins()` should verify a plugin's command list doesn't override a built-in
* `set culture` using an invalid language tag causes an __expected__ error, but seems to send strange commands to IRC 
* Discord bots connect even when Discord is marked as disabled in the config

## `JuvoProcess.Modules.HackerNewsModule`
* 'Ask/Show/Etc HN' entries display a blank url, should show link to HN
* Log shows occasional unhandled exceptions when formatting an output string from a new story
* Ouput sometimes shows a blank story title AND url

## `JuvoClient.Modules.WeatherModule`
* commands not working if culure is `eo-001`

## `JuvoProcess.Net.Discord`
* `DiscordClient`: Occassional disconnection(?) occurs and recovery/reconnection fails causing an unhandled exception with `Listen()`

## `JuvoProcess.Net.Irc`
* Messages are being truncated when they exceed the 484 limit and have to be broken up

# Planned Changes

## `JuvoProcess`
* Accept command line arguments in the process to override behavior and config file
* ~~Move `WebHost` refs/code from `Program` into `JuvoClient`~~
* ~~Move configuration code from `JuvoClient` into `Program` and allow it to be injected in~~

## `JuvoProcess.Configuration`
* Move the default config from GetDefaultConfig() into a resource file


# Planned Additions

## `JuvoProcess.*`
* Add unit tests for everything we can

## `JuvoClient`
* All configurable option to summon the but with 'botnick[ ,.:-]'

## `JuvoProcess.Net.Irc`
* Add ability to connect with SSL

# Long-Term

## `JuvoProcess.Net.Discord/Irc/Slack`
* cross-protocol communication?
