using System;
using System.Collections.Generic;
using Stepflow;
using Stepflow.Gui;
using MidiControls;

namespace Stepflow.Midi.ControlHelpers
{
    public interface IResonator : IStepflowControlElementComponent
    {
        StringControl.SlideBar elementar { get; set; }
    }
}
