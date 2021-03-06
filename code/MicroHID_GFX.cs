﻿using System;
using System.Collections;
using Dissonance.Integrations.UNet_HLAPI;
using UnityEngine;
using UnityEngine.Networking;

public class MicroHID_GFX : NetworkBehaviour
{
	public MicroHID_GFX()
	{
	}

	private void Start()
	{
		this.pmng = PlayerManager.singleton;
		this.invdis = UnityEngine.Object.FindObjectOfType<InventoryDisplay>();
		this.plyid = base.GetComponent<HlapiPlayer>();
	}

	private void Update()
	{
		if (base.isLocalPlayer && Input.GetButtonDown("Fire1") && base.GetComponent<Inventory>().curItem == 16 && !this.onFire && base.GetComponent<WeaponManager>().inventoryCooldown <= 0f && base.GetComponent<Inventory>().items[base.GetComponent<Inventory>().GetItemIndex()].durability > 0f)
		{
			this.onFire = true;
			this.CallCmdUse();
			base.StartCoroutine(this.PlayAnimation());
		}
	}

	private IEnumerator PlayAnimation()
	{
		this.damageGiven = 0f;
		this.anim.SetTrigger("Shoot");
		this.shotSource.Play();
		foreach (Light light in this.progress)
		{
			light.intensity = 0f;
		}
		this.GlowLight(0);
		yield return new WaitForSeconds(2.2f);
		this.GlowLight(1);
		yield return new WaitForSeconds(2.2f);
		this.GlowLight(2);
		yield return new WaitForSeconds(2.2f);
		this.GlowLight(3);
		this.GlowLight(5);
		yield return new WaitForSeconds(2.2f);
		this.GlowLight(4);
		yield return new WaitForSeconds(0.6f);
		this.teslaFX.Play();
		for (int i = 0; i < 20; i++)
		{
			GameObject[] players = this.pmng.players;
			foreach (GameObject gameObject in players)
			{
				RaycastHit raycastHit;
				if (Vector3.Dot(this.cam.transform.forward, (this.cam.transform.position - gameObject.transform.position).normalized) < -0.92f && Physics.Raycast(this.cam.transform.position, (gameObject.transform.position - this.cam.transform.position).normalized, out raycastHit, this.range) && raycastHit.transform.name == gameObject.name)
				{
					Hitmarker.Hit(2.3f);
					this.CallCmdHurtPlayersInRange(gameObject);
				}
			}
			yield return new WaitForSeconds(0.25f);
		}
		this.onFire = false;
		foreach (Light light2 in this.progress)
		{
			light2.intensity = 0f;
		}
		yield break;
	}

