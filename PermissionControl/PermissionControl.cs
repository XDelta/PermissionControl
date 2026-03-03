using System;
using System.Reflection;

using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

using HarmonyLib;

using ResoniteModLoader;

namespace PermissionControl;
public class PermissionControl : ResoniteMod {
	internal const string VERSION_CONSTANT = "2.0.2";
	public override string Name => "PermissionControl";
	public override string Author => "Delta";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/XDelta/PermissionControl";

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> Enabled = new("Enabled", "Enable Mod", () => true);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> ShowDebugInfo = new("ShowDebugInfo", "Show permission debugging info and RefIDs for Roles", () => false);

	private static ModConfiguration Config;

	public override void OnEngineInit() {
		Config = GetConfiguration()!;
		Config.Save(true);
		Harmony harmony = new("net.deltawolf.PermissionControl");
		harmony.PatchAll();
	}

	[HarmonyPatch(typeof(SessionControlDialog), "GenerateUi")]
	class SessionControlDialog_GenerateUi_Patch {
		public static void Postfix(SessionControlDialog __instance, SessionControlDialog.Tab tab) {
			if (tab == SessionControlDialog.Tab.Permissions) {
				// Find the SyncRef<Button> for the permissionOverridesButton
				if (__instance.GetType()
					.GetField("_permissionOverridesButton", BindingFlags.NonPublic | BindingFlags.Instance)
					.GetValue(__instance) is SyncRef<Button> permissionOverridesButton) {
					// Change the localeString of the permissionOverridesButton
					if (Config.GetValue(Enabled)) {
						permissionOverridesButton.Target.Slot.GetComponentInChildren<Text>().Content.Value = "Edit Permission Overrides";
					}
				}
			}
		}
	}

	[HarmonyPatch(typeof(SessionControlDialog), "OnClearUserPermissionOverrides")]
	class SessionControlDialog_Patch {
		private static TextField userIDTextField;
		public static bool Prefix(SessionControlDialog __instance) {
			if (Enabled.Value) {
				RectTransform modalOverlay = __instance.Slot.OpenModalOverlay(new float2(0.7f, 0.7f));
				OpenOverrideEditor(modalOverlay);
				return false;
			} else {
				return true;
			}
		}

		public static void OpenOverrideEditor(RectTransform rect) {
			World focusedWorld = rect.Slot.Engine.WorldManager.FocusedWorld;
			UIBuilder ui = new(rect.Slot);
			RadiantUI_Constants.SetupDefaultStyle(ui);
			ui.Style.MinHeight = 24f;
			ui.HorizontalHeader(48f, out RectTransform header, out RectTransform content);
			ui.ForceNext = header;
			ui.Text("Edit Permission Overrides");
			content.AddFixedPadding(8f, 0f, 0f, 0f);
			ui.NestInto(content);
			ui.SplitVertically(0.20f, out RectTransform top, out RectTransform bottom, 0.02f);
			ui.NestInto(top);
			ui.VerticalLayout(4f).ForceExpandHeight.Value = false;
			RectTransform sp = ui.Spacer(3f);
			sp.Slot.AttachComponent<Image>().Tint.Value = new colorX(1f, 1f, 1f, 0.5f);
			if (ShowDebugInfo.Value) {
				ui.TextField($"Debug Info:  World: {focusedWorld.Name}, RolesVersion: {focusedWorld.Permissions.RolesVersion}");
				//Roles version changes when adding/removing actual role options. ~129 for a default world
			}
			ui.NestInto(ui.Next("Roles"));
			ui.SplitHorizontally(0.25f, out RectTransform left, out RectTransform right, 0.01f);
			ui.NestInto(left);
			userIDTextField = ui.TextField();
			userIDTextField.Text.NullContent.Value = "<i>U-ID</i>";
			userIDTextField.Editor.Target.FinishHandling.Value = TextEditor.FinishAction.NullOnWhitespace;
			userIDTextField.Text.Align = Alignment.MiddleLeft;
			ui.NestOut();
			ui.NestInto(right);
			ui.HorizontalLayout(1f);
			for (int i = 0; i < focusedWorld.Permissions.Roles.Count; i++) {
				PermissionSet permissionSet = focusedWorld.Permissions.Roles[i];
				Button button = ui.Button<int>(permissionSet.RoleName.Value, ui.Style.ButtonColor, new ButtonEventHandler<int>(AddSingleUserPermissionOverride), i);
			}
			ui.NestOut();
			ui.NestOut();
			ui.NestOut();

			ui.Button("Clear All User Overrides", new ButtonEventHandler(ClearUserPermissionOverrides));
			ui.CurrentRect.Slot.AttachComponent<LayoutElement>().MinHeight.Value = 24f;
			ui.NestInto(bottom);

			//Stop early if no overrides exist
			if (focusedWorld.Permissions.DefaultUserPermissions.Count < 1) {
				AddLabel(ui, "No Permission Overrides Found");
				return;
			}

			//List each override entry
			var currentLine = 0;
			ui.ScrollArea(new Alignment?(Alignment.TopCenter));
			ui.VerticalLayout(4f, 4f);
			ui.FitContent(SizeFit.Disabled, SizeFit.PreferredSize);
			ui.Style.MinHeight = 24f;
			//TODO Rebuild on permission changes
			foreach (var entry in focusedWorld.Permissions.DefaultUserPermissions) {
				ui.Style.ButtonColor = new colorX(0.08f);
				var UserEntry = ui.Next(entry.Key); //UserID
				if (currentLine % 2 == 1) {
					ui.Style.ButtonColor = new colorX(0.16f);
				}
				ui.NestInto(UserEntry);
				ui.HorizontalLayout(1f);
				AddFriendLinkButton(ui, entry.Key);
				try {
					if (Config.GetValue(ShowDebugInfo)) { //Role Override Name
						AddLabel(ui, entry.Value.Target.RoleName.Value + " [" + entry.Value + "]");
					} else {
						AddLabel(ui, entry.Value.Target.RoleName.Value);
					}
				} catch (Exception) {
					AddLabel(ui, "[Invalid Role]");
				}
				ui.Button("Remove", new colorX(0.5f, 0.1f, 0.1f), new ButtonEventHandler<string>(ClearSingleUserPermissionOverrides), entry.Key);
				ui.NestOut();
				ui.NestOut();
				currentLine += 1;
			}
			ui.Style.ButtonColor = new colorX(0.08f);
			ui.NestOut();
		}

