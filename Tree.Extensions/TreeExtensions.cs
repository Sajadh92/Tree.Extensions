namespace Tree.Extensions;

public class TreeItem<T>
{
    public T Item { get; set; }
    public List<TreeItem<T>> Children { get; set; } = new();
}

public static class TreeExtensions
{
    public static List<TreeItem<T>> Tree<T, K>(this List<T> collection,
        Func<T, K> selector, Func<T, K> parent_selector, K? root = default)
    {
        List<TreeItem<T>> tree = new();

        if (root == null)
        {
            collection.Where(x => EqualityComparer<K>.Default.Equals(parent_selector(x), root))
                .ToList().ForEach(x => tree.Add(new() { Item = x }));
        }
        else
        {
            collection.Where(x => EqualityComparer<K>.Default.Equals(selector(x), root))
                .ToList().ForEach(x => tree.Add(new() { Item = x }));
        }

        return Builde(tree, collection, selector, parent_selector).ToList();
    }

    private static IEnumerable<TreeItem<T>> Builde<T, K>(List<TreeItem<T>> tree,
        List<T> collection, Func<T, K> selector, Func<T, K> parent_selector)
    {
        foreach (TreeItem<T> node in tree)
        {
            List<TreeItem<T>> subtree = new();

            collection.Where(x => EqualityComparer<K>.Default.Equals(parent_selector(x), selector(node.Item)))
                .ToList().ForEach(x => subtree.Add(new() { Item = x }));

            yield return new TreeItem<T>
            {
                Item = node.Item,
                Children = Builde(subtree, collection, selector, parent_selector).ToList()
            };
        }
    }

    public static List<T> Parents<T, K>(this List<T> collection,
        Func<T, K> selector, Func<T, K> parent_selector, K? root = default,
        int? depth = default)
    {
        List<T> inner = new();

        if (depth != null && depth <= 0)
        {
            return inner;
        }

        foreach (T item in collection.Where(x => EqualityComparer<K>.Default.Equals(selector(x),
            parent_selector(collection.First(x => EqualityComparer<K>.Default.Equals(selector(x), root))))))
        {
            inner.Add(item);

            inner = inner.Union(collection.Parents(selector, parent_selector, selector(item), depth - 1)).ToList();
        }

        return inner;
    }

    public static List<T> Childs<T, K>(this List<T> collection, 
        Func<T, K> selector, Func<T, K> parent_selector, K? root = default, 
        int? depth = default)
    {
        List<T> inner = new();

        if (depth != null && depth <= 0)
        {
            return inner;
        }

        foreach (T item in collection.Where(x => EqualityComparer<K>.Default.Equals(parent_selector(x), root)))
        {
            inner.Add(item);

            inner = inner.Union(collection.Childs(selector, parent_selector, selector(item), depth - 1)).ToList();
        }

        return inner;
    }

    public static List<T> Roots<T, K>(this List<T> collection,
        Func<T, K> selector, Func<T, K> parent_selector, K? root = default)
    {
        List<T> inner = new();

        if (collection.Parents(selector, parent_selector, root, 1).Any())
        {
            foreach (T item in collection.Parents(selector, parent_selector, root, 1))
            {
                inner = inner.Union(collection.Roots(selector, parent_selector, selector(item))).ToList();
            }
        }
        else
        {
            inner.AddRange(collection.Where(x => EqualityComparer<K>.Default.Equals(selector(x), root)).ToList());
        }

        return inner;
    }

    public static List<T> Leaves<T, K>(this List<T> collection,
        Func<T, K> selector, Func<T, K> parent_selector, K? root = default)
    {
        List<T> inner = new();

        if (collection.Childs(selector, parent_selector, root, 1).Any())
        {
            foreach (T item in collection.Childs(selector, parent_selector, root, 1))
            {
                inner = inner.Union(collection.Leaves(selector, parent_selector, selector(item))).ToList();
            }
        }
        else
        {
            inner.AddRange(collection.Where(x => EqualityComparer<K>.Default.Equals(selector(x), root)).ToList());
        }
        
        return inner;
    }
}