using System;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Steamworks;

//[DisallowMultipleComponent]
//[AddComponentMenu("Network/Network Room Player")]
public class SteamRoomPlayer : NetworkBehaviour
{
    [SyncVar]
    public string netPlayerName;
    public string localPlayerName;
    public Texture avatarImage;
    public Texture avatarReadyImage;

    public int tempX, tempY, tempWidth, tempHeight;

    [SyncVar(hook = nameof(HandleSteamIdUpdated))]
    [SerializeField] private ulong steamId;

    protected Callback<AvatarImageLoaded_t> avatarImageLoaded;

    [SerializeField] private RawImage profileImage = null;

    [Tooltip("This flag controls whether the default UI is shown for the room player")]
    public bool showRoomGUI = true;

    [Header("Diagnostics")]

    /// <summary>
    /// Diagnostic flag indicating whether this player is ready for the game to begin.
    /// <para>Invoke CmdChangeReadyState method on the client to set this flag.</para>
    /// <para>When all players are ready to begin, the game will start. This should not be set directly, CmdChangeReadyState should be called on the client to set it on the server.</para>
    /// </summary>
    [Tooltip("Diagnostic flag indicating whether this player is ready for the game to begin")]
    [SyncVar(hook = nameof(ReadyStateChanged))]
    public bool readyToBegin;

    /// <summary>
    /// Diagnostic index of the player, e.g. Player1, Player2, etc.
    /// </summary>
    [Tooltip("Diagnostic index of the player, e.g. Player1, Player2, etc.")]
    [SyncVar(hook = nameof(IndexChanged))]
    public int index;

    public void SetSteamId(ulong steamId)
    {
        this.steamId = steamId;
    }

    [Command]
    public void CmdSetPlayerName(string name)
    {
        netPlayerName = name;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(onAvatarImageLoaded);
    }

    private void onAvatarImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID.m_SteamID != steamId) { return; }
        profileImage.texture = GetSteamImageAsTexture(callback.m_iImage);
    }

    private void HandleSteamIdUpdated(ulong oldSteamId, ulong newSteamId)
    {
        var cSteamId = new CSteamID(newSteamId);
        
        localPlayerName = SteamFriends.GetFriendPersonaName(cSteamId);
        
        int imageId = SteamFriends.GetLargeFriendAvatar(cSteamId);
        
        if (imageId == -1) { return; }
        
        profileImage.texture = GetSteamImageAsTexture(imageId);
    }

    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);

        if (isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }

        return texture;
    }

    /// <summary>
    /// Do not use Start - Override OnStartHost / OnStartClient instead!
    /// </summary>
    public void Start()
    {
        if (isLocalPlayer)
        {
            //localPlayerName = PlayerPrefs.GetString("savedName");
            playerPreferences.singleton.playerName = localPlayerName;
            CmdSetPlayerName(localPlayerName);
        }

        if (NetworkManager.singleton is SteamRoomManager room)
        {
            // NetworkRoomPlayer object must be set to DontDestroyOnLoad along with SteamRoomManager
            // in server and all clients, otherwise it will be respawned in the game scene which would
            // have undesirable effects.
            if (room.dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);

            room.roomSlots.Add(this);

            if (NetworkServer.active)
                room.RecalculateRoomPlayerIndices();

            if (NetworkClient.active)
                room.CallOnClientEnterRoom();
        }
        else Debug.LogError("RoomPlayer could not find a SteamRoomManager. The RoomPlayer requires a SteamRoomManager object to function. Make sure that there is one in the scene.");
    }

    public virtual void OnDisable()
    {
        if (NetworkClient.active && NetworkManager.singleton is SteamRoomManager room)
        {
            // only need to call this on client as server removes it before object is destroyed
            room.roomSlots.Remove(this);

            room.CallOnClientExitRoom();
        }
    }

    #region Commands

    [Command]
    public void CmdChangeReadyState(bool readyState)
    {
        readyToBegin = readyState;
        SteamRoomManager room = NetworkManager.singleton as SteamRoomManager;
        if (room != null)
        {
            room.ReadyStatusChanged();
        }
    }

    #endregion

    #region SyncVar Hooks

    /// <summary>
    /// This is a hook that is invoked on clients when the index changes.
    /// </summary>
    /// <param name="oldIndex">The old index value</param>
    /// <param name="newIndex">The new index value</param>
    public virtual void IndexChanged(int oldIndex, int newIndex) { }

    /// <summary>
    /// This is a hook that is invoked on clients when a RoomPlayer switches between ready or not ready.
    /// <para>This function is called when the a client player calls CmdChangeReadyState.</para>
    /// </summary>
    /// <param name="newReadyState">New Ready State</param>
    public virtual void ReadyStateChanged(bool oldReadyState, bool newReadyState) { }

    #endregion

    #region Room Client Virtuals

    /// <summary>
    /// This is a hook that is invoked on clients for all room player objects when entering the room.
    /// <para>Note: isLocalPlayer is not guaranteed to be set until OnStartLocalPlayer is called.</para>
    /// </summary>
    public virtual void OnClientEnterRoom()
    {
        Debug.Log("player entered! :D");
    }

    /// <summary>
    /// This is a hook that is invoked on clients for all room player objects when exiting the room.
    /// </summary>
    /// 

    public virtual void OnClientExitRoom()
    {
        Debug.Log("player left! :(");
    }

    #endregion

    #region Optional UI

    /// <summary>
    /// Render a UI for the room. Override to provide your own UI
    /// </summary>
    public virtual void OnGUI()
    {
        if (!showRoomGUI)
            return;

        SteamRoomManager room = NetworkManager.singleton as SteamRoomManager;
        if (room)
        {
            if (!room.showRoomGUI)
                return;

            if (!NetworkManager.IsSceneActive(room.RoomScene))
                return;

            DrawPlayerReadyState();
            DrawPlayerReadyButton();
        }
    }

    void DrawPlayerReadyState()
    {
        GUILayout.BeginArea(new Rect(60f + (index * 130), 80, 120, 160));

        var style = new GUIStyle();
        style.fontSize = 30;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;

        //GUILayout.Label($"Player [{index + 1}]");
        GUILayout.Label($"{netPlayerName}", style);

        GUI.DrawTexture(new Rect(0, 160, 120, -120), profileImage.texture);

        if (readyToBegin)
            GUI.DrawTexture(new Rect(0, 40, 120, 120), avatarReadyImage);

        if (((isServer && index > 0) || isServerOnly) && GUILayout.Button("KICK"))
        {
            // This button only shows on the Host for all players other than the Host
            // Host and Players can't remove themselves (stop the client instead)
            // Host can kick a Player this way.
            GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
        }

        GUILayout.EndArea();
    }

    void DrawPlayerReadyButton()
    {
        if (NetworkClient.active && isLocalPlayer)
        {
            GUILayout.BeginArea(new Rect(85, 260, 70, 20));
            //GUI.DrawTexture(new Rect(0, 20, 120, 120), avatarImage);
            if (readyToBegin)
            {
                if (GUILayout.Button("Cancel"))
                    CmdChangeReadyState(false);
            }
            else
            {
                if (GUILayout.Button("Ready?"))
                    CmdChangeReadyState(true);
            }

            GUILayout.EndArea();
        }
    }

    #endregion
}