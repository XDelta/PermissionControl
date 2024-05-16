# PermissionControl

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://resonite.com/).
Allows more fine control over permission overrides such as adding and removing overrides individually, even for users who aren't in the session.

## Installation
1. Install [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
1. Place [PermissionControl.dll](https://github.com/XDelta/PermissionControl/releases/latest/download/PermissionControl.dll) into your `rml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods` for a default install. You can create it if it's missing, or if you launch the game once with ResoniteModLoader installed it will create the folder for you.
1. Start the game. If you want to verify that the mod is working you can check your Resonite logs.

## Usage
As the host of a session, go to the session page, then to the permissions tab and click `Edit Permission Overrides`.
From here you can add additional overrides by typing a UserID (like `U-Delta`) in the field then clicking a role to give them.
All currently applied overrides will be listed with the option to open their profile by clicking their UserID and an option to remove the individual override. The target user does not need to be in the session to set or change an override for them.

## Config Options

| Config Option     | Default | Description |
| ------------------ | ------- | ----------- |
| `Enabled` | `true` | Enables the mod |
| `ShowDebugInfo` | `false` | If `true`, will show permission debugging info and RefIDs for Roles |

![Image of permission override screen](https://github.com/XDelta/PermissionControl/assets/7883807/999a72e7-9ce9-4978-9ad0-ac96d3b7c7fd)
