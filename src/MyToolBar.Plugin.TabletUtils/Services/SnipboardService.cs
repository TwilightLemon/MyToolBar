using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using MyToolBar.Plugin.TabletUtils.Models;

namespace MyToolBar.Plugin.TabletUtils.Services;

/// <summary>
/// 速记板核心服务 — 文本/图片条目的创建、查询、管理
/// </summary>
public class SnipboardService
{
    private readonly SnipRepository _repo;

    public SnipboardService(SnipRepository repo)
    {
        _repo = repo;
    }

    #region 文本

    /// <summary>
    /// 保存文本条目
    /// </summary>
    public async Task<SnipItem> SaveText(string text, int? tagId = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("文本内容不能为空");

        var item = new SnipItem
        {
            Type = SnipType.Text,
            Content = text.Trim(),
            TagId = tagId
        };
        return await _repo.Insert(item);
    }

    #endregion

    #region 图片

    /// <summary>
    /// 从剪贴板保存图片
    /// </summary>
    public async Task<SnipItem> SaveImageFromClipboard(int? tagId = null)
    {
        if (!Clipboard.ContainsImage())
            throw new InvalidOperationException("剪贴板中没有图片");

        var bitmap = Clipboard.GetImage();
        if (bitmap == null)
            throw new InvalidOperationException("无法读取剪贴板中的图片");

        // 保存到临时文件
        var tempPath = Path.Combine(SnipRepository.ImageDir, $"clip_{Guid.NewGuid():N}.png");
        SaveBitmapToPng(bitmap, tempPath);

        // 复制到存储目录并生成缩略图
        return await SaveImageFileInternal(tempPath, tagId, deleteSource: true);
    }

    /// <summary>
    /// 从文件路径保存图片（拖入、粘贴文件等场景）
    /// </summary>
    public async Task<SnipItem> SaveImageFile(string sourcePath, int? tagId = null)
    {
        if (!File.Exists(sourcePath))
            throw new FileNotFoundException("图片文件不存在", sourcePath);

        return await SaveImageFileInternal(sourcePath, tagId);
    }

    /// <summary>
    /// 内部图片保存流程：复制到存储目录 → 生成缩略图 → 插入数据库
    /// </summary>
    private async Task<SnipItem> SaveImageFileInternal(string sourcePath, int? tagId, bool deleteSource = false)
    {
        // 确定存储路径: Images\{yyyyMM}\{guid}.{ext}
        var now = DateTime.Now;
        var monthDir = now.ToString("yyyyMM");
        var ext = Path.GetExtension(sourcePath).ToLower();
        if (string.IsNullOrEmpty(ext) || ext == ".tmp") ext = ".png";

        var imageRelDir = Path.Combine("Images", monthDir);
        var imageAbsDir = Path.Combine(SnipRepository.DataDir, imageRelDir);
        Directory.CreateDirectory(imageAbsDir);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var destPath = Path.Combine(imageAbsDir, fileName);

        // 复制图片
        File.Copy(sourcePath, destPath, overwrite: true);

        if (deleteSource)
        {
            try { File.Delete(sourcePath); } catch { }
        }

        // 生成缩略图
        var thumbRelPath = GenerateThumbnail(destPath, monthDir);

        // 数据库记录
        var item = new SnipItem
        {
            Type = SnipType.Image,
            ImagePath = Path.Combine(imageRelDir, fileName).Replace('\\', '/'),
            ThumbnailPath = thumbRelPath?.Replace('\\', '/'),
            TagId = tagId
        };
        return await _repo.Insert(item);
    }

    /// <summary>
    /// 生成缩略图，返回相对路径
    /// </summary>
    private string? GenerateThumbnail(string imagePath, string monthDir)
    {
        try
        {
            var thumbRelDir = Path.Combine("Thumbnails", monthDir);
            var thumbAbsDir = Path.Combine(SnipRepository.DataDir, thumbRelDir);
            Directory.CreateDirectory(thumbAbsDir);

            var thumbFileName = $"{Path.GetFileNameWithoutExtension(imagePath)}_thumb.jpg";
            var thumbDest = Path.Combine(thumbAbsDir, thumbFileName);

            // 使用 WPF 内置位图处理（无需 System.Drawing）
            var decoder = BitmapDecoder.Create(
                new Uri(imagePath),
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

            var frame = decoder.Frames[0];
            const int maxWidth = 320;

            if (frame.PixelWidth > maxWidth)
            {
                var scale = maxWidth / (double)frame.PixelWidth;
                var scaledBitmap = new TransformedBitmap(frame, new System.Windows.Media.ScaleTransform(scale, scale));

                var encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = 92;
                encoder.Frames.Add(BitmapFrame.Create(scaledBitmap));

                using var stream = File.OpenWrite(thumbDest);
                encoder.Save(stream);
            }
            else
            {
                // 原图已足够小，直接保存缩略图
                var encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = 92;
                encoder.Frames.Add(BitmapFrame.Create(frame));
                using var stream = File.OpenWrite(thumbDest);
                encoder.Save(stream);
            }

            return Path.Combine(thumbRelDir, thumbFileName).Replace('\\', '/');
        }
        catch
        {
            // 缩略图生成失败不阻塞主流程
            return null;
        }
    }

    /// <summary>
    /// 将 BitmapSource 保存为 PNG
    /// </summary>
    private static void SaveBitmapToPng(BitmapSource bitmap, string path)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.OpenWrite(path);
        encoder.Save(stream);
    }

    /// <summary>
    /// 使用系统关联程序打开图片
    /// </summary>
    public static void OpenImageWithSystem(string relativePath)
    {
        var fullPath = Path.Combine(SnipRepository.DataDir, relativePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("图片文件不存在", fullPath);

        Process.Start(new ProcessStartInfo
        {
            FileName = fullPath,
            UseShellExecute = true
        });
    }

    #endregion

    #region 查询

    public Task<List<SnipItem>> GetTodayItems() => _repo.GetTodayItems();
    public Task<List<SnipItem>> GetByDate(DateTime date) => _repo.GetByDate(date);
    public Task<List<SnipItem>> GetByTag(int tagId) => _repo.GetByTag(tagId);
    public Task<List<SnipItem>> GetTrashItems() => _repo.GetTrashItems();
    public Task<List<SnipItem>> Search(string keyword) => _repo.Search(keyword);
    public Task<SnipItem?> GetById(int id) => _repo.GetById(id);
    public Task<List<DateTime>> GetDatesWithEntries() => _repo.GetDatesWithEntries();

    #endregion

    #region 操作

    public Task SetTag(int itemId, int? tagId) => _repo.SetTag(itemId, tagId);
    public Task SoftDelete(int id) => _repo.SoftDelete(id);
    public Task Restore(int id) => _repo.Restore(id);
    public Task HardDelete(int id) => _repo.HardDelete(id);
    public Task ClearToday() => _repo.ClearToday();
    public Task EmptyTrash() => _repo.EmptyTrash();

    #endregion
}
