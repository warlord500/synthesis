﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EditorsLibrary;
using OGLViewer;

public partial class ExporterGUI : Form
{

    public static ExporterGUI Instance;

    private RigidNode_Base skeletonBase;
    private List<BXDAMesh> meshes;

    private ExporterProgressForm exporterProgress;

    private string lastDirPath = null;

    public ExporterGUI()
    {
        InitializeComponent();

        Instance = this;
        BXDSettings.Load();

        RigidNode_Base.NODE_FACTORY = delegate()
        {
            return new OGL_RigidNode();
        };

        fileNew.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            SetNew();
        });
        fileLoad.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            LoadFromInventor();
        });
        fileOpen.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            OpenExisting();
        });
        fileSave.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            SaveRobot(false);
        });
        fileSaveAs.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            SaveRobot(true);
        });
        fileExit.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            Close();
        });

        settingsExporter.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            var defaultValues = BXDSettings.Instance.GetSettingsObject("Exporter Settings");

            ExporterSettings eSettings = new ExporterSettings((defaultValues != null) ? (ExporterSettings.EditorSettingsValues) defaultValues :
                                                                                        ExporterSettings.GetDefaultSettings());

            eSettings.ShowDialog(this);

            BXDSettings.Instance.AddSettingsObject("Exporter Settings", eSettings.values);
        });
        settingsViewer.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            var defaultValues = BXDSettings.Instance.GetSettingsObject("Viewer Settings");

            ViewerSettings vSettings = new ViewerSettings((defaultValues != null) ? (ViewerSettings.ViewerSettingsValues) defaultValues : 
                                                                                    ViewerSettings.GetDefaultSettings());
            vSettings.ShowDialog(this);

            BXDSettings.Instance.AddSettingsObject("Viewer Settings", vSettings.values);
        });

        helpAbout.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            AboutDialog about = new AboutDialog();
            about.ShowDialog(this);
        });

        this.FormClosing += new FormClosingEventHandler(delegate(object sender, FormClosingEventArgs e)
        {
            if (skeletonBase != null && !WarnUnsaved()) e.Cancel = true;
            else BXDSettings.Save();
        });
    }

    public void SetNew()
    {
        skeletonBase = null;
        meshes = null;

        ReloadPanels();
    }

    public void LoadFromInventor()
    {
        if (skeletonBase != null && !WarnUnsaved()) return;

        try
        {
            AutoResetEvent startEvent = new AutoResetEvent(false);

            var exporterThread = new Thread(() =>
            {
                exporterProgress = new ExporterProgressForm(startEvent);
                exporterProgress.ShowDialog();
            });

            exporterThread.SetApartmentState(ApartmentState.STA);
            exporterThread.Start();

            startEvent.WaitOne();

            Exporter.LoadInventorInstance();
            skeletonBase = Exporter.ExportSkeleton();
            meshes = Exporter.ExportMeshes(skeletonBase);

            Console.WriteLine("Finished!");
            exporterProgress.SetProgressText("Finished");

            exporterThread.Join();
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
            return;
        }

        ReloadPanels();
    }

    public void OpenExisting()
    {
        if (skeletonBase != null && !WarnUnsaved()) return;

        string dirPath = OpenFolderPath();

        if (dirPath == null) return;

        try
        {
            skeletonBase = BXDJSkeleton.ReadSkeleton(dirPath + "\\skeleton.bxdj");
            meshes = new List<BXDAMesh>();

            var meshFiles = Directory.GetFiles(dirPath).Where(name => name.EndsWith(".bxda"));
            foreach (string fileName in meshFiles)
            {
                BXDAMesh mesh = new BXDAMesh();
                mesh.ReadFromFile(fileName);
                meshes.Add(mesh);
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
        }

        lastDirPath = dirPath;

        ReloadPanels();
    }

    public bool SaveRobot(bool isSaveAs)
    {
        if (skeletonBase == null || meshes == null) return false;

        string dirPath = lastDirPath;

        if (dirPath == null || isSaveAs) dirPath = OpenFolderPath();

        if (!isSaveAs ^ File.Exists(dirPath + "\\skeleton.bxdj") && !WarnOverwrite()) return false;

        try
        {
            BXDJSkeleton.WriteSkeleton(dirPath + "\\skeleton.bxdj", skeletonBase);

            for (int i = 0; i < meshes.Count; i++)
            {
                meshes[i].WriteToFile(dirPath + "\\node_" + i + ".bxda");
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
        }

        MessageBox.Show("Saved!");

        lastDirPath = dirPath;

        return true;
    }

    public void ExporterReset()
    {
        exporterProgress.ResetProgress();
    }

    public void ExporterSetProgress(double percentLength)
    {
        exporterProgress.AddProgress((int) Math.Floor(percentLength) - exporterProgress.GetProgress());
    }

    public void ExporterSetSubText(string text)
    {
        exporterProgress.SetProgressText(text);
    }

    private string OpenFolderPath()
    {
        string dirPath = null;

        var dialogThread = new Thread(() =>
        {
            FolderBrowserDialog openDialog = new FolderBrowserDialog();
            openDialog.RootFolder = Environment.SpecialFolder.UserProfile;
            openDialog.ShowNewFolderButton = false;
            openDialog.Description = "Choose Robot Folder";
            DialogResult openResult = openDialog.ShowDialog();

            if (openResult == DialogResult.OK) dirPath = openDialog.SelectedPath;
        });

        dialogThread.SetApartmentState(ApartmentState.STA);
        dialogThread.Start();
        dialogThread.Join();

        return dirPath;
    }

    private bool WarnOverwrite()
    {
        DialogResult overwriteResult = MessageBox.Show("Really overwrite?", "Overwrite Warning", MessageBoxButtons.YesNo);

        if (overwriteResult == DialogResult.Yes) return true;
        else return false;
    }

    private bool WarnUnsaved()
    {
        DialogResult saveResult = MessageBox.Show("Do you want to save your work?", "Save", MessageBoxButtons.YesNoCancel);

        if (saveResult == DialogResult.Yes)
        {
            return SaveRobot(false);
        }
        else if (saveResult == DialogResult.No)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void ReloadPanels()
    {
        jointEditorPane1.SetSkeleton(skeletonBase);
        bxdaEditorPane1.loadModel(meshes);
        robotViewer1.loadModel(skeletonBase, meshes);
    }

}
