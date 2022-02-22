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
        //dynamic GetMNistPretrained(NDarray matrix)
        //{
        //    //List<NDarray> matrix = new List<NDarray>();
        //    //matrix.Add(np.array(MNist.a1));
        //    //matrix.Add(np.array(MNist.a2));
        //    //matrix.Add(np.array(MNist.a3));
        //    //matrix.Add(np.array(MNist.a4));
        //    //matrix.Add(np.array(MNist.a5));
        //    //matrix.Add(np.array(MNist.a6));
        //    //matrix.Add(np.array(MNist.a7));

        //    var w0 = ((NDarray)matrix[0]).transpose().astype(np.int32);
        //    var w1 = ((NDarray)matrix[2]).transpose().astype(np.int32);
        //    var w2 = ((NDarray)matrix[4]).transpose().astype(np.int32);
        //    var b1 = ((NDarray)matrix[1]).astype(np.int32);
        //    var b2 = ((NDarray)matrix[3]).astype(np.int32);
        //    var b3 = ((NDarray)matrix[5]).astype(np.int32);
        //    dynamic self = new System.Dynamic.ExpandoObject();

        //    self.spikes_in = new InPort(shape: new Shape(w0.shape[1]));//(w0.shape[1], )
        //    self.spikes_out = new OutPort(shape: new Shape(w2.shape[0]));//(w2.shape[0], )
        //    self.w_dense0 = new Var(shape: w0.shape, init: w0);
        //    self.b_lif1 = new Var(shape: new Shape(w0.shape[0]), init: b1);//(w0.shape[0],)
        //    self.w_dense1 = new Var(shape: w1.shape, init: w1);
        //    self.b_lif2 = new Var(shape: new Shape(w1.shape[0]), init: b2);//(w1.shape[0],)
        //    self.w_dense2 = new Var(shape: w2.shape, init: w2);
        //    self.b_output_lif = new Var(shape: new Shape(w2.shape[0]), init: b3);//(w2.shape[0],)

        //    //# Up-level currents and voltages of LIF Processes
        //    //# for resetting (see at the end of the tutorial)
        //    //            self.lif1_u = Var(shape = (w0.shape[0],), init = 0)
        //    //            self.lif1_v = Var(shape = (w0.shape[0],), init = 0)
        //    //            self.lif2_u = Var(shape = (w1.shape[0],), init = 0)
        //    //            self.lif2_v = Var(shape = (w1.shape[0],), init = 0)
        //    //            self.oplif_u = Var(shape = (w2.shape[0],), init = 0)
        //    //            self.oplif_v = Var(shape = (w2.shape[0],), init = 0)

        //    return self;
        //}
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

            theNeuronArray.SetNeuronCurrentCharge(1, 1.4f);
            theNeuronArray.SetNeuronCurrentCharge(2, 0.9f);
            theNeuronArray.Fire(); //should transfer current chargest to last
            float a = theNeuronArray.GetNeuronLastCharge(1);
            float b = theNeuronArray.GetNeuronLastCharge(2);

            string s0 = theNeuronArray.GetNeuronLabel(1);
            theNeuronArray.SetNeuronLabel(1, "Fred");
            string s1 = theNeuronArray.GetNeuronLabel(1);
            theNeuronArray.SetNeuronLabel(1, "George");
            string s2 = theNeuronArray.GetNeuronLabel(1);

            theNeuronArray.AddSynapse(2, 4, .75f, 1, false);
            List<Synapse> synapses2 = theNeuronArray.GetSynapsesList(2);
            theNeuronArray.AddSynapse(1, 2, .5f, 0, false);
            List<Synapse> synapses1 = theNeuronArray.GetSynapsesList(1);
            theNeuronArray.AddSynapse(1, 3, .6f, 0, false);
            synapses1 = theNeuronArray.GetSynapsesList(1);
            theNeuronArray.AddSynapse(1, 4, .75f, 1, false);
            synapses1 = theNeuronArray.GetSynapsesList(1);
            theNeuronArray.AddSynapse(2, 4, .75f, 1, false);
            long count = theNeuronArray.GetTotalSynapses();
            List<Synapse> synapses0 = theNeuronArray.GetSynapsesList(0);
            synapses1 = theNeuronArray.GetSynapsesList(1);
            List<Synapse> synapsesFrom = theNeuronArray.GetSynapsesFromList(4);

            NeuronPartial n = theNeuronArray.GetPartialNeuron(1);
            theNeuronArray.Fire();
            long gen = theNeuronArray.GetGeneration();
            NeuronPartial n1 = theNeuronArray.GetPartialNeuron(1);
            NeuronPartial n2 = theNeuronArray.GetPartialNeuron(2);
            NeuronPartial n3 = theNeuronArray.GetPartialNeuron(3);
            theNeuronArray.DeleteSynapse(1, 3);
            theNeuronArray.DeleteSynapse(1, 2);





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
            //foreach(var dim in graph.Initializer)
            //{
                //MessageBox.Show("allocating synapses");
                //Parallel.For(0, neuronCount, x =>
                //{
            for (int x = 0; x < neuronCount; x++)
            {
                for (int j = 0; j < synapsesPerNeuron; j++)
                {
                    int target = x + j;
                    if (target >= theNeuronArray.GetArraySize()) target -= theNeuronArray.GetArraySize();
                    if (target < 0) target += theNeuronArray.GetArraySize();
                    //theNeuronArray.AddSynapse(x, target, 1.0f, 0, true);
                    if (j >= dim.FloatData.Count) break;
                    theNeuronArray.AddSynapse(x, target, dim.FloatData[j] / max, 0, true);
                }
            }
            //});
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
                //theNeuronArray.SetNeuronCurrentCharge(100 * i, 1);
                theNeuronArray.SetCompleteNeuron(neu);
            }
            //MessageBox.Show("synapses and charge complete");
            //}
            //theNeuronArray.Initialize(arraySize, theNeuronArray.rows);
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

                //var myData = np.load(file.FullName,allow_pickle: true);//.Replace("\\", "/")
                //var table = GetMNistPretrained(myData);


                // Examples see https://github.com/onnx/models
                var onnxInputFilePath = file.FullName;

                var model = ModelProto.Parser.ParseFromFile(onnxInputFilePath);

                var graph = model.Graph;
                // Clean graph e.g. remove initializers from inputs that may prevent constant folding
                graph.Clean();
                // Set dimension in graph to enable dynamic batch size during inference
                graph.SetDim(dimIndex: 0, DimParamOrValue.New("N"));
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
