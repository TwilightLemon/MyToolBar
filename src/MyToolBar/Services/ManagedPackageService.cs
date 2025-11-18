using Microsoft.Extensions.DependencyInjection;
using MyToolBar.Common;
using MyToolBar.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using static MyToolBar.Common.GlobalService;

namespace MyToolBar.Services;
/// <summary>
/// 托管的插件包管理服务 包括插件加载与设置Sign托管
/// </summary>
public class ManagedPackageService : IManagedSettingsSaver
{
    private static readonly string _packageSettingsSign="ManagedPackageConf",
                                       _packageSettingsName=typeof(ManagedPackageService).FullName;
    private static readonly string _packageDir=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Plugins");
    private readonly SettingsMgr< Dictionary<string,ManagedPkgConf>> _managedPkgConfs=new(_packageSettingsSign,_packageSettingsName);
    private readonly Dictionary<string,ManagedPackage> _managedPkg=[];
    private readonly Dictionary<IPlugin, ManagedPackage> _plugins = [];
    private readonly List<ISettingsMgr> _settingsMgrs = [];
    public Dictionary<string,ManagedPackage> ManagedPkg=>_managedPkg;
    public Dictionary<IPlugin,ManagedPackage> Plugins=>_plugins;
    public static string PackageDirectory=>_packageDir;

    public ManagedPackageService()
    {
        CreateDir();
    }
    ~ManagedPackageService()
    {
        foreach (var managedPkg in _managedPkg)
        {
            managedPkg.Value.LoadContext.Unload();
        }
    }

    private static void CreateDir()
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
    public async Task Load()
    {
        //先读取预先的配置
        await _managedPkgConfs.LoadAsync();
        //
        string[] dirs = Directory.GetDirectories(_packageDir);
        foreach (var dir in dirs)
        {
            SyncFromPackagePath(dir);
        }
        //保存配置文件
        await _managedPkgConfs.SaveAsync();
    }
    public ManagedPkgConf GetManagedPkgConf(string PackageName) => _managedPkgConfs.Data[PackageName];
    public Task SaveManagedPkgConf() => _managedPkgConfs.SaveAsync();

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
            conf.PackageDirectory ??= path;
        }
        else
        {
            //写入配置文件
            _managedPkgConfs.Data.Add(pkgObj.PackageName, new ManagedPkgConf { PackageName = pkgObj.PackageName, IsEnabled = true, PackageDirectory = path });
        }
        //添加到托管包列表
        var loaded = new ManagedPackage(loadContext, pkgObj, isEnable);
        _managedPkg.Add(pkgObj.PackageName, loaded);

        //建立plugin->package的映射关系
        pkgObj.Plugins.ForEach(p => _plugins.Add(p, loaded));

        /*if (!isEnable)
        {//unload之后无法读取基本信息..? 
            loadContext.Unload();
        }*/
    }
    
    public void EnableInRegistered(string packageName)
    {
        if(_managedPkgConfs.Data.TryGetValue(packageName,out var conf))
        {
            conf.IsEnabled=true;
            if(_managedPkg.TryGetValue(packageName,out var pkg))
            {
                //已经加载，只是没有启用
                pkg.IsEnabled = true;
                _=SaveManagedPkgConf();
            }
            else SyncFromPackagePath(conf.PackageDirectory);
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
            //移出UI组件，然后卸载程序集
            await App.Host.Services.GetRequiredService<PluginReactiveService>().UnloadFromPackage(managedPkg.Package);
            managedPkg.LoadContext.Unload();
            //从托管包列表中移除
            _managedPkg.Remove(packageName);
            _managedPkgConfs.Data[packageName].IsEnabled = false;
            await _managedPkgConfs.SaveAsync();
        }
    }

    public bool AddSettingsMgr<T>(ISettingsMgr settingsMgr)
    {
        _settingsMgrs.Add(settingsMgr);
        return true;//?
    }

    public void SaveManagedSettings()
    {
        foreach (var item in _settingsMgrs)
        {
            item.Save();
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
    public string PackageDirectory { get; set; }
    public bool IsEnabled { get; set; } = true;
}
