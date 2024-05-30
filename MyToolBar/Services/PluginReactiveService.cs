using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MyToolBar.Common;
using MyToolBar.Common.UIBases;
using MyToolBar.Plugin;

namespace MyToolBar.Services
{
    /// <summary>
    /// 响应主窗口上的插件变更
    /// </summary>
    public class PluginReactiveService(ManagedPackageService managedPackageService)
    {
        private readonly ManagedPackageService _managedPackageService=managedPackageService;
        private static readonly string _installConfSign="InstalledPluginConf",
                                                    _installConfPkgName=typeof(PluginReactiveService).FullName;
        private SettingsMgr<InstalledPluginConf> _installedConf=new(_installConfSign,_installConfPkgName);
        public IPlugin? OuterControl { get;private set; }
        public Dictionary<IPlugin,CapsuleBase> Capsules { get; private set; } = [];

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
                if(_managedPackageService.ManagedPkg.FirstOrDefault(p=>p.Key==conf.PackageName) is { } package)
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
                    if(_managedPackageService.ManagedPkg.FirstOrDefault(p=>p.Key==capConf.PackageName) is { } package)
                    {
                        var pkg=package.Value.Package;
                        if (pkg.Plugins.FirstOrDefault(p => p.Name == capConf.PluginName) is { Type:PluginType.Capsule} plugin)
                        {
                            AddCapsule(plugin);
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
            if (plugin.GetMainElement() is OuterControlBase oc)
            {
                Debug.Assert(plugin.AcPackage != null);
                _installedConf.Data.OutterControl = new(plugin.AcPackage.PackageName, plugin.Name);
                OuterControl = plugin;
                OuterControlChanged?.Invoke(oc);
                if (saveConf)
                    await _installedConf.Save();
            }
            return true;
        }
        public bool AddCapsule(IPlugin plugin) {
            //不是Capsule or 已存在
            if (plugin.Type != PluginType.Capsule || Capsules.Any(p => p.Key.Name == plugin.Name))
                return false;

            if (plugin.GetMainElement() is CapsuleBase cap)
            {
                cap.Init();
                Capsules.Add(plugin, cap);
                CapsuleAdded?.Invoke(cap);
                return true;
            }
            return false;
        }
        public bool RemoveCapsule(IPlugin plugin)
        {
            if (plugin.Type != PluginType.Capsule)
                return false;

            if(Capsules.FirstOrDefault(p => p.Key.Name == plugin.Name) is var cap)
            {
                CapsuleRemoved?.Invoke(cap.Key);
                Capsules.Remove(cap.Key);
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
    }
}
