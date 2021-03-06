﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RemoteAdmin
{
	public class GiveItem : RemoteAdminBehaviour
	{
		public GiveItem()
		{
		}

		public void SetButtons(Button[] _buttons)
		{
			this.buttons = _buttons;
			foreach (Button button in this.buttons)
			{
				button.GetComponent<Outline>().effectColor = new Color(1f, 1f, 1f, 0.0784f);
			}
		}

		private void Start()
		{
			this.selectedColor = base.GetComponent<Chooser>().selectedColor;
		}

		public override void Reply(string[] reply)
		{
			if (reply.Length > 0 && reply[0] == "GIVE SUCCESS")
			{
				this.respond.text = "<color=lime>Success!</color>";
			}
		}

		private IEnumerator WaitForRespond()
		{
			if (!this.working)
			{
				this.working = true;
				this.respond.text = "<color=#3183D0>Sending request...</color>";
				float time = 0f;
				while (time < 5f && this.respond.text == "<color=#3183D0>Sending request...</color>")
				{
					time += 0.02f;
					yield return new WaitForSeconds(0.02f);
				}
				if (this.respond.text == "<color=#3183D0>Sending request...</color>")
				{
					this.respond.text = "<color=red>Timed out...</color>";
				}
				yield return new WaitForSeconds(2f);
				this.respond.text = string.Empty;
				this.working = false;
			}
			yield break;
		}

		public void SetDuration(Button b)
		{
			this.itemID = -1;
			foreach (Button button in this.buttons)
			{
				button.GetComponent<Outline>().effectColor = ((!(button == b)) ? new Color(1f, 1f, 1f, 0.0784f) : this.selectedColor);
				if (button == b)
				{
					string name = button.transform.name;
					this.itemID = int.Parse(name.Remove(0, name.IndexOf("_") + 1));
				}
			}
			this.RefreshGUI();
		}

		public void RefreshGUI()
		{
			if (this.rootObject.activeSelf)
			{
				this.command = string.Empty;
				int num = 0;
				if (this.itemID >= 0)
				{
					this.command += "GIVE (";
					foreach (PlayersMonitor.Player player in base.GetComponent<PlayersMonitor>().players)
					{
						if (player.GetSelected())
						{
							num++;
							this.command = this.command + player.playerInfo.address + "; ";
						}
					}
					string text = this.command;
					this.command = string.Concat(new object[]
					{
						text,
						") {",
						this.itemID,
						"}"
					});
					if (num == 0)
					{
						this.warning.text = "<color=orange>No player(s) selected.</color>";
						this.confirmButton.interactable = false;
					}
					else
					{
						this.warning.text = "<color=lime>It will affect on " + num + " player(s).</color>";
						this.confirmButton.interactable = true;
					}
					this.commandPreview.text = this.command;
				}
				else
				{
					this.warning.text = "<color=orange>Please specify the item.</color>";
				}
			}
		}

		public void Confirm()
		{
			PlayerManager.localPlayer.GetComponent<QueryProcessor>().CallCmdSendQuery(this.command);
			base.StartCoroutine(this.WaitForRespond());
		}

		public int itemID;

		private Button[] buttons;

		private Color selectedColor;

		public Button confirmButton;

		public Text commandPreview;

		public Text warning;

		public Text respond;

		public GameObject rootObject;

		private bool working;

		private string command;
	}
}
