using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MyToolBar.Common;
using MyToolBar.Common.UIBases;
using MyToolBar.Plugin;

namespace MyToolBar.Services
{
    /// <summary>
    /// 通知和应用主窗口插件的服务
    /// </summary>
    public class PluginReactiveService(ManagedPackageService managedPackageService)
    {
        private readonly ManagedPackageService _managedPackageService=managedPackageService;
        private static readonly string _installConfSign="InstalledPluginConf",
                                                    _installConfPkgName=typeof(PluginReactiveService).FullName;
        private readonly SettingsMgr<InstalledPluginConf> _installedConf=new(_installConfSign,_installConfPkgName);

        public IPlugin? OuterControl { get;private set; }
        public Dictionary<IPlugin,CapsuleBase> Capsules { get; private set; } = [];
        public Dictionary<IPlugin, ServiceBase> UserServices = [];

        public event Action<OuterControlBase>? OuterControlChanged;
        public event Action<CapsuleBase>? CapsuleAdded;
        public event Action<IPlugin>? CapsuleRemoved;

        public async Task Load()
        {
            await _managedPackageService.WaitForLoading();
            await _installedConf.Load();
            //加载OuterControl
            if(_installedConf.Data.OutterControl is { } conf)
            {
                if(_managedPackageService.ManagedPkg.FirstOrDefault(p=>p.Key==conf.PackageName&&p.Value.IsEnabled) is {Key:not null,Value: not null} package)
                {
                    var pkg=package.Value.Package;
                    if (pkg.Plugins.FirstOrDefault(p => p.Name == conf.PluginName) is {Type:PluginType.OuterControl} plugin)
                    { 
                        await SetOuterControl(plugin, false);
                    }
                }
            }
            //加载Capsules
            if (_installedConf.Data.Capsules.Count > 0)
            {
                foreach(var capConf in _installedConf.Data.Capsules)
                {
                    if(_managedPackageService.ManagedPkg.FirstOrDefault(
                        p=>p.Key==capConf.PackageName && p.Value.IsEnabled) is { Key: not null, Value: not null } package)
                    {
                        var pkg=package.Value.Package;
                        if (pkg.Plugins.FirstOrDefault(p => p.Name == capConf.PluginName) is { Type:PluginType.Capsule} plugin)
                        {
                            await AddCapsule(plugin,false);
                        }
                    }
                }
            }
            //加载WindowServices
            if (_installedConf.Data.UserServices.Count > 0)
            {
                foreach (var serviceConf in _installedConf.Data.UserServices)
                {
                    if (_managedPackageService.ManagedPkg.FirstOrDefault(p => p.Key == serviceConf.PackageName && p.Value.IsEnabled) is { Key: not null, Value: not null } package)
                    {
                        var pkg = package.Value.Package;
                        if (pkg.Plugins.FirstOrDefault(p => p.Name == serviceConf.PluginName) is { Type: PluginType.UserService } plugin)
                        {
                            await AddUserService(plugin, false);
                        }
                    }
                }
            }
        }


