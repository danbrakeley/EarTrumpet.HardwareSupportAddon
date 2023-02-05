# EarTrumpet.HardwareSupportAddon

## Overview

This is an addon for [EarTrumpet](https://github.com/File-New-Project/EarTrumpet) that adds limited support for MIDI and deej hardware controls.

This repo started as a fork of [EarTrumpet.HardwareSupportAddon](https://github.com/applapp/EarTrumpet.HardwareSupportAddon). I made this fork to clean up some of the UI and to add support for changing the volume of whichever app has focus in Windows. If the original project shows signs of life soon, then I will try to package my changes into PRs and get it merged into a single unit.

Please check out and support the original authors, as my fork only exists because of all of their hard work!

## Supported hardware

- MIDI controllers (sliders / rotary potentiometers with linear mapping, buttons, rotary encoders).
- [deej](https://github.com/omriharel/deej) controllers (using the serial port, suitable e.g. for arduino projects) (sliders / rotary potentiometers, but makers can also map buttons or rotary encoders to linear values in their microcontroller-code to use all functionalities).

The plugin offers extensive configuration possibilities including offset, implicit control inversion, and scaling options.

### Supported commands to be controlled via hardware:

- Volume/mute for:
  - the main mix of one or more audio devices.
  - named applications
  - foreground app in Windows
  - apps via their position in the app list in EarTrumpet (aka by index)
- Control of the default output device in Windows.
  - TODO: is that what this does? haven't played with it yet

## Open Questions

- Is mapping apps via their index useful? Presumably a given app will not have a fixed index over time, either as other apps are started/stopped, or after a reboot, so you wouldn't be able to build up any muscle memory around which control will influence which app. Maybe there's still a valid use case here?
