using com.tvd12.ezyfoxserver.client;
using com.tvd12.ezyfoxserver.client.constant;
using com.tvd12.ezyfoxserver.client.entity;
using com.tvd12.ezyfoxserver.client.request;
using com.tvd12.ezyfoxserver.client.support;
using com.tvd12.ezyfoxserver.client.unity;
using UnityEngine;
using Object = System.Object;

namespace Assambra.Server
{
    public class NetworkManager : EzyAbstractController
    {
        public static NetworkManager Instance { get; private set; }

        private EzySocketConfig socketConfig;


        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            else
                Instance = this;

            socketConfig = GetSocketConfig();
        }

        private new void OnEnable()
        {
            base.OnEnable();
            AddHandler<EzyObject>(Commands.SERVER_STOP, ServerStopRequest);
            AddHandler<EzyObject>(Commands.SERVER_PLAYER_SPAWN, ServerPlayerSpawnRequest);
        }

        private void Update()
        {
            EzyClients.getInstance()
                .getClient(socketConfig.ZoneName)
                .processEvents();
        }

        protected override EzySocketConfig GetSocketConfig()
        {
            return EzySocketConfig.GetBuilder()
                .ZoneName("master-server")
                .AppName("master-server")
                .TcpUrl("127.0.0.1:3005")
                .UdpPort(2611)
                .UdpUsage(true)
                .EnableSSL(false)
                .Build();
        }

        public void Login(string username, string password)
        {
            LOGGER.debug("Login username = " + username + ", password = " + password);
            LOGGER.debug("Socket clientName = " + socketProxy.getClient().getName());

            socketProxy.onLoginSuccess<Object>(HandleLoginSuccess);
            socketProxy.onUdpHandshake<Object>(HandleUdpHandshake);
            socketProxy.onAppAccessed<Object>(HandleAppAccessed);

            // Login to socket server
            socketProxy.setLoginUsername(username);
            socketProxy.setLoginPassword(password);

            socketProxy.setUrl(socketConfig.TcpUrl);
            socketProxy.setUdpPort(socketConfig.UdpPort);
            socketProxy.setDefaultAppName(socketConfig.AppName);
            socketProxy.setTransportType(EzyTransportType.TCP);

            socketProxy.connect();
        }

        public new void Disconnect()
        {
            base.Disconnect();
        }

        private void HandleLoginSuccess(EzySocketProxy proxy, Object data)
        {
            LOGGER.debug("Log in successfully");
        }

        private void HandleUdpHandshake(EzySocketProxy proxy, Object data)
        {
            LOGGER.debug("HandleUdpHandshake");
            socketProxy.send(new EzyAppAccessRequest(socketConfig.AppName));
        }

        private void HandleAppAccessed(EzyAppProxy proxy, Object data)
        {
            LOGGER.debug("App access successfully");
            
            LOGGER.debug("SendServerReady");
            SendServerReady();
        }

        #region SEND

        private void SendServerReady()
        {
            appProxy.send(Commands.SERVER_READY);
        }

        #endregion

        #region RECEIVE

        private void ServerStopRequest(EzyAppProxy proxy, EzyObject data)
        {
            LOGGER.debug("Receive SERVER_STOP request");
            
            Disconnect();
            Application.Quit();
        }

        private void ServerPlayerSpawnRequest(EzyAppProxy proxy, EzyObject data)
        {
            LOGGER.debug("Receive SERVER_PLAYER_SPAWN request");

            string username = data.get<string>("username");
            EzyArray position = data.get<EzyArray>("position");
            EzyArray rotation = data.get<EzyArray>("rotation");
            LOGGER.debug("Username: " + username);
            LOGGER.debug("x: " + position.get<float>(0) + " y: " + position.get<float>(1) + " z: " + position.get<float>(2));
            LOGGER.debug("x: " + rotation.get<float>(0) + " y: " + rotation.get<float>(1) + " z: " + rotation.get<float>(2));
        }

        #endregion
    }
}

