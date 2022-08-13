namespace Tree.Extensions;

public class TreeNode<T>
{
    public T Node { get; set; }
    public List<TreeNode<T>> Children { get; set; } = new();
}

public static class TreeExtensions
{
    private static void CheckParams<T, K>(List<T> collection, Func<T, K> selector, Func<T, K> parent_selector)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        if (selector == null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        if (parent_selector == null)
        {
            throw new ArgumentNullException(nameof(parent_selector));
        }
    }

    private static IEnumerable<TreeNode<T>> BuildeTree<T, K>(List<TreeNode<T>> tree, List<T> collection, Func<T, K> selector, Func<T, K> parent_selector, bool revers = false, int? depth = default)
    {
        foreach (TreeNode<T> item in tree)
        {
            List<TreeNode<T>> subtree = new();
            
            if (revers)
            {
                subtree = collection.GetParentsAsTree(selector, parent_selector, selector(item.Node), 1);
            }
            else
            {
                subtree = collection.GetChildsAsTree(selector, parent_selector, selector(item.Node), 1);
            }

            yield return new TreeNode<T>
            {
                Node = item.Node,
                Children = depth != null && depth <= 0 ? new() : BuildeTree(subtree, collection, selector, parent_selector, revers, depth == null ? depth : depth - 1).ToList()
            };
        }
    }

    public static List<TreeNode<T>> ToTree<T, K>(this List<T> collection, Func<T, K> selector, Func<T, K> parent_selector, K? root = default, int? depth = default)
    {
        CheckParams(collection, selector, parent_selector);

        List<TreeNode<T>> tree = new();

        if (depth != null && depth <= 0)
        {
            return tree;
        }

        if (root == null)
        {
            tree = collection.GetChildsAsTree(selector, parent_selector, root, 1);
        }
        else
        {
            tree = collection.GetChildsAsTree(selector, selector, root, 1);
        }

        return BuildeTree(tree, collection, selector, parent_selector, revers: false, depth == null ? depth : depth - 1).ToList();
    }

    public static List<TreeNode<T>> ToTreeRevers<T, K>(this List<T> collection, Func<T, K> selector, Func<T, K> parent_selector, K? root = default, int? depth = default)
    {
        CheckParams(collection, selector, parent_selector);

        List<TreeNode<T>> tree = new();

        if (depth != null && depth <= 0)
        {
            return tree;
        }

        if (root == null)
        {
            tree = collection.GetLeaves(selector, parent_selector).ToTree(selector, parent_selector);
        }
        else
        {
            collection.Where(x => EqualityComparer<K>.Default.Equals(selector(x), root))
                .ToList().ForEach(x => tree.Add(new() { Node = x }));
        }

        return BuildeTree(tree, collection, selector, parent_selector, revers: true, depth == null ? depth : depth - 1).ToList();
    }

    public static List<T> GetParents<T, K>(this List<T> collection, Func<T, K> selector, Func<T, K> parent_selector, K? root = default, int? depth = default)
    {
        CheckParams(collection, selector, parent_selector);

        List<T> inner = new();

        if (depth != null && depth <= 0)
        {
            return inner;
        } 

        foreach (T node in collection.Where(x => EqualityComparer<K>.Default.Equals(selector(x),
            parent_selector(collection.First(x => EqualityComparer<K>.Default.Equals(selector(x), root))))))
        {
            inner.Add(node); inner = inner.Union(collection.GetParents(selector, parent_selector, selector(node), depth == null ? depth : depth - 1)).ToList();
        }

        return inner;
    }

    public static List<TreeNode<T>> GetParentsAsTree<T, K>(this List<T> collection, Func<T, K> selector, Func<T, K> parent_selector, K? root = default, int? depth = default)
    {
        CheckParams(collection, selector, parent_selector);

        List<T> inner = new();

        if (depth != null && depth <= 0)
        {
            return inner.ToTree(selector, parent_selector, root);
        }

        foreach (T node in collection.Where(x => EqualityComparer<K>.Default.Equals(selector(x),
            parent_selector(collection.First(x => EqualityComparer<K>.Default.Equals(selector(x), root))))))
        {
            inner.Add(node); inner = inner.Union(collection.GetParents(selector, parent_selector, selector(node), depth == null ? depth : depth - 1)).ToList();
        }

        return inner.ToTree(selector, parent_selector, root); 
    }

    public static List<T> GetChilds<T, K>(this List<T> collection, Func<T, K> selector, Func<T, K> parent_selector, K? root = default, int? depth = default)
    {
        CheckParams(collection, selector, parent_selector);

        List<T> inner = new();

        if (depth != null && depth <= 0)
        {
            return inner;
        }

        foreach (T node in collection.Where(x => EqualityComparer<K>.Default.Equals(parent_selector(x), root)))
        {
            inner.Add(node); inner = inner.Union(collection.GetChilds(selector, parent_selector, selector(node), depth == null ? depth : depth - 1)).ToList();
        }

        return inner;
    }

    public static List<TreeNode<T>> GetChildsAsTree<T, K>(this List<T> collection, Func<T, K> selector, Func<T, K> parent_selector, K? root = default, int? depth = default)
    {
        CheckParams(collection, selector, parent_selector);

        List<T> inner = new();

        if (depth != null && depth <= 0)
        {
            return inner.ToTree(selector, parent_selector, root);
        }

        foreach (T node in collection.Where(x => EqualityComparer<K>.Default.Equals(parent_selector(x), root)))
        {
            inner.Add(node); inner = inner.Union(collection.GetChilds(selector, parent_selector, selector(node), depth == null ? depth : depth - 1)).ToList();
        }

        return inner.ToTree(selector, parent_selector, root);
    }

    public static List<T> GetRoots<T, K>(this List<T> collection, Func<T, K> selector, Func<T, K> parent_selector, K? root = default)
    {
        CheckParams(collection, selector, parent_selector);

        List<T> inner = new();

        if (collection.GetParents(selector, parent_selector, root, 1).Any())
        {
            foreach (T node in collection.GetParents(selector, parent_selector, root, 1))
            {
                inner = inner.Union(collection.GetRoots(selector, parent_selector, selector(node))).ToList();
            }
        }
        else
        {
            T? node = collection.FirstOrDefault(x => EqualityComparer<K>.Default.Equals(selector(x), root));

            if (node is not null)
            {
                inner.Add(node);
            }
        }

        return inner;
    }

    public static List<T> GetLeaves<T, K>(this List<T> collection, Func<T, K> selector, Func<T, K> parent_selector, K? root = default)
    {
        CheckParams(collection, selector, parent_selector);

        List<T> inner = new();

        if (collection.GetChilds(selector, parent_selector, root, 1).Any())
        {
            foreach (T node in collection.GetChilds(selector, parent_selector, root, 1))
            {
                inner = inner.Union(collection.GetLeaves(selector, parent_selector, selector(node))).ToList();
            }
        }
        else
        {
            T? node = collection.FirstOrDefault(x => EqualityComparer<K>.Default.Equals(selector(x), root));

            if (node is not null)
            {
                inner.Add(node);
            }
        }
        
        return inner;
    }
}