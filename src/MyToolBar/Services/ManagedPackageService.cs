using System;
using MyToolBar.Plugin;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using MyToolBar.Common;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

/*
 TODO: Plugin 加载模式改为从文件夹中加载；
             新增方法：解包和验证
 */
namespace MyToolBar.Services;
/// <summary>
/// 托管的插件包管理服务 包括插件加载与设置Sign托管
/// </summary>
public class ManagedPackageService
{
    private static readonly string _packageSettingsSign="ManagedPackageConf",
                                       _packageSettingsName=typeof(ManagedPackageService).FullName;
    private static readonly string _packageDir=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Plugins");
    private readonly SettingsMgr< Dictionary<string,ManagedPkgConf>> _managedPkgConfs=new(_packageSettingsSign,_packageSettingsName);
    private readonly Dictionary<string,ManagedPackage> _managedPkg=[];
    private FileSystemWatcher? _watcher;
    private bool _isLoaded=false;
    public Dictionary<string,ManagedPackage> ManagedPkg=>_managedPkg;

    public ManagedPackageService()
    {
        CreateDir();
        InitWatcher();
    }
    ~ManagedPackageService()
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
            NotifyFilter = NotifyFilters.DirectoryName,
            EnableRaisingEvents = true
        };
        _watcher.Changed += _watcher_Created;
        _watcher.Created += _watcher_Created;
    }
    private DateTime _lastTime=DateTime.MinValue;
    private void _watcher_Created(object sender, FileSystemEventArgs e) {
        if (DateTime.Now - _lastTime < TimeSpan.FromSeconds(1)) return;
        _lastTime = DateTime.Now;
        SyncFromPackagePath(e.FullPath);
    }

    private void CreateDir()
    {
        if(!Directory.Exists(_packageDir))
        {
            Directory.CreateDirectory(_packageDir);
        }
    }

    /// <summary>
    /// 已启用的托管包中可用的特定类型的插件
    /// </summary>
    /// <param name="t">插件类型</param>
    /// <returns></returns>
    public List<IPlugin> GetTypePlugins(PluginType t)
    {
        var e = new List<IPlugin>();
        var ava = _managedPkg.Values.Where(p => p.IsEnabled);
        foreach (var p in ava)
        {
            e.AddRange(p.Package.Plugins.Where(a => a.Type == t));
        }
        return e;
    }

    /// <summary>
    /// 加载所有启用的托管包(App启动时调用)
    /// </summary>
    public async void Load()
    {
        //先读取预先的配置
        await _managedPkgConfs.Load();
        //
        string[] dirs = Directory.GetDirectories(_packageDir);
        foreach (var dir in dirs)
        {
            SyncFromPackagePath(dir);
        }
        //保存配置文件
        await _managedPkgConfs.Save();
        _isLoaded=true;
    }
    /// <summary>
    /// 等待托管包加载完成
    /// </summary>
    /// <returns></returns>
    public async Task WaitForLoading()
    {
        while (!_isLoaded) await Task.Delay(10);
    }
    /// <summary>
    /// 有新的托管包文件时同步
    /// </summary>
    /// <param name="path"></param>
    public void SyncFromPackagePath(string path)
    {
        //check if it is a valid package
        string packageName = new DirectoryInfo(path).Name;
        string mainFile = Path.Combine(path, packageName+".dll");
        if (!File.Exists(mainFile)) return;

        var loadContext = new AssemblyLoadContext(Guid.NewGuid().ToString(), true);
        var resolver = new AssemblyDependencyResolver(mainFile);
        loadContext.Resolving += (context, assemblyName) =>
        {
            string? dependencyPath = resolver.ResolveAssemblyToPath(assemblyName);
            if (dependencyPath != null)
            {
                return context.LoadFromAssemblyPath(dependencyPath);
            }
            return null;
        };
        //load from mainFile
        var assembly = loadContext.LoadFromStream(File.OpenRead(mainFile));
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
        //不启用也要先加载程序集 读取配置信息之后卸载
        bool isEnable=true;
        if (_managedPkgConfs.Data.TryGetValue(pkgObj.PackageName, out var conf))
        {
            if (!conf.IsEnabled)
            {
                //不启用
                isEnable=false;
            }
        }
        else
        {
            //写入配置文件
            _managedPkgConfs.Data.Add(pkgObj.PackageName, new ManagedPkgConf { PackageName = pkgObj.PackageName, IsEnabled = true,FilePath=mainFile });
        }
        //添加到托管包列表
        _managedPkg.Add(pkgObj.PackageName, new ManagedPackage(loadContext, pkgObj,isEnable));
        pkgObj.Plugins?.ForEach(o => o.AcPackage=pkgObj);

        /*if (!isEnable)
        {//unload之后无法读取基本信息..? 
            loadContext.Unload();
        }*/
    }
    
    public async void EnableInRegistered(string packageName)
    {
        if(_managedPkgConfs.Data.TryGetValue(packageName,out var conf))
        {
            conf.IsEnabled=true;
            if(_managedPkg.TryGetValue(packageName,out var managedPkg))
            {
                managedPkg.IsEnabled=true;
                _managedPkg[packageName]=managedPkg;
                await _managedPkgConfs.Save();
            }
        }
    }
    /// <summary>
    /// 从已装载的托管包中卸载
    /// </summary>
    /// <param name="packageName"></param>
    public async void UnloadFromRegistered(string packageName)
    {
        if (_managedPkg.TryGetValue(packageName, out var managedPkg))
        {
            managedPkg.LoadContext.Unload();
            managedPkg.IsEnabled= false;
            _managedPkg[packageName] = managedPkg;
            _managedPkgConfs.Data[packageName].IsEnabled = false;
            await _managedPkgConfs.Save();
        }
    }
}
/// <summary>
/// 成功加载的托管包 包含已加载的程序集和包对象
/// </summary>
public class ManagedPackage(AssemblyLoadContext loadContext, IPackage package,bool isEnable=true)
{
    public readonly IPackage Package=package;
    public readonly AssemblyLoadContext LoadContext=loadContext;
    public string PackageName => Package.PackageName;
    public bool IsEnabled { get; set; } = isEnable;
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
