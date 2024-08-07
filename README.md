# MyToolBar (Beta)
[![LICENSE](https://img.shields.io/badge/license-GPL%20v3.0-blue.svg?style=flat-square)](https://github.com/TwilightLemon/MyToolBar/blob/master/LICENSE)  
### Platform
Run on Windows 10 1803~11  
Powered by .NET 8 on WPF  

目前此App处于测试阶段，功能可能不稳定，作者正在努力提高可定制性。
## 简介
  这是一个为提升Surface等Windows平板设备体验的全局顶部栏工具，目标是：  
-  实时了解和控制设备运行状态
-  提供常用服务适用于平板的快捷方式
-  提升平板触控和笔的操作体验
-  与WindowsUI融为一体
-  轻量级和低功耗  
-  高拓展性和可定制性

当然，此app不仅适配平板，在未来还会同步优化笔记本形态的使用体验。
  
  ![Main](https://github.com/TwilightLemon/Data/blob/master/50c4d49f1bd71f44cd3bec9e4fdf5fd8.jpg?raw=true)
## 功能
### 通过AppBar固定的顶部栏
- 左侧:   
    点击"〇"打开主菜单;  
    显示前台窗口Title (搭配无标题栏的应用如edge vscode等使用很香);  
    从顶部下滑(TouchEvent)可快速启动Task Viewer(相当于按下Win+Tab键)  

- 中部:   
    这里的设计是高亮显示的可交互信息，未来可拓展成为下滑搜索、媒体控制、显示重要信息的区域  
    目前（夹带了点私货）可与LemonApp交互(依旧是Touch) 控制播放，从左到右依次划分为上一曲、播放/暂停、下一曲 以及 显示歌词  在LemonApp->设置->快捷功能->与MyToolBar联动即可

- 右部: 这里是称为"胶囊(Capsules)"的交互信息显示区 依靠统一的GlobalTimer刷新显然数据，并可以下滑弹出详细信息  
   硬件监视: Memory CPU Temperature NetworkSpeed 下滑启动 内存占用TOP20的进程 点击进入详细页: back - KILL - filePath  
   天气: (我移除了windows小部件 但很需要一个地方来显示天气) 通过ip定位城市，有搜索、收藏功能，长按(鼠标右键点击)可设置为默认显示城市

### 适用于手写笔的手势快捷菜单
看了别人家的安卓平板，发现有很多好用的笔手势功能像是右上角划过可以快速进入屏幕绘制，于是我也写了一个。不过目前还处于不完善阶段，见./MyToolBar/PenPackages/目录。  
现有功能：
- 用笔在屏幕右上角向左下滑动，弹出笔菜单（TODO:可拓展功能）  
- 简陋的inkCavans可在屏幕上圈点勾画，WPF原生适配压感、平滑、反转清除，当用手或鼠标双击屏幕时可穿透画板操作其他应用，再次选画笔时可绘制。

### 保持与WindowsUI高度融合
- 支持Dark/Light Mode 跟随系统
- 全局亚克力材质特效
### More...

## 如何使用
编译整个项目，将MyToolBar.Plugin.BasicPackage.dll文件复制到MyToolBar.exe运行目录\\Plugins\\下，在设置页面中启用需要的组件即可。

## 版权声明
本应用由 [Twilight./Lemon(https://twlm.space)(QQ:2728578956)](https://twlm.space) 开发，转载和使用请保留原仓库地址和版权信息
使用和传播请遵守GPL协议，详见LICENSE

App内部分矢量图标来自: [iconfont](https://www.iconfont.cn)  
Weather Api及Icon来自： [和风天气](https://www.qweather.com)
