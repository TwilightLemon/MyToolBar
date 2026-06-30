using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;
using MyToolBar.Plugin.TabletUtils.Models;

namespace MyToolBar.Plugin.TabletUtils.Services;

/// <summary>
/// SQLite 数据访问层 — 负责速记条目的持久化
/// </summary>
public class SnipRepository : IDisposable
{
    private readonly SqliteConnection _db;

    /// <summary>
    /// 数据库文件完整路径
    /// </summary>
    public static readonly string DataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MyToolBar", "Snipboard");

    public static readonly string DbPath = Path.Combine(DataDir, "snipboard.db");
    public static readonly string ImageDir = Path.Combine(DataDir, "Images");
    public static readonly string ThumbDir = Path.Combine(DataDir, "Thumbnails");


    public SnipRepository()
    {
        Directory.CreateDirectory(DataDir);
        Directory.CreateDirectory(ImageDir);
        Directory.CreateDirectory(ThumbDir);

        _db = new SqliteConnection($"Data Source={DbPath}");
        _db.Open();

        // 启用 WAL 模式提升并发性能
        using var walCmd = _db.CreateCommand();
        walCmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA foreign_keys=ON;";
        walCmd.ExecuteNonQuery();

        InitializeDatabase();
    }

    #region 数据库初始化

    private void InitializeDatabase()
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Tags (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                Name        TEXT NOT NULL,
                Color       TEXT NOT NULL DEFAULT '#808080',
                SortOrder   INTEGER DEFAULT 0,
                CreatedAt   DATETIME DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS SnipItems (
                Id            INTEGER PRIMARY KEY AUTOINCREMENT,
                Type          INTEGER NOT NULL,
                Content       TEXT,
                ImagePath     TEXT,
                ThumbnailPath TEXT,
                TagId         INTEGER REFERENCES Tags(Id) ON DELETE SET NULL,
                CreatedAt     DATETIME DEFAULT (datetime('now','localtime')),
                DeletedAt     DATETIME DEFAULT NULL,
                SourceApp     TEXT
            );

            CREATE INDEX IF NOT EXISTS IX_SnipItems_CreatedAt ON SnipItems(CreatedAt);
            CREATE INDEX IF NOT EXISTS IX_SnipItems_TagId ON SnipItems(TagId);
            CREATE INDEX IF NOT EXISTS IX_SnipItems_DeletedAt ON SnipItems(DeletedAt);

            -- FTS5 全文搜索虚拟表（外部内容表模式，利用已有数据避免重复存储）
            CREATE VIRTUAL TABLE IF NOT EXISTS SnipItemsFts USING fts5(
                Content,
                content='SnipItems',
                content_rowid='Id'
            );

            -- 触发器：插入时同步到 FTS
            CREATE TRIGGER IF NOT EXISTS SnipItems_AI AFTER INSERT ON SnipItems BEGIN
                INSERT INTO SnipItemsFts(rowid, Content)
                VALUES (new.Id, new.Content);
            END;

            -- 触发器：删除时同步 FTS
            CREATE TRIGGER IF NOT EXISTS SnipItems_AD AFTER DELETE ON SnipItems BEGIN
                INSERT INTO SnipItemsFts(SnipItemsFts, rowid, Content)
                VALUES ('delete', old.Id, old.Content);
            END;

            -- 触发器：更新时同步 FTS
            CREATE TRIGGER IF NOT EXISTS SnipItems_AU AFTER UPDATE ON SnipItems BEGIN
                INSERT INTO SnipItemsFts(SnipItemsFts, rowid, Content)
                VALUES ('delete', old.Id, old.Content);
                INSERT INTO SnipItemsFts(rowid, Content)
                VALUES (new.Id, new.Content);
            END;
        ";
        cmd.ExecuteNonQuery();

        // 重建 FTS 索引，确保已有数据被索引（外部内容表不会自动索引已有行）
        try
        {
            using var rebuildCmd = _db.CreateCommand();
            rebuildCmd.CommandText = "INSERT INTO SnipItemsFts(SnipItemsFts) VALUES('rebuild')";
            rebuildCmd.ExecuteNonQuery();
        }
        catch { /* FTS5 可能未编译进此 SQLite 版本，忽略 */ }

        SeedDefaultTags();
    }

    /// <summary>
    /// 插入默认标签（仅在 Tags 表为空时）
    /// </summary>
    private void SeedDefaultTags()
    {
        using var checkCmd = _db.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Tags";
        var count = (long)checkCmd.ExecuteScalar()!;
        if (count > 0) return;

        using var seedCmd = _db.CreateCommand();
        seedCmd.CommandText = @"
            INSERT INTO Tags (Name, Color, SortOrder) VALUES
                ('工作', '#4CAF50', 0),
                ('灵感', '#FF9800', 1),
                ('待办', '#2196F3', 2);
        ";
        seedCmd.ExecuteNonQuery();
    }

