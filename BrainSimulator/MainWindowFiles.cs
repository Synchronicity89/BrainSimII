//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using Microsoft.Win32;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;
using Onnx;
using System.Linq;
using System.Text;
using Onnx.Formatting;

namespace BrainSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static void Info(GraphProto graph, TextWriter writer)
        {
            theNeuronArray = new NeuronArray();
            int neuronCount = 100;
            int synapsesPerNeuron = 10;
            //MessageBox.Show("Starting array allocation");
            //theNeuronArray = new NeuronHandler();
            //MessageBox.Show("any existing array removed");
            theNeuronArray.Initialize(neuronCount, 10);
            //MessageBox.Show("array allocation complete");
            int test = theNeuronArray.GetArraySize();
            int threads = theNeuronArray.GetThreadCount();
            theNeuronArray.SetThreadCount(16);
            threads = theNeuronArray.GetThreadCount();

            HashSet<string> initializerNameSet = new HashSet<string>(graph.Initializer.Select((TensorProto i) => i.Name));
            int nID = 0;
            foreach (var initializer in initializerNameSet)
            {
                theNeuronArray.AddLabelToCache(nID++, initializer);
            }
            List<ValueInfoProto> valueInfos = graph.Input.Where((ValueInfoProto i) => !initializerNameSet.Contains(i.Name)).ToList();
            foreach (var valueInfo in valueInfos)
            {
                writer.WriteLine(valueInfo.ToString());
            }
            List<ValueInfoProto> valueInfos2 = graph.Input.Where((ValueInfoProto i) => initializerNameSet.Contains(i.Name)).ToList();
            writer.WriteLine("## Inputs without Initializer");
            Info(valueInfos, writer);
            writer.WriteLine();
            writer.WriteLine("## Outputs");
            Info(graph.Output, writer);
            writer.WriteLine();
            writer.WriteLine("## Inputs with Initializer");
            Info(valueInfos2, writer);
            writer.WriteLine();
            writer.WriteLine("## Initializers (Parameters etc.)");
            graph.Initializer.Format(writer);
            var dim = graph.Initializer.OrderBy(d => d.FloatData.Count).Last();
            var max = dim.FloatData.Max();
            for (int x = 0; x < neuronCount; x++)
            {
                for (int j = 0; j < synapsesPerNeuron; j++)
                {
                    int target = x + j;
                    if (target >= theNeuronArray.GetArraySize()) target -= theNeuronArray.GetArraySize();
                    if (target < 0) target += theNeuronArray.GetArraySize();
                    if (j >= dim.FloatData.Count) break;
                    theNeuronArray.AddSynapse(x, target, dim.FloatData[j] / max, 0, true);
                }
            }
            var rand = new Random();
            for (int i = 0; i < neuronCount; i++) 
            {
                var neu = theNeuronArray.GetNeuron(i);
                neu.Owner = theNeuronArray;
                neu.Model = Neuron.modelType.LIF;
                neu.LastCharge = dim.FloatData[i] / max;
                neu.ShowSynapses = true;
                foreach(var syn in neu.Synapses)
                {
                    if (syn.Weight == 0.0) syn.Weight = dim.FloatData[i + neuronCount] / max;
                    syn.TargetNeuron = (int)(100.0 * rand.NextDouble());
                }
                theNeuronArray.SetCompleteNeuron(neu);
            }
            theNeuronArray.LoadComplete = true;
            var rowsVal = theNeuronArray.rows;
            var notReady = IsArrayEmpty();
            writer.WriteLine();
            writer.WriteLine("## Value Infos");
            Info(graph.ValueInfo, writer);
        }

        private static void Info(IReadOnlyList<ValueInfoProto> valueInfos, TextWriter writer)
        {
            var tensors = valueInfos.Where((ValueInfoProto i) => i.Type.ValueCase == TypeProto.ValueOneofCase.TensorType).ToList();
            GraphExtensions.WriteInfoIfAny(tensors, "Tensors", MarkdownFormatter.FormatAsTensors, writer);
            var sequences = valueInfos.Where((ValueInfoProto i) => i.Type.ValueCase == TypeProto.ValueOneofCase.SequenceType).ToList();
            GraphExtensions.WriteInfoIfAny(sequences, "Sequences", MarkdownFormatter.FormatAsSequences, writer);
            var maps = valueInfos.Where((ValueInfoProto i) => i.Type.ValueCase == TypeProto.ValueOneofCase.MapType).ToList();
            GraphExtensions.WriteInfoIfAny(maps, "Maps", MarkdownFormatter.FormatAsMaps, writer);
            var nones = valueInfos.Where((ValueInfoProto i) => i.Type.ValueCase == TypeProto.ValueOneofCase.None).ToList();
            GraphExtensions.WriteInfoIfAny(nones, "Nones", MarkdownFormatter.FormatAsNones, writer);
        }

        private static void WriteInfoIfAny<T>(IReadOnlyList<T> values, string name, Action<IReadOnlyList<T>, TextWriter> info, TextWriter writer)
        {
            if (values.Count > 0)
            {
                writer.WriteLine("### " + name);
                info(values, writer);
            }
        }
        private async void LoadFile(string fileName)
        {
            CloseAllModuleDialogs();
            CloseHistoryWindow();
            CloseNotesWindow();
            theNeuronArrayView.theSelection.selectedRectangles.Clear();
            CloseAllModuleDialogs();
            SuspendEngine();

            bool success = false;
            var file = new FileInfo(fileName);
            if(file.Extension.ToLower().Contains("onnx"))
            {
                // Examples see https://github.com/onnx/models
                var onnxInputFilePath = file.FullName;

                var model = ModelProto.Parser.ParseFromFile(onnxInputFilePath);

                var graph = model.Graph;
                // Clean graph e.g. remove initializers from inputs that may prevent constant folding
                //graph.Clean();
                //// Set dimension in graph to enable dynamic batch size during inference
                //graph.SetDim(dimIndex: 0, DimParamOrValue.New("N"));
                // Get summarized info about the graph
                var info = graph.Info();

                System.Console.WriteLine(info);

                StringBuilder build = new StringBuilder();
                // an instance of the stringwriter class is created and the instance of the     stringbuilder class is passed as a parameter to stringwriter class
                StringWriter write = new StringWriter(build);
                Info(graph, write);
                //model.WriteToFile(@"mnist-8-clean-dynamic-batch-size.onnx");

            }
            else
            { 
                await Task.Run(delegate { success = XmlFile.Load(ref theNeuronArray, fileName); });
                if (!success)
                {
                    CreateEmptyNetwork();
                    Properties.Settings.Default["CurrentFile"] = currentFileName;
                    Properties.Settings.Default.Save();
                    ResumeEngine();
                    return;
                }
                currentFileName = fileName;

                ReloadNetwork.IsEnabled = true;
                Reload_network.IsEnabled = true;
                if (XmlFile.CanWriteTo(currentFileName))
                    SaveButton.IsEnabled = true;
                else
                    SaveButton.IsEnabled = false;
            }
            SetTitleBar();
            await Task.Delay(1000).ContinueWith(t => ShowDialogs());
            foreach (ModuleView na in theNeuronArray.modules)
            {
                if (na.TheModule != null)
                    na.TheModule.SetUpAfterLoad();
            }
            theNeuronArray.LoadComplete = true;

            if (theNeuronArray.displayParams != null)
                theNeuronArrayView.Dp = theNeuronArray.displayParams;

            AddFileToMRUList(currentFileName);
            Properties.Settings.Default["CurrentFile"] = currentFileName;
            Properties.Settings.Default.Save();

            Update();
            SetShowSynapsesCheckBox(theNeuronArray.ShowSynapses);
            SetPlayPauseButtonImage(theNeuronArray.EngineIsPaused);
            SetSliderPosition(theNeuronArray.EngineSpeed);

            engineIsPaused = theNeuronArray.EngineIsPaused;

            engineSpeedStack.Clear();
            engineSpeedStack.Push(theNeuronArray.EngineSpeed);

            if (!engineIsPaused)
                ResumeEngine();
        }

        private bool LoadClipBoardFromFile(string fileName)
        {

            XmlFile.Load(ref myClipBoard, fileName);

            foreach (ModuleView na in myClipBoard.modules)
            {
                if (na.TheModule != null)
                    na.TheModule.SetUpAfterLoad();
                {
                    try
                    {
                        na.TheModule.SetUpAfterLoad();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("SetupAfterLoad failed on module " + na.Label + ".   Message: " + e.Message);
                    }
                }
            }
            return true;
        }

        private bool SaveFile(string fileName)
        {
            SuspendEngine();
            //If the path contains "bin\64\debug" change the path to the actual development location instead
            //because file in bin..debug can be clobbered on every rebuild.
            if (fileName.ToLower().Contains("bin\\x64\\debug"))
            {
                MessageBoxResult mbResult = System.Windows.MessageBox.Show(this, "Save to source folder instead?", "Save", MessageBoxButton.YesNoCancel,
                MessageBoxImage.Asterisk, MessageBoxResult.No);
                if (mbResult == MessageBoxResult.Yes)
                    fileName = fileName.ToLower().Replace("bin\\x64\\debug\\", "");
                if (mbResult == MessageBoxResult.Cancel)
                    return false;
            }

            foreach (ModuleView na in theNeuronArray.modules)
            {
                if (na.TheModule != null)
                {
                    try
                    {
                        na.TheModule.SetUpBeforeSave();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("SetupBeforeSave failed on module " + na.Label + ".   Message: " + e.Message);
                    }
                }
            }

            theNeuronArray.displayParams = theNeuronArrayView.Dp;
            if (XmlFile.Save(theNeuronArray, fileName))
            {
                currentFileName = fileName;
                SetCurrentFileNameToProperties();
                ResumeEngine();
                undoCountAtLastSave = theNeuronArray.GetUndoCount();
                return true;
            }
            else
            {
                ResumeEngine();
                return false;
            }
        }
        private void SaveClipboardToFile(string fileName)
        {
            foreach (ModuleView na in myClipBoard.modules)
            {
                if (na.TheModule != null)
                    na.TheModule.SetUpBeforeSave();
            }

            if (XmlFile.Save(myClipBoard, fileName))
                currentFileName = fileName;
        }

        private void AddFileToMRUList(string filePath)
        {
            StringCollection MRUList = (StringCollection)Properties.Settings.Default["MRUList"];
            if (MRUList == null)
                MRUList = new StringCollection();
            MRUList.Remove(filePath); //remove it if it's already there
            MRUList.Insert(0, filePath); //add it to the top of the list
            Properties.Settings.Default["MRUList"] = MRUList;
            Properties.Settings.Default.Save();
        }

        private void LoadCurrentFile()
        {
            LoadFile(currentFileName);
        }

        private static void SetCurrentFileNameToProperties()
        {
            Properties.Settings.Default["CurrentFile"] = currentFileName;
            Properties.Settings.Default.Save();
        }

        int undoCountAtLastSave = 0;
        private bool PromptToSaveChanges()
        {
            if (IsArrayEmpty()) return false;
            MainWindow.theNeuronArray.GetCounts(out long synapseCount, out int neuronInUseCount);
            if (neuronInUseCount == 0) return false;
            if (theNeuronArray.GetUndoCount() == undoCountAtLastSave) return false; //no changes have been made

            bool canWrite = XmlFile.CanWriteTo(currentFileName, out string message);

            SuspendEngine();

            bool retVal = false;
            MessageBoxResult mbResult = System.Windows.MessageBox.Show(this, "Do you want to save changes?", "Save", MessageBoxButton.YesNoCancel,
            MessageBoxImage.Asterisk, MessageBoxResult.No);
            if (mbResult == MessageBoxResult.Yes)
            {
                if (currentFileName != "" && canWrite)
                {
                    if (SaveFile(currentFileName))
                        undoCountAtLastSave = theNeuronArray.GetUndoCount();
                }
                else
                {
                    if (SaveAs())
                    {
                    }
                    else
                    {
                        retVal = true;
                    }
                }
            }
            if (mbResult == MessageBoxResult.Cancel)
            {
                retVal = true;
            }
            ResumeEngine();
            return retVal;
        }
        private bool SaveAs()
        {
            string defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            defaultPath += "\\BrainSim";
            try
            {
                if (Directory.Exists(defaultPath)) defaultPath = "";
                else Directory.CreateDirectory(defaultPath);
            }
            catch
            {
                //maybe myDocuments is readonly of offline? let the user do whatever they want
                defaultPath = "";
            }
            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                Filter = "XML Network Files|*.xml",
                Title = "Select a Brain Simulator File",
                InitialDirectory = defaultPath
            };

            // Show the Dialog.  
            // If the user clicked OK in the dialog and  
            Nullable<bool> result = saveFileDialog1.ShowDialog();
            if (result ?? false)// System.Windows.Forms.DialogResult.OK)
            {
                if (SaveFile(saveFileDialog1.FileName))
                {
                    AddFileToMRUList(currentFileName);
                    SetTitleBar();
                    return true;
                }
            }
            return false;
        }
    }
}
