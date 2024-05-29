using System;
using MyToolBar.Plugin;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using MyToolBar.Common;
using System.IO;
using System.Threading.Tasks;

namespace MyToolBar.Services;
public class PluginService
{
    private static readonly string _packageSettingsSign="ManagedPackageConf",
                                       _packageSettingsName="MyToolBar.PluginService";
    private static readonly string _packageDir=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Plugins");
    private readonly SettingsMgr< Dictionary<string,ManagedPkgConf>> _managedPkgConfs=new(_packageSettingsSign,_packageSettingsName);
    private Dictionary<string,ManagedPackage> _managedPkg=[];
    private FileSystemWatcher _watcher;
    private bool _isLoaded=false;

    public List<string> ManagedSettingsSigns=[];
    public Dictionary<string,ManagedPackage> ManagedPkg=>_managedPkg;
    public PluginService()
    {
        CreateDir();
        InitWatcher();
    }
    ~PluginService()
    {
        _watcher?.Dispose();
        foreach (var managedPkg in _managedPkg)
        {
            managedPkg.Value.LoadContext.Unload();
        }
    }
    private void InitWatcher()
    {
        _watcher = new FileSystemWatcher(_packageDir)
        {
            Filter = "*.dll",
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };
        _watcher.Created += _watcher_Created;
        _watcher.Deleted += _watcher_Deleted;
    }
    private DateTime _lastTime=DateTime.MinValue;
    private void _watcher_Deleted(object sender, FileSystemEventArgs e) {
        if(DateTime.Now-_lastTime<TimeSpan.FromSeconds(1)) return;
        _lastTime=DateTime.Now;
        var m=_managedPkgConfs.Data.FirstOrDefault(kv => kv.Value.FilePath == e.FullPath);
        Unload(m.Key);
    }

    private void _watcher_Created(object sender, FileSystemEventArgs e) {
        if (DateTime.Now - _lastTime < TimeSpan.FromSeconds(1)) return;
        _lastTime = DateTime.Now;
        SyncFromFile(e.FullPath);
    }

    private void CreateDir()
    {
        if(!Directory.Exists(_packageDir))
        {
            Directory.CreateDirectory(_packageDir);
        }
    }
    /// <summary>
    /// 加载所有启用的托管包(用于启动时)
    /// </summary>
    public async void Load()
    {
        //先读取预先的配置
        await _managedPkgConfs.Load();
        //遍历目录下的所有dll文件
        string[] files = Directory.GetFiles(_packageDir, "*.dll");
        foreach (var file in files)
        {
            SyncFromFile(file);
        }
        //保存配置文件
        await _managedPkgConfs.Save();
        _isLoaded=true;
    }
    public async Task WaitForLoading()
    {
        while (!_isLoaded) await Task.Delay(10);
    }
    /// <summary>
    /// 有新的托管包文件时同步
    /// </summary>
    /// <param name="file"></param>
    public void SyncFromFile(string file)
    {
        var loadContext = new AssemblyLoadContext(Guid.NewGuid().ToString(), true);
        var assembly = loadContext.LoadFromAssemblyPath(file);
        var package = assembly.GetTypes().FirstOrDefault(t => typeof(IPackage).IsAssignableFrom(t));
        if (package == null)
        {
            loadContext.Unload();
            return;
        }
        if (Activator.CreateInstance(package) is not IPackage pkgObj)
        {
            loadContext.Unload();
            return;
        }
        //检查是否已加载
        if (_managedPkg.ContainsKey(pkgObj.PackageName))
        {
            loadContext.Unload();
            return;
        }
        //检查配置文件中是否启用 不存在则默认启用并写入配置文件
        if (_managedPkgConfs.Data.TryGetValue(pkgObj.PackageName, out var conf))
        {
            if (!conf.IsEnabled)
            {
                //不启用
                loadContext.Unload();
                return;
            }
        }
        else
        {
            //写入配置文件
            _managedPkgConfs.Data.Add(pkgObj.PackageName, new ManagedPkgConf { PackageName = pkgObj.PackageName, IsEnabled = true,FilePath=file });
        }
        //添加到托管包列表
        _managedPkg.Add(pkgObj.PackageName, new ManagedPackage(loadContext, pkgObj));
        if(pkgObj.Plugins!=null)
        {
            foreach (var plugin in pkgObj.Plugins)
            {
                if (plugin.SettingsSignKeys != null)
                {
                    ManagedSettingsSigns.AddRange(plugin.SettingsSignKeys);
                }
            }
        }
    }
    
    /// <summary>
    /// 从已装载的托管包中卸载
    /// </summary>
    /// <param name="packageName"></param>
    public async void Unload(string packageName)
    {
        if (_managedPkg.TryGetValue(packageName, out var managedPkg))
        {
            //这里暂时不需要移除SettingsSignKeys
            managedPkg.LoadContext.Unload();
            _managedPkg.Remove(packageName);
            _managedPkgConfs.Data[packageName].IsEnabled = false;
            await _managedPkgConfs.Save();
        }
    }
}
/// <summary>
/// 成功加载的托管包 包含已加载的程序集和包对象
/// </summary>
public class ManagedPackage(AssemblyLoadContext loadContext, IPackage package)
{
    public readonly IPackage Package=package;
    public readonly AssemblyLoadContext LoadContext=loadContext;
    public string PackageName => Package.PackageName;
    public bool IsEnabled { get; set; } = true;
}
/// <summary>
/// 写入配置文件的托管包
/// </summary>
public class ManagedPkgConf
{
    public string PackageName { get; set; }
    public string FilePath { get; set; }
    public bool IsEnabled { get; set; } = true;
}
