[![Carthage compatible](https://img.shields.io/badge/Carthage-compatible-4BC51D.svg?style=flat)](https://github.com/Carthage/Carthage)
# What is HearthMirror?

HearthMirror is a library written in C/C++ that allows reading data directly from a running [Hearthstone](http://battle.net/hearthstone "Hearthstone") game.

## Usage

### MacOS:
The library can be built as a dynamic lib or as an OSX Framework. If you intend to use the features in your app the following steps have to be done:

- Add the following section to your plist file:
    ```xml
    <key>SecTaskAccess</key>
    <array>
		<string>allowed</string>
	</array>
    ```
    
- Embed plist to the binary
- Sign your code
- Get task attach rights by calling the *acquireTaskportRight()* function

### Windows:
Full support with 32 bit. No extra access rights are necessary.

## FAQ

* Is it injecting or modifying the memory of the game? No.
* Is 64bit supported? On MacOS yes, on Windows not.
