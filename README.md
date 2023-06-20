﻿# PermissionControl

A [NeosModLoader](https://github.com/neos-modding-group/NeosModLoader) mod for [Neos VR](https://neos.com/).
Allows more fine control over permission overrides such as adding and removing overrides individually, even for users who aren't in the session. A solution for https://github.com/Neos-Metaverse/NeosPublic/issues/3768

## Installation
1. Install [NeosModLoader](https://github.com/neos-modding-group/NeosModLoader).
1. Place [PermissionControl.dll](https://github.com/XDelta/PermissionControl/releases/latest/download/PermissionControl.dll) into your `nml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\NeosVR\nml_mods` for a default install. You can create it if it's missing, or if you launch the game once with NeosModLoader installed it will create the folder for you.
1. Start the game. If you want to verify that the mod is working you can check your Neos logs.

## Usage
As the host of a session, go to the session page, then to the permissions tab and click `Edit Permission Overrides`.
From here you can add additional overrides by typing a UserID (like U-Delta) where `U-ID` is then clicking a role to give them.
All currently applied overrides will be listed with the option to open their profile by clicking their UserID and an option to remove the override.

## Config Options

| Config Option     | Default | Description |
| ------------------ | ------- | ----------- |
| `Enabled` | `true` | Enables the mod |
| `ShowDebugInfo` | `false` | If `true`, will show permission debugging info and RefIDs for Roles |

![Image of permission override screen](https://github.com/XDelta/PermissionControl/assets/7883807/707466d5-c155-40a6-9fa0-24fd2ccca1cf)
