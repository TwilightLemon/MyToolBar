using System;
using System.Runtime.InteropServices;
using System.IO;
namespace MyToolBar.WinApi;
public class ExplorerAPI
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern uint SHParseDisplayName([In] string pszName, [In] IntPtr pbc, [Out] out IntPtr ppidl, [In] uint sfgaoIn, [Out] out uint psfgaoOut);

    [DllImport("shell32.dll")]
    public static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, uint dwFlags);

    public static void OpenFolderAndSelectFile(string filePath)
    {
        IntPtr pidl;
        uint psfgaoOut;
        SHParseDisplayName(Path.GetDirectoryName(filePath), IntPtr.Zero, out pidl, 0, out psfgaoOut);

        IntPtr[] pidlArray = new IntPtr[1];
        SHParseDisplayName(filePath, IntPtr.Zero, out pidlArray[0], 0, out psfgaoOut);

        SHOpenFolderAndSelectItems(pidl, (uint)pidlArray.Length, pidlArray, 0);

        Marshal.FreeCoTaskMem(pidl);
        Marshal.FreeCoTaskMem(pidlArray[0]);
    }
}