﻿//
// Copyright (c) [Name]. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

namespace BrainSimulator.Modules
{
    public class ModuleLife : ModuleBase
    {
        //any public variable you create here will automatically be stored with the network
        //unless you precede it with the [XmlIgnore] directive
        //[XlmIgnore] 
        //public theStatus = 1;

        //fill this method in with code which will execute
        //once for each cycle of the engine
        public override void Fire()
        {
            Init();  //be sure to leave this here

            //if you want the dlg to update, use the following code 
            //because the thread you are in is not the UI thread
            //if (dlg != null)
            //     Application.Current.Dispatcher.Invoke((Action)delegate { dlg.Draw(); });
        }

        //fill this method in with code which will execute once
        //when the module is added, when "initialize" is selected from the context menu,
        //or when the engine restart button is pressed
        public override void Initialize()
        {
            ClearNeurons();
            for (int x = 0; x < mv.Width; x += 2)
            {
                for (int y = 0; y < mv.Height; y += 2)
                {
                    //three internal neurons 
                    Neuron valueNeuron = mv.GetNeuronAt(x + 1, y);
                    if (valueNeuron == null) continue;
                    valueNeuron.Label = "#";
                    Neuron liveNeuron = mv.GetNeuronAt(x, y);
                    if (liveNeuron == null) continue;
                    liveNeuron.Label = "+";
                    Neuron dieNeuron = mv.GetNeuronAt(x, y + 1);
                    if (dieNeuron == null) continue;
                    dieNeuron.Label = "-";
                    liveNeuron.Model = Neuron.modelType.LIF;
                    dieNeuron.Model = Neuron.modelType.LIF;
                    liveNeuron.LeakRate = 1;
                    dieNeuron.LeakRate = 1;
                    //three internal synapses
                    try { liveNeuron.AddSynapse(valueNeuron.Id, 1); } catch { }
                    try
                    {
                        dieNeuron.AddSynapse(valueNeuron.Id, -1);
                    }
                    catch { }
                    try
                    {
                        valueNeuron.AddSynapse(liveNeuron.Id, .4f);
                    }
                    catch { }
                    // add live & die synapses to the 8 surrounding neurons
                    // may be better to rewrite this without relying on try catch blocks. 
                    // bottom row
                    try { valueNeuron.AddSynapse(mv.GetNeuronAt(x + 2, y).Id, .4f); } catch { }
                    try { valueNeuron.AddSynapse(mv.GetNeuronAt(x + 2, y + 1).Id, .3f); } catch { }
                    try { valueNeuron.AddSynapse(mv.GetNeuronAt(x + 2, y + 2).Id, .4f); } catch { }
                    try { valueNeuron.AddSynapse(mv.GetNeuronAt(x + 2, y + 2 + 1).Id, .3f); } catch { }
                    try { valueNeuron.AddSynapse(mv.GetNeuronAt(x + 2, y - 2).Id, .4f); } catch { }
                    try { valueNeuron.AddSynapse(mv.GetNeuronAt(x + 2, y - 2 + 1).Id, .3f); } catch { }

                    // middle row
                    try { valueNeuron.AddSynapse(mv.GetNeuronAt(x, y + 2).Id, .4f); } catch { }
                    try { valueNeuron.AddSynapse(mv.GetNeuronAt(x, y + 2 + 1).Id, .3f); } catch { }
                    try { valueNeuron.AddSynapse(mv.GetNeuronAt(x, y - 2).Id, .4f); } catch { }
                    try { valueNeuron.AddSynapse(mv.GetNeuronAt(x, y - 2 + 1).Id, .3f); } catch { }

                    // top row
                    try { valueNeuron.AddSynapse(mv.GetNeuronAt(x - 2, y).Id, .4f); } catch { }
                    try { valueNeuron.AddSynapse(mv.GetNeuronAt(x - 2, y + 1).Id, .3f); } catch { }
                    try { valueNeuron.AddSynapse(mv.GetNeuronAt(x - 2, y + 2).Id, .4f); } catch { }
                    try { valueNeuron.AddSynapse(mv.GetNeuronAt(x - 2, y + 2 + 1).Id, .3f); } catch { }
                    try { valueNeuron.AddSynapse(mv.GetNeuronAt(x - 2, y - 2).Id, .4f); } catch { }
                    try { valueNeuron.AddSynapse(mv.GetNeuronAt(x - 2, y - 2 + 1).Id, .3f); } catch { }
                }
            }
        }
    }
}