	[Command(channel = 11)]
	private void CmdHurtPlayersInRange(GameObject ply)
	{
		if (base.GetComponent<Inventory>().curItem == 16 && Vector3.Distance(base.GetComponent<PlyMovementSync>().position, ply.transform.position) < this.range && base.GetComponent<WeaponManager>().GetShootPermission(ply.GetComponent<CharacterClassManager>()))
		{
			base.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo((float)UnityEngine.Random.Range(200, 300), string.Empty, "TESLA"), ply);
		}
	}

	[Command(channel = 2)]
	private void CmdUse()
	{
		Inventory component = base.GetComponent<Inventory>();
		for (int i = 0; i < (int)component.items.Count; i++)
		{
			if (component.items[i].id == 16)
			{
				component.items.ModifyDuration(i, 0f);
			}
		}
		this.CallRpcSyncAnim();
	}

	[ClientRpc(channel = 1)]
	private void RpcSyncAnim()
	{
		if (!base.isLocalPlayer)
		{
			base.GetComponent<AnimationController>().PlaySound("HID_Shoot", true);
			base.GetComponent<AnimationController>().DoAnimation("Shoot");
		}
	}

	private void GlowLight(int id)
	{
		base.StartCoroutine(this.SetLightState((id != 5) ? ((id != 4) ? 3f : 6f) : 50f, this.progress[id], (id != 5) ? 2f : 50f));
	}

	private IEnumerator SetLightState(float targetIntensity, Light light, float speed)
	{
		while (light.intensity < targetIntensity)
		{
			light.intensity += Time.deltaTime * speed;
			yield return new WaitForEndOfFrame();
		}
		yield break;
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdHurtPlayersInRange(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHurtPlayersInRange called on client.");
			return;
		}
		((MicroHID_GFX)obj).CmdHurtPlayersInRange(reader.ReadGameObject());
	}

	protected static void InvokeCmdCmdUse(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUse called on client.");
			return;
		}
		((MicroHID_GFX)obj).CmdUse();
	}

	public void CallCmdHurtPlayersInRange(GameObject ply)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdHurtPlayersInRange called on server.");
			return;
		}
		if (base.isServer)
		{
			this.CmdHurtPlayersInRange(ply);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write(0);
		networkWriter.Write((short)((ushort)5));
		networkWriter.WritePackedUInt32((uint)MicroHID_GFX.kCmdCmdHurtPlayersInRange);
		networkWriter.Write(base.GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(ply);
		base.SendCommandInternal(networkWriter, 11, "CmdHurtPlayersInRange");
	}

	public void CallCmdUse()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdUse called on server.");
			return;
		}
		if (base.isServer)
		{
			this.CmdUse();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write(0);
		networkWriter.Write((short)((ushort)5));
		networkWriter.WritePackedUInt32((uint)MicroHID_GFX.kCmdCmdUse);
		networkWriter.Write(base.GetComponent<NetworkIdentity>().netId);
		base.SendCommandInternal(networkWriter, 2, "CmdUse");
	}

	protected static void InvokeRpcRpcSyncAnim(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSyncAnim called on server.");
			return;
		}
		((MicroHID_GFX)obj).RpcSyncAnim();
	}

	public void CallRpcSyncAnim()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSyncAnim called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write(0);
		networkWriter.Write((short)((ushort)2));
		networkWriter.WritePackedUInt32((uint)MicroHID_GFX.kRpcRpcSyncAnim);
		networkWriter.Write(base.GetComponent<NetworkIdentity>().netId);
		this.SendRPCInternal(networkWriter, 1, "RpcSyncAnim");
	}

	static MicroHID_GFX()
	{
		NetworkBehaviour.RegisterCommandDelegate(typeof(MicroHID_GFX), MicroHID_GFX.kCmdCmdHurtPlayersInRange, new NetworkBehaviour.CmdDelegate(MicroHID_GFX.InvokeCmdCmdHurtPlayersInRange));
		MicroHID_GFX.kCmdCmdUse = -1833499346;
		NetworkBehaviour.RegisterCommandDelegate(typeof(MicroHID_GFX), MicroHID_GFX.kCmdCmdUse, new NetworkBehaviour.CmdDelegate(MicroHID_GFX.InvokeCmdCmdUse));
		MicroHID_GFX.kRpcRpcSyncAnim = -572266021;
		NetworkBehaviour.RegisterRpcDelegate(typeof(MicroHID_GFX), MicroHID_GFX.kRpcRpcSyncAnim, new NetworkBehaviour.CmdDelegate(MicroHID_GFX.InvokeRpcRpcSyncAnim));
		NetworkCRC.RegisterBehaviour("MicroHID_GFX", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result;
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}

	public Light[] progress;

	public ParticleSystem teslaFX;

	public Animator anim;

	public AudioSource shotSource;

	public bool onFire;

	public float range;

	public GameObject cam;

	private PlayerManager pmng;

	private HlapiPlayer plyid;

	private InventoryDisplay invdis;

	private float damageGiven;

	private static int kCmdCmdHurtPlayersInRange = 1650017390;

	private static int kCmdCmdUse;

	private static int kRpcRpcSyncAnim;
}
