using VeraPdf.Library.Core;

namespace VeraPdf.Library.Tests;

public class DirectoryTests
{
    [Fact]
    public void DirectoryReturnsLookupsAndReadOnlySnapshots()
    {
        var directory = new MapBackedDirectory<int, string>(
            [new(1, "one"), new(2, "two")]);

        Assert.Equal("one", directory.GetItem(1));
        Assert.Null(directory.GetItem(3));
        Assert.Equal(["one", "two"], directory.GetItems());
        Assert.True(directory.GetKeys().SetEquals([1, 2]));
        Assert.Equal(2, directory.Size);
        Assert.False(directory.IsEmpty);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)directory.GetItems())[0] = "changed");
        Assert.True(new MapBackedDirectory<int, string>().IsEmpty);
    }

    [Fact]
    public void RegistrationsReplaceRemoveAndReturnPreviousValues()
    {
        var registry = new MapBackedRegistry<string, int>();

        Assert.Equal(0, registry.RegisterItem("a", 1));
        Assert.Equal(1, registry.RegisterItem("a", 2));
        registry.RegisterItems([new("b", 3)]);
        Assert.Equal(2, registry.PutdateItem("a", 4));
        registry.PutdateItems([new("c", 5)]);
        Assert.Equal(4, registry.UpdateItem("a", 6));
        registry.UpdateItems([new("b", 7), new("c", 8)]);
        registry.RemoveItems(["b"]);

        Assert.Equal(6, registry.GetItem("a"));
        Assert.Equal(8, registry.GetItem("c"));
        Assert.Equal(0, registry.GetItem("b"));
        Assert.Equal(2, registry.Size);
        Assert.Equal(6, registry.RemoveItem("a"));
        Assert.Equal(0, registry.RemoveItem("missing"));
    }

    [Fact]
    public void BulkUpdateValidatesAllKeysBeforeChangingAnyValues()
    {
        var registry = new MapBackedRegistry<int, string>([new(1, "old")]);

        Assert.Throws<KeyNotFoundException>(() =>
            registry.UpdateItems([new(1, "new"), new(2, "missing")]));

        Assert.Equal("old", registry.GetItem(1));
    }

    [Fact]
    public void DirectoryEqualityComparesItemsAndKeys()
    {
        var first = new MapBackedDirectory<int, string>([new(1, "one"), new(2, "two")]);
        var same = new MapBackedDirectory<int, string>([new(1, "one"), new(2, "two")]);
        var different = new MapBackedDirectory<int, string>([new(2, "two"), new(1, "one")]);

        Assert.Equal(first, same);
        Assert.Equal(first.GetHashCode(), same.GetHashCode());
        Assert.NotEqual(first, different);
    }
}