		public static void AddFriendLinkButton(UIBuilder ui, string user) {
			ui.Next("Button");
			ui.Current.AttachComponent<Image>().Tint.Value = ui.Style.ButtonColor;
			Button button = ui.Current.AttachComponent<Button>();
			ui.Current.AttachComponent<ContactLink>().UserId.Value = user;
			ui.Nest();
			ui.Text(user);
			ui.NestOut();
		}

		public static void AddLabel(UIBuilder ui, string label) {
			ui.Next("Button");
			ui.Current.AttachComponent<Image>().Tint.Value = ui.Style.ButtonColor;
			ui.Nest();
			ui.Text(label);
			ui.NestOut();
		}

		[SyncMethod(typeof(Delegate))]
		public static void ClearUserPermissionOverrides(IButton button, ButtonEventData eventData) {
			World focusedWorld = button.Slot.Engine.WorldManager.FocusedWorld;
			focusedWorld.RunSynchronously(delegate {
				if (!focusedWorld.IsAuthority) {
					return;
				}
				focusedWorld.Permissions.DefaultUserPermissions.Clear();
			});
		}

		[SyncMethod(typeof(Delegate))]
		public static void ClearSingleUserPermissionOverrides(IButton button, ButtonEventData eventData, string user) {
			World focusedWorld = button.Slot.Engine.WorldManager.FocusedWorld;
			focusedWorld.RunSynchronously(delegate {
				if (!focusedWorld.IsAuthority) {
					return;
				}
				focusedWorld.Permissions.DefaultUserPermissions.Remove(user);
			});
		}

		[SyncMethod(typeof(Delegate))]
		public static void AddSingleUserPermissionOverride(IButton button, ButtonEventData eventData, int permissionSet) {
			World focusedWorld = button.Slot.Engine.WorldManager.FocusedWorld;
			focusedWorld.RunSynchronously(delegate {
				if (!focusedWorld.IsAuthority) {
					return;
				}

				string user = userIDTextField.Text.Content.Value;
				user = user.Trim();
				if (string.IsNullOrEmpty(user)) {
					return;
				}
				if (user.StartsWith("u-", StringComparison.OrdinalIgnoreCase)) {
					user = "U-" + user[2..];
				} else {
					user = "U-" + user;
				}

				Msg($"Adding override for {user} as {focusedWorld.Permissions.Roles[permissionSet].RoleName}");
				focusedWorld.Permissions.DefaultUserPermissions.Remove(user); //Remove any overrides before adding a new one
				focusedWorld.Permissions.DefaultUserPermissions.Add(user, focusedWorld.Permissions.Roles[permissionSet]);
			});
		}
	}
}
