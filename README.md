<div align=center>

# MyToolBar (Preview)
<img src="https://raw.githubusercontent.com/TwilightLemon/MyToolBar/refs/heads/master/src/MyToolBar/Resources/icon.ico" width="128" height="128"/>

✨ *为Surface Pro而生的顶部工具栏 支持触控和笔快捷方式* ✨

[![LICENSE](https://img.shields.io/badge/license-GPL%20v3.0-blue.svg?style=flat-square)](https://github.com/TwilightLemon/MyToolBar/blob/master/LICENSE)
![C#](https://img.shields.io/badge/lang-C%23-orange)
![WPF](https://img.shields.io/badge/UI-WPF-b33bb3)
[![GitHub Repo stars](https://img.shields.io/github/stars/TwilightLemon/MyToolBar)](https://github.com/TwilightLemon/MyToolBar/stargazers)
</div>

## 🚀平台
运行于 Windows10 1803~ Windows11  
基于 .NET 8 和 WPF 开发  
现已进入预发布阶段，欢迎体验和反馈！ 
 
## 📝 简介
  这是一个为提升Surface等Windows平板设备体验的全局顶部栏工具，目标是：  
-  实时了解和控制设备运行状态：硬件状态监控和快速进程管理，现由插件`MyToolBar.Plugin.BasicPackage`提供
-  提供常用功能常驻平台、提升平板触控和笔的操作体验: 拓展触控笔菜单、侧边栏、顶部栏手势等交互方式，现由插件`MyToolBar.Plugin.TabletUtils`提供
-  与WindowsUI融为一体
-  轻量级和低功耗  
-  高拓展性和可定制性

当然，此app不仅适配平板，也同样适用于用鼠标操作的桌面设备。
  
  ![Main](https://raw.githubusercontent.com/TwilightLemon/Data/refs/heads/master/MTB_Settings_Main.jpg)
## 😺 功能
### 通过AppBar固定的顶部栏
了解什么是AppBar(Win32 API): [WPF使用AppBar实现窗口停靠，适配缩放、全屏响应和多窗口并列](https://blog.twlmgatito.cn/posts/use-appbar-in-wpf/)
- 左侧:   
    点击"〇"打开主菜单;  
    显示前台窗口Title (搭配无标题栏的应用如edge vscode等使用很香);

- 中部: 外部控制（Outer Control）  
    这里的设计是高亮显示的可交互信息，可通过插件进行拓展。目前在`MyToolBar.Plugin.BasicPackage`中提供了交互式媒体控制和简单的时钟显示插件。

- 右部: "胶囊(Capsules)"交互信息显示区，并可以下滑（或单击）弹出详细信息(PopupWindow)。  
    目前在`MyToolBar.Plugin.BasicPackage`中提供了以下功能：  
   - 硬件监测: 电池、CPU占有率、内存占用、网络状态；PopupWindow为进程管理器提供进程查看和结束、`NT Kernel`级别的进程冻结和压缩功能，终结模式下可以查找挂起进程和焦点窗口进程，以便快速终结。
   - 天气: (我移除了windows小部件 但很需要一个地方来显示天气) 通过GPS定位城市，有搜索、收藏功能，长按(鼠标右键点击)可设置为默认显示城市。
   - 简易时钟显示。

上面的Outer Control和Capsules都是由插件提供的，可以自行在设置页面中启用和调整。
![Plugin](https://raw.githubusercontent.com/TwilightLemon/Data/refs/heads/master/MTB_Settings_Plugin.jpg)

### 平板设备拓展工具 (由`MyToolBar.Plugin.TabletUtils`提供)
现有功能：
- 适用于触摸和笔的菜单：  
在屏幕右上角滑动打开菜单，目前提供截图（调用系统截图工具）和屏幕绘制功能。屏幕绘制功能支持使用触控笔在屏幕上绘制，支持撤销和清除操作，可以使用笔进行书写，使用手指圈画以选择，双指点击以清除或粘贴内容(图像和文本)。
- 左侧侧边栏：  
触摸屏幕左侧边缘（鼠标靠近停留）打开侧边栏，现接入DeepSeek AI Chat

![Tablet](https://raw.githubusercontent.com/TwilightLemon/Data/refs/heads/master/MTB_Settings_Services.jpg)
### 保持与Windows高度融合
- 沉浸模式与全屏应用融为一体
- 支持Dark/Light Mode 跟随系统
- 全局亚克力/云母材质特效
- 支持Modern Standby待机模式和电源优化

### More...

## 📦 如何使用(Preview)
1. 自行编译：  
    - 编译整个项目
    - 将`MyToolBar.Plugin.BasicPackage`和`MyToolBar.Plugin.TabletUtils`的dll文件及其依赖文件放入同名文件夹下，再放入`Plugins`文件夹  
      插件文件结构如下：
      ```
      /MyToolBar.exe运行目录/Plugins/
           MyToolBar.Plugin.BasicPackage/
                MyToolBar.Plugin.BasicPackage.dll 主文件
                MyToolBar.Plugin.BasicPackage.deps.json 依赖文件
                ...其他依赖dll
                /Zh-CN/xxxresource.dll 资源文件
                ...
      ```
2. 到Release页面下载最新版本，解压到任意目录，运行MyToolBar.exe
3. 计划上架Microsoft Store和WinGet

## 版权声明
本应用由 [TwilightLemon (https://blog.twlmgatito.cn) (QQ:2728578956)](https://twlm.space) 开发，Fork请保留原仓库地址和版权信息
使用和传播请遵守GPL协议，详见LICENSE

App内部分矢量图标来自: [Iconfont](https://www.iconfont.cn)  
Weather Api及Icon来自： [和风天气](https://www.qweather.com)


<!--
## MyToolBar.Common API Doc
//TODO: 提供统一的WindowBase实现
//GlobalService提供由主进程注册的全局服务

- WinAPI
  #### 静态类 可直接使用

  #### 注册类 由GlobalService提供接口

- Behaviors
    - [x] BlurWindowBehavior 提供窗口模糊效果和统一的暗亮色模式管理行为
    - [x] DraggableUIBehavior 
    - [x] WindowDragMoveBehavior

- Func
     - [x] HttpHelper
     - [x] ImageHelper

- Styles
     - [x] IconData 提供统一的常用图标资源
     - [x] ThemeColor(_Light) 提供统一的主题颜色资源
     - [x] UITemplate 提供常用控件的模板及样式 
-->
