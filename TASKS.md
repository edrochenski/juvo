# Issues

## `JuvoProcess`
* Log files on __Linux__ seem to be using a back-slash instead of a forward-slash and are not being stored in a logs directory

## `JuvoProcess.Modules.HackerNewsModule`
* 'Ask/Show/Etc HN' entries display a blank url, should show link to HN
* Log shows occasional unhandled exceptions when formatting an output string from a new story
* Ouput sometimes shows a blank story title AND url

## `JuvoProcess.Net.Discord`
* `DiscordClient`: Occassional disconnection(?) occurs and recovery/reconnection fails causing an unhandled exception with `Listen()`

# Planned Changes

## `JuvoProcess`
* Move `WebHost` refs/code from `Program` into `JuvoClient`
* ~~Move configuration code from `JuvoClient` into `Program` and allow it to be injected in~~

# Planned Additions

## `JuvoProcess.*`
* Add unit tests for everything we can