    #endregion

    #region 标签操作

    public async Task<List<Tag>> GetAllTags()
    {
        var tags = new List<Tag>();
        using var cmd = _db.CreateCommand();
        cmd.CommandText = @"
            SELECT t.*, (SELECT COUNT(*) FROM SnipItems s WHERE s.TagId = t.Id AND s.DeletedAt IS NULL) AS ItemCount
            FROM Tags t
            ORDER BY t.SortOrder, t.Id";
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tags.Add(ReadTag(reader));
        }
        return tags;
    }

    public async Task<Tag?> GetTagById(int id)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "SELECT * FROM Tags WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return ReadTag(reader);
        return null;
    }

    public async Task<Tag> AddTag(string name, string color)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Tags (Name, Color, SortOrder)
            VALUES (@name, @color, (SELECT IFNULL(MAX(SortOrder), 0) + 1 FROM Tags));
            SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@color", color);
        var id = (long)await cmd.ExecuteScalarAsync()!;
        return (await GetTagById((int)id))!;
    }

    public async Task UpdateTag(Tag tag)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "UPDATE Tags SET Name = @name, Color = @color, SortOrder = @order WHERE Id = @id";
        cmd.Parameters.AddWithValue("@name", tag.Name);
        cmd.Parameters.AddWithValue("@color", tag.Color);
        cmd.Parameters.AddWithValue("@order", tag.SortOrder);
        cmd.Parameters.AddWithValue("@id", tag.Id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteTag(int id)
    {
        using var cmd = _db.CreateCommand();
        // 先将使用该标签的条目设为 NULL
        cmd.CommandText = "UPDATE SnipItems SET TagId = NULL WHERE TagId = @id";
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();

        // 再删除标签
        using var delCmd = _db.CreateCommand();
        delCmd.CommandText = "DELETE FROM Tags WHERE Id = @id";
        delCmd.Parameters.AddWithValue("@id", id);
        await delCmd.ExecuteNonQueryAsync();
    }

    #endregion

    #region 条目查询

    /// <summary>
    /// 获取今日所有未删除的条目
    /// </summary>
    public async Task<List<SnipItem>> GetTodayItems()
    {
        return await GetByDate(DateTime.Today);
    }

    /// <summary>
    /// 获取指定日期未删除的条目
    /// </summary>
    public async Task<List<SnipItem>> GetByDate(DateTime date)
    {
        var items = new List<SnipItem>();
        using var cmd = _db.CreateCommand();
        cmd.CommandText = @"
            SELECT s.*, t.Id AS Tag_Id, t.Name AS Tag_Name, t.Color AS Tag_Color
            FROM SnipItems s
            LEFT JOIN Tags t ON s.TagId = t.Id
            WHERE date(s.CreatedAt) = @date AND s.DeletedAt IS NULL
            ORDER BY s.CreatedAt DESC";
        cmd.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(ReadSnipItemWithTag(reader));
        }
        return items;
    }

    /// <summary>
    /// 获取指定日期范围内的未删除条目
    /// </summary>
    public async Task<List<SnipItem>> GetByDateRange(DateTime from, DateTime to)
    {
        var items = new List<SnipItem>();
        using var cmd = _db.CreateCommand();
        cmd.CommandText = @"
            SELECT s.*, t.Id AS Tag_Id, t.Name AS Tag_Name, t.Color AS Tag_Color
            FROM SnipItems s
            LEFT JOIN Tags t ON s.TagId = t.Id
            WHERE date(s.CreatedAt) BETWEEN @from AND @to AND s.DeletedAt IS NULL
            ORDER BY s.CreatedAt DESC";
        cmd.Parameters.AddWithValue("@from", from.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@to", to.ToString("yyyy-MM-dd"));
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(ReadSnipItemWithTag(reader));
        }
        return items;
    }

    /// <summary>
    /// 按标签筛选未删除条目
    /// </summary>
    public async Task<List<SnipItem>> GetByTag(int tagId)
    {
        var items = new List<SnipItem>();
        using var cmd = _db.CreateCommand();
        cmd.CommandText = @"
            SELECT s.*, t.Id AS Tag_Id, t.Name AS Tag_Name, t.Color AS Tag_Color
            FROM SnipItems s
            LEFT JOIN Tags t ON s.TagId = t.Id
            WHERE s.TagId = @tagId AND s.DeletedAt IS NULL
            ORDER BY s.CreatedAt DESC";
        cmd.Parameters.AddWithValue("@tagId", tagId);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(ReadSnipItemWithTag(reader));
        }
        return items;
    }

    /// <summary>
    /// 回收站：获取所有已删除条目
    /// </summary>
    public async Task<List<SnipItem>> GetTrashItems()
    {
        var items = new List<SnipItem>();
        using var cmd = _db.CreateCommand();
        cmd.CommandText = @"
            SELECT s.*, t.Id AS Tag_Id, t.Name AS Tag_Name, t.Color AS Tag_Color
            FROM SnipItems s
            LEFT JOIN Tags t ON s.TagId = t.Id
            WHERE s.DeletedAt IS NOT NULL
            ORDER BY s.DeletedAt DESC";
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(ReadSnipItemWithTag(reader));
        }
        return items;
    }

    /// <summary>
    /// FTS5 全文搜索（仅搜索未删除条目），FTS 不可用时回退到 LIKE
    /// </summary>
    public async Task<List<SnipItem>> Search(string keyword)
    {
        var items = new List<SnipItem>();

        try
        {
            // 构建 FTS5 前缀查询：每个词追加 * 实现前缀匹配，多词用 AND 连接
            var terms = keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var ftsQuery = string.Join(" AND ", terms.Select(t =>
            {
                var escaped = t.Replace("\"", "\"\"");
                return $"\"{escaped}\"*";
            }));

            using var cmd = _db.CreateCommand();
            cmd.CommandText = @"
                SELECT s.*, t.Id AS Tag_Id, t.Name AS Tag_Name, t.Color AS Tag_Color
                FROM SnipItemsFts f
                JOIN SnipItems s ON f.rowid = s.Id
                LEFT JOIN Tags t ON s.TagId = t.Id
                WHERE SnipItemsFts MATCH @keyword AND s.DeletedAt IS NULL
                ORDER BY s.CreatedAt DESC";
            cmd.Parameters.AddWithValue("@keyword", ftsQuery);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(ReadSnipItemWithTag(reader));
            }
        }
        catch
        {
            // FTS 不可用（SQLite 未编译 FTS5 或索引损坏）→ 回退到 LIKE
            items = await SearchByLike(keyword);
        }

        return items;
    }

    /// <summary>
    /// LIKE 回退搜索
    /// </summary>
    private async Task<List<SnipItem>> SearchByLike(string keyword)
    {
        var items = new List<SnipItem>();
        using var cmd = _db.CreateCommand();
        cmd.CommandText = @"
            SELECT s.*, t.Id AS Tag_Id, t.Name AS Tag_Name, t.Color AS Tag_Color
            FROM SnipItems s
            LEFT JOIN Tags t ON s.TagId = t.Id
            WHERE s.Content LIKE @keyword AND s.DeletedAt IS NULL AND s.Type = 0
            ORDER BY s.CreatedAt DESC";
        cmd.Parameters.AddWithValue("@keyword", $"%{keyword}%");
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(ReadSnipItemWithTag(reader));
        }
        return items;
    }

    /// <summary>
    /// 获取单个条目（含标签）
    /// </summary>
    public async Task<SnipItem?> GetById(int id)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = @"
            SELECT s.*, t.Id AS Tag_Id, t.Name AS Tag_Name, t.Color AS Tag_Color
            FROM SnipItems s
            LEFT JOIN Tags t ON s.TagId = t.Id
            WHERE s.Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return ReadSnipItemWithTag(reader);
        return null;
    }

    /// <summary>
    /// 获取所有有数据的日期列表（用于日期选择器标记）
    /// </summary>
    public async Task<List<DateTime>> GetDatesWithEntries()
    {
        var dates = new List<DateTime>();
        using var cmd = _db.CreateCommand();
        cmd.CommandText = @"
            SELECT DISTINCT date(CreatedAt) FROM SnipItems
            WHERE DeletedAt IS NULL
            ORDER BY date(CreatedAt) DESC";
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            if (DateTime.TryParse(reader.GetString(0), out var d))
                dates.Add(d);
        }
        return dates;
    }

    #endregion

    #region 条目写入

    /// <summary>
    /// 插入新条目，返回带 Id 的对象
    /// </summary>
    public async Task<SnipItem> Insert(SnipItem item)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO SnipItems (Type, Content, ImagePath, ThumbnailPath, TagId, SourceApp)
            VALUES (@type, @content, @imagePath, @thumbnailPath, @tagId, @sourceApp);
            SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("@type", (int)item.Type);
        cmd.Parameters.AddWithValue("@content", (object?)item.Content ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@imagePath", (object?)item.ImagePath ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@thumbnailPath", (object?)item.ThumbnailPath ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@tagId", (object?)item.TagId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@sourceApp", (object?)item.SourceApp ?? DBNull.Value);

        var id = (long)await cmd.ExecuteScalarAsync()!;
        item.Id = (int)id;
        item.CreatedAt = DateTime.Now;
        return item;
    }

    /// <summary>
    /// 更新条目标签
    /// </summary>
    public async Task SetTag(int itemId, int? tagId)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "UPDATE SnipItems SET TagId = @tagId WHERE Id = @id";
        cmd.Parameters.AddWithValue("@tagId", (object?)tagId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id", itemId);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 软删除：设置 DeletedAt = 当前时间
    /// </summary>
    public async Task SoftDelete(int id)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "UPDATE SnipItems SET DeletedAt = datetime('now','localtime') WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 从回收站恢复
    /// </summary>
    public async Task Restore(int id)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "UPDATE SnipItems SET DeletedAt = NULL WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 硬删除：删除条目及其关联文件
    /// </summary>
    public async Task HardDelete(int id)
    {
        // 先取出条目以获取文件路径
        var item = await GetById(id);
        if (item == null) return;

        // 删除关联文件
        if (!string.IsNullOrEmpty(item.ImagePath))
        {
            var fullPath = Path.Combine(DataDir, item.ImagePath);
            try { if (File.Exists(fullPath)) File.Delete(fullPath); } catch { }
        }
        if (!string.IsNullOrEmpty(item.ThumbnailPath))
        {
            var fullPath = Path.Combine(DataDir, item.ThumbnailPath);
            try { if (File.Exists(fullPath)) File.Delete(fullPath); } catch { }
        }

        // 删除数据库记录
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "DELETE FROM SnipItems WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 软删除今天所有条目
    /// </summary>
    public async Task ClearToday()
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = @"
            UPDATE SnipItems SET DeletedAt = datetime('now','localtime')
            WHERE date(CreatedAt) = date('now','localtime') AND DeletedAt IS NULL";
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 清空回收站：硬删除所有已标记删除的条目
    /// </summary>
    public async Task EmptyTrash()
    {
        // 先获取所有回收站条目以清理文件
        var trashItems = await GetTrashItems();
        foreach (var item in trashItems)
        {
            if (!string.IsNullOrEmpty(item.ImagePath))
            {
                var fullPath = Path.Combine(DataDir, item.ImagePath);
                try { if (File.Exists(fullPath)) File.Delete(fullPath); } catch { }
            }
            if (!string.IsNullOrEmpty(item.ThumbnailPath))
            {
                var fullPath = Path.Combine(DataDir, item.ThumbnailPath);
                try { if (File.Exists(fullPath)) File.Delete(fullPath); } catch { }
            }
        }

        using var cmd = _db.CreateCommand();
        cmd.CommandText = "DELETE FROM SnipItems WHERE DeletedAt IS NOT NULL";
        await cmd.ExecuteNonQueryAsync();
    }

    #endregion

    #region 辅助方法

    private static Tag ReadTag(SqliteDataReader reader)
    {
        return new Tag
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Color = reader.GetString(reader.GetOrdinal("Color")),
            SortOrder = reader.GetInt32(reader.GetOrdinal("SortOrder")),
            CreatedAt = TryGetDateTime(reader, "CreatedAt") ?? DateTime.MinValue,
            ItemCount = TryGetInt32(reader, "ItemCount")
        };
    }

    private static SnipItem ReadSnipItemWithTag(SqliteDataReader reader)
    {
        var item = new SnipItem
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Type = (SnipType)reader.GetInt32(reader.GetOrdinal("Type")),
            Content = TryGetString(reader, "Content"),
            ImagePath = TryGetString(reader, "ImagePath"),
            ThumbnailPath = TryGetString(reader, "ThumbnailPath"),
            TagId = TryGetInt32(reader, "TagId"),
            CreatedAt = TryGetDateTime(reader, "CreatedAt") ?? DateTime.MinValue,
            DeletedAt = TryGetDateTime(reader, "DeletedAt"),
            SourceApp = TryGetString(reader, "SourceApp")
        };

        // 尝试读取关联标签
        try
        {
            var tagId = TryGetInt32(reader, "Tag_Id");
            if (tagId.HasValue)
            {
                item.Tag = new Tag
                {
                    Id = tagId.Value,
                    Name = reader.GetString(reader.GetOrdinal("Tag_Name")),
                    Color = reader.GetString(reader.GetOrdinal("Tag_Color"))
                };
            }
        }
        catch { /* 别名列不存在时忽略 */ }

        return item;
    }

    private static string? TryGetString(SqliteDataReader reader, string name)
    {
        try
        {
            var ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }
        catch { return null; }
    }

    private static int? TryGetInt32(SqliteDataReader reader, string name)
    {
        try
        {
            var ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
        }
        catch { return null; }
    }

    private static DateTime? TryGetDateTime(SqliteDataReader reader, string name)
    {
        try
        {
            var ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
        }
        catch { return null; }
    }

    #endregion

    public void Dispose()
    {
        _db?.Close();
        _db?.Dispose();
    }
}
