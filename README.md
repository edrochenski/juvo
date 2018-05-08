# Juvo
Juvo is a multi-protocol/client chat bot designed to be extensible and _eventually_ *fully*-testable.

# Goals
 1. __Multi-protocol/client__: one bot to rule them all? I am tired of running many different processes that are essentially providing the same functionality.
 
 2. __Multi-platform__: Run on win/Linux/mac with limited/no need for conditional compilation, configuration or runtime decisions.
 
 3. __Extendable__: Low-friction extensibility that still provides full access to the bot.
 
 4. __Testable/Clean__: Following good patterns/practices, keeping [SOLID](https://en.wikipedia.org/wiki/SOLID_(object-oriented_design)) and [DRY](https://en.wikipedia.org/wiki/Don%27t_repeat_yourself) in-mind as much as possible.

 5. __Easy__: Easy to understand, hack on, configure, build, and run.

# Current State
This is a very early-stage, "born-again" project. Originally written in C++ and utilizing Lua scripts, it has gotten new life in C# with .NET Core and .NET language-based scripting.

I am currently keeping track of my tasks/ideas/bugs/features [here](TASKS.md)

# Contact
- __Maintainers__
    - Ed Rochenski (@edrochenski)
        - edrochenski [ at ] bytedown [dot] com
        - [Twitter](https://twitter.com/edrochenski)
        - [Twitch](https://twitch.tv/edroche78)

- __Chat__
    - IRC: [freenode](irc://chat.freenode.net/juvo) | [efnet](irc://irc.choopa.net/juvo) | [undernet](irc://irc.undernet.org/juvo)
    - Discord: [#juvo on CodeCompileRun](https://discord.gg/WczMAFM)
    - Slack: [#juvo on CodeCompileRun](https://join.slack.com/t/codecompilerun/shared_invite/enQtMzIwMDA2MDAyNDMzLTNkOGEwOTAyN2I1MzE1ODczMTZhZWMxYTYzYjQ4MjM1YzdhOGIzZmIzOTdiMmQxNTk4N2U3MzJiNzAyMDAwZTI)

# License
Juvo is licensed under the [MIT License](LICENSE)