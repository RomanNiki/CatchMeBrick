namespace Networking
{
    public enum ConnectStatus
    {
        Undefined,
        Success,               
        ServerFull,               
        GameInProgress,            
        UserRequestedDisconnect,  
        GenericDisconnect,        
        Reconnecting,             
        IncompatibleBuildType,    
        HostEndedSession,         
        StartHostFailed,          
        StartClientFailed,
        LoggedInAgain
    }
}