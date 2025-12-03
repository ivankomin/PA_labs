using System.Windows;
namespace pa_lab4;
public partial class MainWindow : Window
{
    private BTree _btree;
    private int k = 1;
    private const string DbPath = "database.json";
    private const int T_DEGREE = 50;
    private Random rnd = new Random();

    public MainWindow()
    {
        InitializeComponent();
        _btree = BTree.LoadFromFile(DbPath, T_DEGREE);
        Log("Database loaded successfully.");
        UpdateTable();
    }

    private void Log(string msg)
    {
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\n");
        txtLog.ScrollToEnd();
    }

    private void UpdateTable()
    {
        dgData.ItemsSource = _btree.GetAllRows();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        _btree.SaveToFile(DbPath);
        base.OnClosing(e);
    }

    private void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(txtKey.Text, out int key)) { MessageBox.Show("Key must be an integer"); return; }
        string val = txtValue.Text;

        try
        {
            _btree.Insert(key, val);
            Log($"Added: Key={key}, Value={val}");
            UpdateTable();
            _btree.SaveToFile(DbPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
        }
    }

    private void BtnSearch_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(txtKey.Text, out int key)) return;

        int comps;
        string result = _btree.Search(key, out comps);

        if (result != null)
        {
            Log($"[SEARCH] Found! Key: {key} | Data: '{result}' | Comparisons: {comps}");
            MessageBox.Show($"Found!\nData: {result}\nComparisons: {comps}");
        }
        else
        {
            Log($"[SEARCH] not Found. Comparisons: {comps}");
            MessageBox.Show($"Not found!\nComparisons: {comps}");
        }
    }

    private void BtnEdit_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(txtKey.Text, out int key)) return;
        
        if (_btree.Edit(key, txtValue.Text))
        {
            Log($"Edited successfully. Key: {key}. New value: {txtValue.Text}");
            UpdateTable();
            _btree.SaveToFile(DbPath);
        }
        else
        {
            MessageBox.Show("Key not found!");
        }
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(txtKey.Text, out int key)) return;

        if (_btree.Delete(key))
        {
            Log($"Removed successfully. Key: {key}");
            UpdateTable();
            _btree.SaveToFile(DbPath);
        }
        else
        {
            MessageBox.Show("Key not found!");
        }
    }

    private void BtnGen_Click(object sender, RoutedEventArgs e)
    {
        dgData.ItemsSource = null;
        
        for (int i = 0; i < 10000; i++)
        {
            _btree.Insert(k, $"insert_data_here{k}");
            k++;
        }
        UpdateTable();
        _btree.SaveToFile(DbPath);
        Log("Generated successfully.");
    }

    private void BtnTest_Click(object sender, RoutedEventArgs e)
    {
        long totalComps = 0;
        int testCount = 25;

        for(int i=0; i < testCount; i++)
        {
            int k = rnd.Next(1, 1000000);
            int c;
            string res = _btree.Search(k, out c);
            
            string status = (res != null) ? "Found" : "Not found";
            Log($"Test #{i+1}: Key {k} -> {status}. Comparisons: {c}");
            
            totalComps += c;
        }

        double avg = (double)totalComps / testCount;
        Log($"Average comparisons: {avg:F2}");
        MessageBox.Show($"Test completed!\nAverage comparisons: {avg:F2}");
    }
}