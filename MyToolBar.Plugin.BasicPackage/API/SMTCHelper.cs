using Windows.Media.Control;

namespace MyToolBar.Plugin.BasicPackage.API;
public class SMTCHelper(GlobalSystemMediaTransportControlsSession session)
{
    private GlobalSystemMediaTransportControlsSession _globalSMTCSession = session;
    public static async Task<SMTCHelper> CreateInstance()
    {
        var gsmtcsm = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
        var smtcHelper = new SMTCHelper(gsmtcsm.GetCurrentSession());
        gsmtcsm.CurrentSessionChanged += (s, e) =>
            smtcHelper.StartSMTCListener(gsmtcsm);
        smtcHelper.StartSMTCListener(gsmtcsm);
        return smtcHelper;
    }
    /// <summary>
    /// 当媒体信息发生变化时触发 例如Title ,Artist,Album等信息变更
    /// </summary>
    public event EventHandler MediaPropertiesChanged;
    /// <summary>
    /// 当媒体播放状态发生变化时触发 例如播放，暂停，停止等状态变更
    /// </summary>
    public event EventHandler PlaybackInfoChanged;
    /// <summary>
    /// 当媒体会话退出时触发
    /// </summary>
    public event EventHandler SessionExited;
    private void StartSMTCListener(GlobalSystemMediaTransportControlsSessionManager mgr)
    {
        bool existed = _globalSMTCSession != null;
        _globalSMTCSession = mgr.GetCurrentSession();
        if (_globalSMTCSession == null)
        {
            if (existed) SessionExited?.Invoke(this, null);
            return;
        }
        //这些事件的e中没有任何有用信息；此外该监听可能会有延迟
        _globalSMTCSession.MediaPropertiesChanged += (s, e) =>
        {
            MediaPropertiesChanged?.Invoke(this, null);
        };
        _globalSMTCSession.PlaybackInfoChanged += (s, e) =>
        {
            PlaybackInfoChanged?.Invoke(this, null);
        };
    }

    public async Task<GlobalSystemMediaTransportControlsSessionMediaProperties?> GetMediaInfoAsync()
    {
        try
        {
            if (_globalSMTCSession == null) return null;
            return await _globalSMTCSession.TryGetMediaPropertiesAsync();
        }
        catch
        {
            return null;
        }
    }

    public GlobalSystemMediaTransportControlsSessionPlaybackStatus? GetPlaybackStatus()
    {
        if (_globalSMTCSession == null) return null;
        return _globalSMTCSession.GetPlaybackInfo().PlaybackStatus;
    }

    public string? GetAppMediaId() => _globalSMTCSession?.SourceAppUserModelId;

    public async Task<bool> PlayOrPause()
    {
        if (_globalSMTCSession == null) return false;
        if (_globalSMTCSession.GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
        {
            await _globalSMTCSession.TryPauseAsync();
            return false;
        }
        else
        {
            await _globalSMTCSession.TryPlayAsync();
            return true;
        }
    }
    public async Task<bool> Previous()
    {
        if (_globalSMTCSession == null) return false;
        return await _globalSMTCSession.TrySkipPreviousAsync();
    }
    public async Task<bool> Next()
    {
        if (_globalSMTCSession == null) return false;
        return await _globalSMTCSession.TrySkipNextAsync();
    }

}
