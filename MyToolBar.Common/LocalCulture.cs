using System;
using System.Collections.Generic;
using System.Globalization;
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
    private static CultureInfo ToCultureInfo(this Language lang)
    {
        return lang switch
        {
            Language.zh_cn => new CultureInfo("zh-CN"),
            Language.en_us => new CultureInfo("en-US"),
            _ => throw new NotSupportedException($"The language {lang} is not supported."),
        };
    }
    public static event EventHandler<Language>? OnLanguageChanged;
    public static Language Current { get; private set; }

    public static void SetGlobalLanguage(Language lang,bool invoke=true)
    {
        Current = lang;
        CultureInfo.DefaultThreadCurrentCulture = lang.ToCultureInfo();
        if(invoke)
        OnLanguageChanged?.Invoke(null, lang);
    }
}
