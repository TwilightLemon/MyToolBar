using MyToolBar.Plugin.TabletUtils.Models;

namespace MyToolBar.Plugin.TabletUtils.Services;

/// <summary>
/// 标签管理服务
/// </summary>
public class TagService
{
    private readonly SnipRepository _repo;

    public TagService(SnipRepository repo)
    {
        _repo = repo;
    }

    public Task<List<Tag>> GetAllTags() => _repo.GetAllTags();
    public Task<Tag?> GetById(int id) => _repo.GetTagById(id);
    public Task<Tag> AddTag(string name, string color) => _repo.AddTag(name, color);
    public Task UpdateTag(Tag tag) => _repo.UpdateTag(tag);
    public Task DeleteTag(int id) => _repo.DeleteTag(id);
}
