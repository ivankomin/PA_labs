using System.IO;
using System.Text.Json;
namespace pa_lab4;
public class BTreeNode
{
    public List<int> Keys { get; set; } = new List<int>();
    public List<string> Values { get; set; } = new List<string>();
    public List<BTreeNode> Children { get; set; } = new List<BTreeNode>();
    public bool IsLeaf { get; set; }

    public BTreeNode(bool isLeaf)
    {
        IsLeaf = isLeaf;
    }

    public BTreeNode() { }
}

public class BTree
{
    public BTreeNode Root { get; set; }
    public int T { get; set; } 

    public BTree(int t)
    {
        T = t;
        Root = new BTreeNode(true);
    }   
    public BTree() { } 

    public string Search(int key, out int comparisons)
    {
        comparisons = 0;
        return SearchInternal(Root, key, ref comparisons);
    }

    private string SearchInternal(BTreeNode node, int key, ref int comparisons)
    {
        int idx = node.Keys.BinarySearch(key);
        if (node.Keys.Count > 0)
            comparisons += (int)Math.Ceiling(Math.Log2(node.Keys.Count));

        if (idx >= 0)
        {
            return node.Values[idx];
        }

        if (node.IsLeaf)
        {
            return null;
        }
        return SearchInternal(node.Children[~idx], key, ref comparisons);
    }

    public void Insert(int key, string value)
    {
        int dummy = 0;
        if (SearchInternal(Root, key, ref dummy) != null)
        {
            throw new Exception($"Key {key} already exists!");
        }

        BTreeNode r = Root;

        if (r.Keys.Count == 2 * T - 1)
        {
            BTreeNode s = new BTreeNode(false);
            Root = s;
            s.Children.Add(r);
            SplitChild(s, 0);
            InsertNonFull(s, key, value); 
        }
        else
        {
            InsertNonFull(r, key, value);
        }
    }

    private void InsertNonFull(BTreeNode node, int key, string value)
    {
        int idx = node.Keys.BinarySearch(key);
        if (idx < 0) idx = ~idx;

        if (node.IsLeaf)
        {
            node.Keys.Insert(idx, key);
            node.Values.Insert(idx, value);
        }
        else
        {
            if (node.Children[idx].Keys.Count == 2 * T - 1)
            {
                SplitChild(node, idx);
                if (key > node.Keys[idx])
                {
                    idx++;
                }
            }
            InsertNonFull(node.Children[idx], key, value);
        }
    }

    private void SplitChild(BTreeNode parent, int index)
    {
        BTreeNode fullNode = parent.Children[index];
        BTreeNode newNode = new BTreeNode(fullNode.IsLeaf);

        newNode.Keys.AddRange(fullNode.Keys.GetRange(T, T - 1));
        newNode.Values.AddRange(fullNode.Values.GetRange(T, T - 1));

        if (!fullNode.IsLeaf)
        {
            newNode.Children.AddRange(fullNode.Children.GetRange(T, T));
            fullNode.Children.RemoveRange(T, T); 
        }

        fullNode.Keys.RemoveRange(T, T - 1);
        fullNode.Values.RemoveRange(T, T - 1);

        int medianKey = fullNode.Keys[T - 1];
        string medianVal = fullNode.Values[T - 1];

        fullNode.Keys.RemoveAt(T - 1);
        fullNode.Values.RemoveAt(T - 1);

        parent.Children.Insert(index + 1, newNode);
        parent.Keys.Insert(index, medianKey);
        parent.Values.Insert(index, medianVal);
    }

    public bool Edit(int key, string newValue)
    {
        return EditInternal(Root, key, newValue);
    }

    private bool EditInternal(BTreeNode node, int key, string newValue)
    {
        int idx = node.Keys.BinarySearch(key);
        if (idx >= 0)
        {
            node.Values[idx] = newValue;
            return true;
        }

        if (node.IsLeaf)
        {
            return false;
        }
        return EditInternal(node.Children[~idx], key, newValue);
    }

    public bool Delete(int key)
    {
        return DeleteInternal(Root, key);
    }

    private bool DeleteInternal(BTreeNode node, int key)
    {
        int idx = node.Keys.BinarySearch(key);
        if (idx >= 0)
        {
            if (node.IsLeaf)
            {
                node.Keys.RemoveAt(idx);
                node.Values.RemoveAt(idx);
                return true;
            }
            else
            {
                BTreeNode predNode = node.Children[idx];
                while (!predNode.IsLeaf)
                {
                    predNode = predNode.Children[predNode.Children.Count - 1];
                }
                int predKey = predNode.Keys[predNode.Keys.Count - 1];
                string predVal = predNode.Values[predNode.Values.Count - 1];
                node.Keys[idx] = predKey;
                node.Values[idx] = predVal;

                DeleteInternal(node.Children[idx], predKey);
                return true;
            }
        }
        else
        {
            if (node.IsLeaf)
            {
                return false;
            }
            return DeleteInternal(node.Children[~idx], key);
        }
    }

    public List<BTreeRow> GetAllRows()
    {
        var result = new List<BTreeRow>();
        CollectRows(Root, result);
        return result;
    }

    private void CollectRows(BTreeNode node, List<BTreeRow> list)
    {
        for (int i = 0; i < node.Keys.Count; i++)
        {
            if (!node.IsLeaf)
            {
                CollectRows(node.Children[i], list);
            }
            
            list.Add(new BTreeRow 
            { 
                Key = node.Keys[i], 
                Value = node.Values[i] 
            });
        }

        if (!node.IsLeaf)
        {
            CollectRows(node.Children[node.Keys.Count], list);
        }
    }

    public void SaveToFile(string path)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(this, options);
        File.WriteAllText(path, jsonString);
    }

    public static BTree LoadFromFile(string path, int t)
    {
        if (!File.Exists(path))
        {
            return new BTree(t);
        }
        string jsonString = File.ReadAllText(path);
        var tree = JsonSerializer.Deserialize<BTree>(jsonString);
        if (tree != null) return tree;
        
        return new BTree(t);
    }
}

public class BTreeRow
{    
    public int Key { get; set; }
    public string Value { get; set; }
}