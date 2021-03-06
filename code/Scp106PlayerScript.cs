﻿using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class Scp106PlayerScript : NetworkBehaviour
{
	public Vector3 NetworkportalPosition
	{
		get
		{
			return this.portalPosition;
		}
		set
		{
			uint dirtyBit = 1u;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				this.SetPortalPosition(value);
				base.syncVarHookGuard = false;
			}
			base.SetSyncVar<Vector3>(value, ref this.portalPosition, dirtyBit);
		}
	}

	public Scp106PlayerScript()
	{
	}

	private void Start()
	{
		this.cooldownImg = GameObject.Find("Cooldown106").GetComponent<Image>();
		this.ccm = base.GetComponent<CharacterClassManager>();
		this.fpc = base.GetComponent<FirstPersonController>();
		base.InvokeRepeating("HumanPocketLoss", 1f, 1f);
	}

	private void Update()
	{
		this.CheckForInventoryInput();
		this.CheckForShootInput();
		this.AnimateHighlightedText();
		this.UpdatePointText();
	}

	private void HumanPocketLoss()
	{
		if (base.isLocalPlayer && base.transform.position.y < -1500f)
		{
			base.GetComponent<PlayerStats>().CallCmdSelfDeduct(new PlayerStats.HitInfo(1f, "WORLD", "POCKET"));
		}
	}

	private void CheckForShootInput()
	{
		if (base.isLocalPlayer && this.iAm106)
		{
			this.cooldownImg.fillAmount = Mathf.Clamp01((this.attackCooldown > 0f) ? (1f - this.attackCooldown * 2f) : 0f);
			if (this.attackCooldown > 0f)
			{
				this.attackCooldown -= Time.deltaTime;
			}
			if (Input.GetButtonDown("Fire1") && this.attackCooldown <= 0f && base.GetComponent<WeaponManager>().inventoryCooldown <= 0f)
			{
				this.attackCooldown = 0.5f;
				this.Shoot();
			}
		}
	}

	private void Shoot()
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(this.plyCam.transform.position, this.plyCam.transform.forward, out raycastHit, 1.5f))
		{
			CharacterClassManager component = raycastHit.transform.GetComponent<CharacterClassManager>();
			if (component != null && component.klasy[component.curClass].team != Team.SCP)
			{
				this.CallCmdMovePlayer(raycastHit.transform.gameObject, ServerTime.time);
				Hitmarker.Hit(1.5f);
			}
		}
	}

	private void UpdatePointText()
	{
		if (this.pointsText == null)
		{
			this.pointsText = UnityEngine.Object.FindObjectOfType<ScpInterfaces>().Scp106_ability_points;
			return;
		}
		this.ultimatePoints += Time.deltaTime * 6.66f * this.teleportSpeed;
		this.ultimatePoints = Mathf.Clamp(this.ultimatePoints, 0f, 100f);
		this.pointsText.text = TranslationReader.Get("Legancy_Interfaces", 11);
	}

	private bool BuyAbility(int cost)
	{
		if ((float)cost <= this.ultimatePoints)
		{
			this.ultimatePoints -= (float)cost;
			return true;
		}
		return false;
	}

	private void AnimateHighlightedText()
	{
		if (this.highlightedAbilityText == null)
		{
			this.highlightedAbilityText = UnityEngine.Object.FindObjectOfType<ScpInterfaces>().Scp106_ability_highlight;
			return;
		}
		this.highlightedString = string.Empty;
		if (this.highlightID == 1)
		{
			this.highlightedString = TranslationReader.Get("Legancy_Interfaces", 12);
		}
		if (this.highlightID == 2)
		{
			this.highlightedString = TranslationReader.Get("Legancy_Interfaces", 13);
		}
		if (this.highlightedString != this.highlightedAbilityText.text)
		{
			if (this.highlightedAbilityText.canvasRenderer.GetAlpha() > 0f)
			{
				this.highlightedAbilityText.canvasRenderer.SetAlpha(this.highlightedAbilityText.canvasRenderer.GetAlpha() - Time.deltaTime * 4f);
			}
			else
			{
				this.highlightedAbilityText.text = this.highlightedString;
			}
		}
		else if (this.highlightedAbilityText.canvasRenderer.GetAlpha() < 1f && this.highlightedString != string.Empty)
		{
			this.highlightedAbilityText.canvasRenderer.SetAlpha(this.highlightedAbilityText.canvasRenderer.GetAlpha() + Time.deltaTime * 4f);
		}
	}

	private void CheckForInventoryInput()
	{
		if (base.isLocalPlayer)
		{
			if (this.popup106 == null)
			{
				this.popup106 = UnityEngine.Object.FindObjectOfType<ScpInterfaces>().Scp106_eq;
				return;
			}
			bool flag = this.iAm106 & Input.GetButton("Inventory");
			CursorManager.scp106 = flag;
			this.popup106.SetActive(flag);
			this.fpc.m_MouseLook.scp106_eq = flag;
		}
	}

	public void Init(int classID, Class c)
	{
		this.iAm106 = (classID == 3);
		this.sameClass = (c.team == Team.SCP);
	}

	public void SetDoors()
	{
		if (base.isLocalPlayer)
		{
			Door[] array = UnityEngine.Object.FindObjectsOfType<Door>();
			foreach (Door door in array)
			{
				foreach (Collider collider in door.GetComponentsInChildren<Collider>())
				{
					if (collider.tag != "DoorButton")
					{
						try
						{
							collider.isTrigger = this.iAm106;
						}
						catch
						{
						}
					}
				}
			}
		}
	}

	public void Contain(bool isScp)
	{
		UnityEngine.Object.Instantiate<GameObject>(this.screamsPrefab);
		if (base.isLocalPlayer && this.iAm106)
		{
			this.ultimatePoints = 0f;
			base.StopAllCoroutines();
			base.StartCoroutine(this.ContainAnimation(isScp));
		}
	}

	public void GotoPD()
	{
		if (base.isLocalPlayer)
		{
			base.transform.position = Vector3.down * 2000f;
		}
	}

	public void DeletePortal()
	{
		if (this.portalPosition.y < 900f)
		{
			this.portalPrefab = null;
			this.NetworkportalPosition = Vector3.zero;
		}
	}

	public void UseTeleport()
	{
		if (this.portalPrefab == null)
		{
			return;
		}
		if (this.BuyAbility(100) && this.portalPosition != Vector3.zero)
		{
			base.StartCoroutine(this.DoTeleportAnimation());
		}
		else
		{
			base.StartCoroutine(this.HighlightPointsText());
		}
	}

	private void SetPortalPosition(Vector3 pos)
	{
		this.NetworkportalPosition = pos;
		base.StartCoroutine(this.DoPortalSetupAnimation());
	}

	public void CreatePortalInCurrentPosition()
	{
		if (this.BuyAbility(100))
		{
			if (base.isLocalPlayer)
			{
				this.CallCmdMakePortal(base.gameObject);
			}
		}
		else
		{
			base.StartCoroutine(this.HighlightPointsText());
		}
	}

	private IEnumerator ContainAnimation(bool b)
	{
		this.NetworkportalPosition = Vector3.zero;
		VignetteAndChromaticAberration vaca = base.GetComponentInChildren<VignetteAndChromaticAberration>();
		this.fpc.m_JumpSpeed = 0f;
		this.goingViaThePortal = true;
		yield return new WaitForSeconds(15f);
		float y = base.transform.position.y - 2.5f;
		this.fpc.noclip = true;
		while (base.transform.position.y > y && this.ccm.curClass != 2)
		{
			if (base.transform.position.y - 2f < y)
			{
				vaca.intensity += Time.deltaTime / 2f;
			}
			vaca.intensity = Mathf.Clamp(vaca.intensity, 0.036f, 1f);
			base.transform.position += Vector3.down * Time.deltaTime / 2f;
			yield return new WaitForEndOfFrame();
		}
		this.fpc.noclip = false;
		if (b)
		{
			this.Kill();
		}
		this.goingViaThePortal = false;
		yield break;
	}

	private void Kill()
	{
		base.GetComponent<CharacterClassManager>().CallCmdSuicide(default(PlayerStats.HitInfo));
	}

	private IEnumerator HighlightPointsText()
	{
		if (!this.isHighlightingPoints)
		{
			this.isHighlightingPoints = true;
			while ((double)this.pointsText.color.g > 0.05)
			{
				this.pointsText.color = Color.Lerp(this.pointsText.color, Color.red, 10f * Time.deltaTime);
				yield return new WaitForEndOfFrame();
			}
			while ((double)this.pointsText.color.g < 0.95)
			{
				this.pointsText.color = Color.Lerp(this.pointsText.color, Color.white, 10f * Time.deltaTime);
				yield return new WaitForEndOfFrame();
			}
			this.isHighlightingPoints = false;
		}
		yield break;
	}

	private IEnumerator DoPortalSetupAnimation()
	{
		while (this.portalPrefab == null)
		{
			this.portalPrefab = GameObject.Find("SCP106_PORTAL");
			yield return new WaitForEndOfFrame();
		}
		if (this.portalPosition != this.portalPrefab.transform.position)
		{
			Animator portalAnim = this.portalPrefab.GetComponent<Animator>();
			portalAnim.SetBool("activated", false);
			yield return new WaitForSeconds(1f);
			this.portalPrefab.transform.position = this.portalPosition;
			portalAnim.SetBool("activated", true);
		}
		yield break;
	}

	private IEnumerator DoTeleportAnimation()
	{
		if (this.portalPrefab != null && !this.goingViaThePortal)
		{
			this.goingViaThePortal = true;
			VignetteAndChromaticAberration vaca = base.GetComponentInChildren<VignetteAndChromaticAberration>();
			this.fpc.noclip = true;
			float y = base.transform.position.y - 2.5f;
			float duration = 0f;
			while (base.transform.position.y > y && duration < 5f)
			{
				duration += Time.fixedDeltaTime;
				if (base.transform.position.y - 2f < y)
				{
					vaca.intensity += Time.fixedDeltaTime / 2f * this.teleportSpeed;
				}
				vaca.intensity = Mathf.Clamp(vaca.intensity, 0.036f, 1f);
				base.transform.position += Vector3.down * Time.fixedDeltaTime / 2f * this.teleportSpeed;
				yield return new WaitForFixedUpdate();
			}
			if (this.portalPosition == Vector3.zero)
			{
				base.GetComponent<PlayerStats>().Explode(true);
			}
			base.transform.position = this.portalPrefab.transform.position - Vector3.up * 1.5f;
			y = base.transform.position.y + 3f;
			duration = 0f;
			while (base.transform.position.y < y && duration < 5f)
			{
				duration += Time.fixedDeltaTime;
				base.transform.position += Vector3.up * Time.fixedDeltaTime / 2f * this.teleportSpeed;
				if (base.transform.position.y + 2f > y)
				{
					vaca.intensity -= Time.fixedDeltaTime / 2f * this.teleportSpeed;
				}
				vaca.intensity = Mathf.Clamp(vaca.intensity, 0.036f, 1f);
				yield return new WaitForFixedUpdate();
			}
			this.fpc.noclip = false;
			this.goingViaThePortal = false;
		}
		yield break;
	}

	[Command(channel = 4)]
	public void CmdMakePortal(GameObject go)
	{
		go.GetComponent<Scp106PlayerScript>().SetPortalPosition(go.transform.position + Vector3.down * 1.23f);
	}

	[Command(channel = 2)]
	private void CmdMovePlayer(GameObject ply, int t)
	{
		if (ServerTime.CheckSynchronization(t) && base.GetComponent<CharacterClassManager>().curClass == 3 && Vector3.Distance(base.GetComponent<PlyMovementSync>().position, ply.transform.position) < 3f)
		{
			base.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(40f, "SCP:106", "SCP:106"), ply);
			this.CallRpcMovePlayer(ply);
		}
	}

	[ClientRpc]
	private void RpcMovePlayer(GameObject ply)
	{
		ply.GetComponent<Scp106PlayerScript>().GotoPD();
	}

	[ClientRpc(channel = 2)]
	public void RpcAnnounceContaining()
	{
		UnityEngine.Object.Instantiate<GameObject>(this.containAnnouncePrefab);
	}

	private void OnTriggerStay(Collider other)
	{
		if (!base.isLocalPlayer || this.ccm.curClass != 3)
		{
			return;
		}
		Door componentInParent = other.GetComponentInParent<Door>();
		if (componentInParent != null)
		{
			this.fpc.m_WalkSpeed = 1f;
			this.fpc.m_RunSpeed = 1f;
			if (componentInParent.isOpen && componentInParent.curCooldown <= 0f)
			{
				this.fpc.m_WalkSpeed = this.ccm.klasy[this.ccm.curClass].walkSpeed;
				this.fpc.m_RunSpeed = this.ccm.klasy[this.ccm.curClass].runSpeed;
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!base.isLocalPlayer || this.ccm.curClass != 3)
		{
			return;
		}
		this.fpc.m_WalkSpeed = this.ccm.klasy[this.ccm.curClass].walkSpeed;
		this.fpc.m_RunSpeed = this.ccm.klasy[this.ccm.curClass].runSpeed;
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdMakePortal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdMakePortal called on client.");
			return;
		}
		((Scp106PlayerScript)obj).CmdMakePortal(reader.ReadGameObject());
	}

	protected static void InvokeCmdCmdMovePlayer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdMovePlayer called on client.");
			return;
		}
		((Scp106PlayerScript)obj).CmdMovePlayer(reader.ReadGameObject(), (int)reader.ReadPackedUInt32());
	}

	public void CallCmdMakePortal(GameObject go)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdMakePortal called on server.");
			return;
		}
		if (base.isServer)
		{
			this.CmdMakePortal(go);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write(0);
		networkWriter.Write((short)((ushort)5));
		networkWriter.WritePackedUInt32((uint)Scp106PlayerScript.kCmdCmdMakePortal);
		networkWriter.Write(base.GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(go);
		base.SendCommandInternal(networkWriter, 4, "CmdMakePortal");
	}

	public void CallCmdMovePlayer(GameObject ply, int t)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdMovePlayer called on server.");
			return;
		}
		if (base.isServer)
		{
			this.CmdMovePlayer(ply, t);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write(0);
		networkWriter.Write((short)((ushort)5));
		networkWriter.WritePackedUInt32((uint)Scp106PlayerScript.kCmdCmdMovePlayer);
		networkWriter.Write(base.GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(ply);
		networkWriter.WritePackedUInt32((uint)t);
		base.SendCommandInternal(networkWriter, 2, "CmdMovePlayer");
	}

	protected static void InvokeRpcRpcMovePlayer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcMovePlayer called on server.");
			return;
		}
		((Scp106PlayerScript)obj).RpcMovePlayer(reader.ReadGameObject());
	}

	protected static void InvokeRpcRpcAnnounceContaining(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAnnounceContaining called on server.");
			return;
		}
		((Scp106PlayerScript)obj).RpcAnnounceContaining();
	}

	public void CallRpcMovePlayer(GameObject ply)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcMovePlayer called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write(0);
		networkWriter.Write((short)((ushort)2));
		networkWriter.WritePackedUInt32((uint)Scp106PlayerScript.kRpcRpcMovePlayer);
		networkWriter.Write(base.GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(ply);
		this.SendRPCInternal(networkWriter, 0, "RpcMovePlayer");
	}

	public void CallRpcAnnounceContaining()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcAnnounceContaining called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write(0);
		networkWriter.Write((short)((ushort)2));
		networkWriter.WritePackedUInt32((uint)Scp106PlayerScript.kRpcRpcAnnounceContaining);
		networkWriter.Write(base.GetComponent<NetworkIdentity>().netId);
		this.SendRPCInternal(networkWriter, 2, "RpcAnnounceContaining");
	}

	static Scp106PlayerScript()
	{
		NetworkBehaviour.RegisterCommandDelegate(typeof(Scp106PlayerScript), Scp106PlayerScript.kCmdCmdMakePortal, new NetworkBehaviour.CmdDelegate(Scp106PlayerScript.InvokeCmdCmdMakePortal));
		Scp106PlayerScript.kCmdCmdMovePlayer = -1259313323;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Scp106PlayerScript), Scp106PlayerScript.kCmdCmdMovePlayer, new NetworkBehaviour.CmdDelegate(Scp106PlayerScript.InvokeCmdCmdMovePlayer));
		Scp106PlayerScript.kRpcRpcMovePlayer = 899430315;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Scp106PlayerScript), Scp106PlayerScript.kRpcRpcMovePlayer, new NetworkBehaviour.CmdDelegate(Scp106PlayerScript.InvokeRpcRpcMovePlayer));
		Scp106PlayerScript.kRpcRpcAnnounceContaining = -1924218768;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Scp106PlayerScript), Scp106PlayerScript.kRpcRpcAnnounceContaining, new NetworkBehaviour.CmdDelegate(Scp106PlayerScript.InvokeRpcRpcAnnounceContaining));
		NetworkCRC.RegisterBehaviour("Scp106PlayerScript", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(this.portalPosition);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1u) != 0u)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(this.portalPosition);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			this.portalPosition = reader.ReadVector3();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			this.SetPortalPosition(reader.ReadVector3());
		}
	}

	[Header("Player Properties")]
	public Camera plyCam;

	public bool iAm106;

	public bool sameClass;

	public float ultimatePoints;

	public float teleportSpeed;

	public GameObject containAnnouncePrefab;

	public GameObject screamsPrefab;

	[Header("Portal")]
	[SyncVar(hook = "SetPortalPosition")]
	public Vector3 portalPosition;

	public GameObject portalPrefab;

	private Vector3 previousPortalPosition;

	private CharacterClassManager ccm;

	private FirstPersonController fpc;

	private GameObject popup106;

	private TextMeshProUGUI highlightedAbilityText;

	private Text pointsText;

	private string highlightedString;

	public int highlightID;

	private Image cooldownImg;

	private float attackCooldown;

	public bool goingViaThePortal;

	private bool isHighlightingPoints;

	private static int kCmdCmdMakePortal = 582440253;

	private static int kCmdCmdMovePlayer;

	private static int kRpcRpcMovePlayer;

	private static int kRpcRpcAnnounceContaining;
}
