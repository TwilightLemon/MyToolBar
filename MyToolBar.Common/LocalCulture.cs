using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyToolBar.Common;
public static class LocalCulture
{
    /// <summary>
    /// 受支持的语言
    /// </summary>
    public enum Language
    {
        zh_cn,en_us
    }
    public static event EventHandler<Language>? OnLanguageChanged;
    public static Language Current { get; private set; }

    public static void SetGlobalLanguage(Language lang,bool invoke=true)
    {
        Current = lang;
        if(invoke)
        OnLanguageChanged?.Invoke(null, lang);
    }
}
