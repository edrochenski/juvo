# Issues

## `JuvoProcess`
* Log files on __Linux__ seem to be using a back-slash instead of a forward-slash and are not being stored in a logs directory
* Build fails on __Linux__ when running from root source folder due to case-sensitivity issues

## `JuvoClient`
* `set culture` using an invalid language tag causes an *expected* error, but seems to send strange commands to IRC
```
2018-02-18 01:41:50,151 [4 ] DEBUG JuvoProcess.JuvoClient - Error setting culture: Culture is not supported.
Parameter name: name
is-invalid is an invalid culture identifier.
System.Globalization.CultureNotFoundException: Culture is not supported.
Parameter name: name
is-invalid is an invalid culture identifier.
   at System.Globalization.CultureInfo.InitializeFromName(String name, Boolean useUserOverride)
   at JuvoProcess.JuvoClient.<CommandSet>d__35.MoveNext() in F:\code\gitlab\edrochenski\juvo\src\juvo\JuvoClient.cs:line 325
2018-02-18 01:41:50,178 [4 ] DEBUG JuvoProcess.JuvoClient - Enqueuing response...
2018-02-18 01:41:50,179 [4 ] DEBUG JuvoProcess.Net.Irc.IrcClient - << Sending 137 bytes
2018-02-18 01:41:50,206 [3 ] DEBUG JuvoProcess.Bots.IrcBot - MSG: :Ashburn.Va.Us.UnderNet.org 421 juvo Parameter :Unknown command
2018-02-18 01:41:50,207 [3 ] DEBUG JuvoProcess.Bots.IrcBot - MSG: :Ashburn.Va.Us.UnderNet.org 421 juvo is-invalid :Unknown command
```

## `JuvoProcess.Configuration`
* Move the default config from GetDefaultConfig() into a resource file

## `JuvoProcess.Modules.HackerNewsModule`
* 'Ask/Show/Etc HN' entries display a blank url, should show link to HN
* Log shows occasional unhandled exceptions when formatting an output string from a new story
* Ouput sometimes shows a blank story title AND url

## `JuvoProcess.Net.Discord`
* `DiscordClient`: Occassional disconnection(?) occurs and recovery/reconnection fails causing an unhandled exception with `Listen()`

# Planned Changes

## `JuvoProcess`
* ~~Move `WebHost` refs/code from `Program` into `JuvoClient`~~
* ~~Move configuration code from `JuvoClient` into `Program` and allow it to be injected in~~
* Accept command line arguments in the process to override behavior and config file

# Planned Additions

## `JuvoProcess.*`
* Add unit tests for everything we can

## `JuvoProcess.Net.Irc`
* Add ability to connect with SSL

# Long-Term

## `JuvoProcess.Net.Discord/Irc/Slack`
* cross-protocol communication?
