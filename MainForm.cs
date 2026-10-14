using System.Text.Json;

namespace FolderSorter;

public class MainForm : Form
{
    Button select = new();
    Button add = new();
    Button skip = new();
    ListBox list = new();
    FlowLayoutPanel buttons = new();

    List<DirectoryInfo> folders = new();
    DirectoryInfo? current;
    List<TargetConfig> targets = new();

    string configFile = Path.Combine(AppContext.BaseDirectory, "targets.json");

    public MainForm()
    {
        Text = "文件夹分类助手";
        Width = 900;
        Height = 600;

        select.Text = "选择待分类目录";
        select.Dock = DockStyle.Top;
        select.Click += SelectFolder;

        add.Text = "添加目标";
        add.Dock = DockStyle.Top;
        add.Click += AddTarget;

        list.Dock = DockStyle.Fill;

        buttons.Dock = DockStyle.Bottom;
        buttons.Height = 80;

        skip.Text = "跳过";
        skip.Click += (_,_) => Next();

        Controls.Add(list);
        Controls.Add(buttons);
        Controls.Add(skip);
        Controls.Add(add);
        Controls.Add(select);

        LoadTargets();
        RefreshButtons();
    }

    void SelectFolder(object? s, EventArgs e)
    {
        using var d = new FolderBrowserDialog();
        if(d.ShowDialog()!=DialogResult.OK) return;

        folders = new DirectoryInfo(d.SelectedPath)
            .GetDirectories()
            .Where(x=>!targets.Any(t=>Path.GetFullPath(t.Path)==Path.GetFullPath(x.FullName)))
            .ToList();

        Next();
    }

    void AddTarget(object? s, EventArgs e)
    {
        using var d = new FolderBrowserDialog();
        if(d.ShowDialog()!=DialogResult.OK) return;

        var name = Microsoft.VisualBasic.Interaction.InputBox(
            "按钮显示名称",
            "备注",
            new DirectoryInfo(d.SelectedPath).Name);

        targets.Add(new TargetConfig{
            Name=name,
            Path=d.SelectedPath
        });

        SaveTargets();
        RefreshButtons();
    }

    void RefreshButtons()
    {
        buttons.Controls.Clear();

        foreach(var t in targets)
        {
            var b=new Button();
            b.Text=t.Name;
            b.Width=120;
            b.Click += (_,_)=>MoveCurrent(t);
            buttons.Controls.Add(b);
        }
    }

    void MoveCurrent(TargetConfig target)
    {
        if(current==null) return;

        var dest=Path.Combine(target.Path,current.Name);

        if(Directory.Exists(dest))
        {
            MessageBox.Show("目标位置已有同名文件夹");
            return;
        }

        Directory.Move(current.FullName,dest);
        Next();
    }

    void Next()
    {
        list.Items.Clear();

        if(folders.Count==0)
        {
            list.Items.Add("完成");
            current=null;
            return;
        }

        current=folders[0];
        folders.RemoveAt(0);

        list.Items.Add("当前文件夹："+current.Name);
        list.Items.Add("----------------");

        foreach(var f in current.GetFiles())
            list.Items.Add(GetIcon(f.Extension)+" "+f.Name);
    }

    string GetIcon(string ext)
    {
        ext=ext.ToLower();

        if(new[]{".mp4",".mov",".mkv",".avi",".webm"}.Contains(ext))
            return "🎬";

        if(new[]{".jpg",".jpeg",".png",".gif",".webp"}.Contains(ext))
            return "🖼";

        if(ext==".json")
            return "📄";

        if(new[]{".pkg",".zip",".rar"}.Contains(ext))
            return "📦";

        return "⬜";
    }

    void LoadTargets()
    {
        if(File.Exists(configFile))
        {
            targets=JsonSerializer.Deserialize<List<TargetConfig>>(
                File.ReadAllText(configFile)) ?? new();
        }
    }

    void SaveTargets()
    {
        File.WriteAllText(configFile,
            JsonSerializer.Serialize(targets,new JsonSerializerOptions{
                WriteIndented=true
            }));
    }
}