        private async Task LoadAllForTest(PluginType type)
        {
            foreach (var pkg in _managedPackageService.ManagedPkg)
            {
                if (pkg.Value.IsEnabled)
                {
                    foreach (var plugin in pkg.Value.Package.Plugins)
                    {
                        if(type==plugin.Type) switch (plugin.Type)
                        {
                            case PluginType.OuterControl:
                                await SetOuterControl(plugin, false);
                                break;
                            case PluginType.Capsule:
                                await AddCapsule(plugin, false);
                                break;
                            case PluginType.UserService:
                                await AddUserService(plugin, false);
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 为主窗口设置OuterControl
        /// </summary>
        /// <param name="plugin"></param>
        /// <returns></returns>
        public async Task<bool> SetOuterControl(IPlugin plugin,bool saveConf=true)
        {
            //不是OuterControl or 已存在
            if (plugin.Type != PluginType.OuterControl||plugin.Name==OuterControl?.Name)
                return false;
            if (plugin.GetMainElement() is OuterControlBase{ } oc)
            {
                Debug.Assert(plugin.AcPackage != null);
                OuterControl = plugin;
                OuterControlChanged?.Invoke(oc);
                if (saveConf)
                {
                    _installedConf.Data.OutterControl = new(plugin.AcPackage.PackageName, plugin.Name);
                    await _installedConf.Save();
                }
            }
            return true;
        }

        public async Task<bool> AddUserService(IPlugin plugin, bool saveConf = true)
        {
            //不是UserService or 已存在
            if (plugin.Type != PluginType.UserService || UserServices.Any(p => p.Key.Name == plugin.Name))
                return false;

            if (plugin.GetServiceHost() is ServiceBase { } service)
            {
                UserServices.Add(plugin,service);
                service.IsRunningChanged += Service_IsRunningChanged;
                await service.Start();
                if (saveConf)
                {
                    _installedConf.Data.UserServices.Add(new(plugin.AcPackage.PackageName, plugin.Name));
                    await _installedConf.Save();
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// UserService自行退出时解除引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="isRunning"></param>
        private async void Service_IsRunningChanged(object? sender, bool isRunning)
        {
            Debug.WriteLine(sender + " IsRunning: " + isRunning);

            if (!isRunning&& sender is ServiceBase { } service&&
                UserServices.FirstOrDefault(p => p.Value == service).Key is IPlugin { } plugin)
            {
                service.IsRunningChanged -= Service_IsRunningChanged;
                await RemoveUserService(plugin);
            }
        }

        public async Task<bool> RemoveUserService(IPlugin plugin)
        {
            if (plugin.Type!= PluginType.UserService)
                return false;

            if(UserServices.FirstOrDefault(p=>p.Key.Name==plugin.Name) is { } service &&
                 _installedConf.Data.UserServices.FirstOrDefault(
                     p => p.PluginName == plugin.Name && p.PackageName==plugin.AcPackage.PackageName) is { } conf)
            {
                _installedConf.Data.UserServices.Remove(conf);
                UserServices.Remove(service.Key);
                if(service.Value is {IsRunning:true} w)
                {
                    await w.Stop();
                    w.Dispose();
                }
                await _installedConf.Save();
                return true;
            }
            return false;
        }
        public async Task<bool> AddCapsule(IPlugin plugin,bool saveConf=true) {
            //不是Capsule or 已存在
            if (plugin.Type != PluginType.Capsule || Capsules.Any(p => p.Key.Name == plugin.Name))
                return false;

            if (plugin.GetMainElement() is CapsuleBase{ } cap)
            {
                cap.Install();
                Capsules.Add(plugin, cap);
                CapsuleAdded?.Invoke(cap);
                if (saveConf)
                {
                    _installedConf.Data.Capsules.Add(new(plugin.AcPackage.PackageName, plugin.Name));
                    await _installedConf.Save();
                }
                return true;
            }
            return false;
        }
        public async Task<bool> RemoveCapsule(IPlugin plugin)
        {
            if (plugin.Type != PluginType.Capsule)
                return false;

            if(Capsules.FirstOrDefault(p => p.Key.Name == plugin.Name) is var cap &&
                _installedConf.Data.Capsules.FirstOrDefault(p=>
                p.PluginName==plugin.Name&&p.PackageName==plugin.AcPackage.PackageName) is var conf)
            {
                _installedConf.Data.Capsules.Remove(conf);
                CapsuleRemoved?.Invoke(cap.Key);
                Capsules.Remove(cap.Key);
                await _installedConf.Save();
                return true;
            }
            return false;
        }
    }
    public class InstalledPluginConf
    {
        public record struct InstalledPlugin(string PackageName,string PluginName);
        public InstalledPlugin? OutterControl { get; set; } = null;
        public List<InstalledPlugin> Capsules { get; set; } = [];
        public List<InstalledPlugin> UserServices { get; set; } = [];
    }
}
