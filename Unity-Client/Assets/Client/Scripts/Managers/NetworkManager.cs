using com.tvd12.ezyfoxserver.client;
using com.tvd12.ezyfoxserver.client.constant;
using com.tvd12.ezyfoxserver.client.entity;
using com.tvd12.ezyfoxserver.client.factory;
using com.tvd12.ezyfoxserver.client.request;
using com.tvd12.ezyfoxserver.client.support;
using com.tvd12.ezyfoxserver.client.unity;
using Object = System.Object;

namespace Assambra.Client
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

        public new void Disconnect()
        {
            base.Disconnect();
        }

        public void Login(string username, string password)
        {
            LOGGER.debug("Login username = " + username + ", password = " + password);
            LOGGER.debug("Socket clientName = " + socketProxy.getClient().getName());

            socketProxy.onLoginSuccess<Object>(LoginSuccessResponse);
            socketProxy.onUdpHandshake<Object>(UdpHandshakeResponse);
            socketProxy.onAppAccessed<Object>(AppAccessedResponse);

            // Login to socket server
            socketProxy.setLoginUsername(username);
            socketProxy.setLoginPassword(password);

            socketProxy.setUrl(socketConfig.TcpUrl);
            socketProxy.setUdpPort(socketConfig.UdpPort);
            socketProxy.setDefaultAppName(socketConfig.AppName);
            socketProxy.setTransportType(EzyTransportType.UDP);

            socketProxy.connect();
        }

        #region REQUEST

        public void CharacterListRequest()
        {
            appProxy.send(Commands.CHARACTER_LIST);
        }

        public void CreateCharacterRequest(string name, long model)
        {
            EzyObject characterdata = EzyEntityFactory
                .newObjectBuilder()
                .append("name", name)
                .append("model", model)
                .build();

            appProxy.send(Commands.CREATE_CHARACTER, characterdata);
        }

        public void PlayRequest(long characterId)
        {
            EzyObject data = EzyEntityFactory
                .newObjectBuilder()
                .append("characterId", characterId)
                .build();

            appProxy.send(Commands.PLAY, data);
        }

        #endregion

        #region RESPONSE

        private void LoginSuccessResponse(EzySocketProxy proxy, Object data)
        {
            LOGGER.debug("Log in successfully");
        }

        private void UdpHandshakeResponse(EzySocketProxy proxy, Object data)
        {
            LOGGER.debug("HandleUdpHandshake");
            socketProxy.send(new EzyAppAccessRequest(socketConfig.AppName));
        }

        private void AppAccessedResponse(EzyAppProxy proxy, Object data)
        {
            LOGGER.debug("App access successfully");
            //PlayRequest();
        }

        #endregion
    }
}
