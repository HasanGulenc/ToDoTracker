using System.Net.Http.Json;
using TaskManager.Tests.Helpers;
using Xunit;

namespace TaskManager.Tests.Tasks;

public class TaskQueryTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private async Task<HttpClient> NewAuthClient()
    {
        var client = factory.CreateClient();
        var token = await AuthHelper.RegisterAndGetToken(client);
        return client.WithToken(token);
    }

    private static async Task<TaskResponse> CreateTask(HttpClient client, object payload)
    {
        var response = await client.PostAsJsonAsync("/api/tasks", payload);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TaskResponse>())!;
    }

    [Fact]
    public async Task FilterByStatus_ReturnsOnlyMatchingTasks()
    {
        var client = await NewAuthClient();
        await CreateTask(client, new { title = "Todo task", status = 0, priority = 0 });
        await CreateTask(client, new { title = "Done task", status = 2, priority = 0 });

        var response = await client.GetAsync("/api/tasks?status=0");
        var body = await response.Content.ReadFromJsonAsync<TaskListResponse>();

        Assert.Equal(1, body!.Total);
        Assert.All(body.Items, t => Assert.Equal("Todo", t.Status));
    }

    [Fact]
    public async Task FilterByPriority_ReturnsOnlyMatchingTasks()
    {
        var client = await NewAuthClient();
        await CreateTask(client, new { title = "High priority", status = 0, priority = 2 });
        await CreateTask(client, new { title = "Low priority", status = 0, priority = 0 });

        var response = await client.GetAsync("/api/tasks?priority=2");
        var body = await response.Content.ReadFromJsonAsync<TaskListResponse>();

        Assert.Equal(1, body!.Total);
        Assert.All(body.Items, t => Assert.Equal("High", t.Priority));
    }

    [Fact]
    public async Task Pagination_RespectsPageSizeAndPage()
    {
        var client = await NewAuthClient();
        for (var i = 0; i < 5; i++)
            await CreateTask(client, new { title = $"Task {i}", status = 0, priority = 0 });

        var page1 = await (await client.GetAsync("/api/tasks?pageSize=2&page=1"))
            .Content.ReadFromJsonAsync<TaskListResponse>();
        var page2 = await (await client.GetAsync("/api/tasks?pageSize=2&page=2"))
            .Content.ReadFromJsonAsync<TaskListResponse>();

        Assert.Equal(5, page1!.Total);
        Assert.Equal(2, page1.Items.Count());
        Assert.Equal(2, page2!.Items.Count());
    }

    [Fact]
    public async Task DueToday_ReturnsOnlyTodaysTasks()
    {
        var client = await NewAuthClient();
        var today = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)).ToString("yyyy-MM-dd");

        await CreateTask(client, new { title = "Today", dueDate = today, status = 0, priority = 0 });
        await CreateTask(client, new { title = "Yesterday", dueDate = yesterday, status = 0, priority = 0 });
        await CreateTask(client, new { title = "No date", status = 0, priority = 0 });

        var response = await client.GetAsync("/api/tasks/due-today");
        var items = await response.Content.ReadFromJsonAsync<List<TaskResponse>>();

        Assert.Single(items!);
        Assert.Equal("Today", items![0].Title);
    }

    [Fact]
    public async Task SortByDueDate_Ascending_ReturnsSortedTasks()
    {
        var client = await NewAuthClient();
        await CreateTask(client, new { title = "Later", dueDate = "2030-12-31", status = 0, priority = 0 });
        await CreateTask(client, new { title = "Sooner", dueDate = "2027-01-01", status = 0, priority = 0 });

        var response = await client.GetAsync("/api/tasks?sortBy=dueDate&sortDir=asc");
        var body = await response.Content.ReadFromJsonAsync<TaskListResponse>();
        var items = body!.Items.ToList();

        Assert.Equal("Sooner", items[0].Title);
        Assert.Equal("Later", items[1].Title);
    }
}
