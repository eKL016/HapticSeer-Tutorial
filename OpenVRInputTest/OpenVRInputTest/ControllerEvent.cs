﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace OpenVRInputTest
{
    public class Button_B_Event : ControllerEvent
    {
        public override string EventName()
        {
            return "B";
        }
        public override EventTypeEmun EventType()
        {
            return EventTypeEmun.Digital;
        }
    }
    public class Button_A_Event : ControllerEvent
    {
        public override string EventName()
        {
            return "A";
        }
        public override EventTypeEmun EventType()
        {
            return EventTypeEmun.Digital;
        }
    }
    public class Button_System_Event : ControllerEvent
    {
        public override string EventName()
        {
            return "System";
        }
        public override EventTypeEmun EventType()
        {
            return EventTypeEmun.Digital;
        }
    }
    public class Button_Trigger_Event : ControllerEvent
    {
        public override string EventName()
        {
            return "Trigger";
        }
        public override EventTypeEmun EventType()
        {
            return EventTypeEmun.Digital;
        }
    }
    public class Button_Touchpad_Event : ControllerEvent
    {
        public override string EventName()
        {
            return "Touchpad";
        }
        public override EventTypeEmun EventType()
        {
            return EventTypeEmun.Digital;
        }
    }
    public class Button_ThumbStick_Event : ControllerEvent
    {
        public override string EventName()
        {
            return "ThumbStick";
        }
        public override EventTypeEmun EventType()
        {
            return EventTypeEmun.Digital;
        }
    }
    public class Button_Grip_Event : ControllerEvent
    {
        public override string EventName()
        {
            return "Grip";
        }
        public override EventTypeEmun EventType()
        {
            return EventTypeEmun.Digital;
        }
    }
    public abstract class ControllerEvent
    {
        public enum EventTypeEmun
        {
            Digital,
            Analog
        }
        public abstract string EventName();
        public abstract EventTypeEmun EventType();

        Controller controller;
        ulong ActionHandle;
        InputDigitalActionData_t Digital;
        InputAnalogActionData_t Analog;
        public ControllerEvent()
        {
            switch (EventType())
            {
                case EventTypeEmun.Digital:
                    Digital = new InputDigitalActionData_t();
                    break;
                case EventTypeEmun.Analog:
                    Analog = new InputAnalogActionData_t();
                    break;
            }
        }
        public void AttachToController(Controller controller)
        {
            if (controller == null)
                throw new Exception("Parameter in ControllerEvent.AttachToController: controller is Null");
            this.controller = controller;
            var errorID = OpenVR.Input.GetActionHandle(controller.ActionHandleBasePath + EventName(), ref ActionHandle);
            if (errorID != EVRInputError.None)
                Utils.PrintError($"GetActionHandle {controller.ControllerName} {EventName()} Error: {Enum.GetName(typeof(EVRInputError), errorID)}");
            Utils.PrintDebug($"Action Handle {controller.ControllerName} {EventName()}: {ActionHandle}");
        }
        public bool DigitalFetchEventResult()
        {
            var size = (uint)Marshal.SizeOf(typeof(InputDigitalActionData_t));
            OpenVR.Input.GetDigitalActionData(ActionHandle, ref Digital, size, controller.ControllerHandle);
#if DEBUG
            // Result
            if (Digital.bChanged)
            {
                Utils.PrintInfo($"Action {ActionHandle}, Active: {Digital.bActive}, State: {Digital.bState} on: {controller.ControllerHandle}");
            }
#endif
            return Digital.bState;
        }
        public InputAnalogActionData_t AnalogFetechEventResult()
        {
            var size = (uint)Marshal.SizeOf(typeof(InputAnalogActionData_t));
#if DEBUG
            var LastTimeStamp = Analog.fUpdateTime;
#endif
            OpenVR.Input.GetAnalogActionData(ActionHandle, ref Analog, size, controller.ControllerHandle);
#if DEBUG
            // Result
            if (Analog.fUpdateTime != LastTimeStamp)
            {
                Utils.PrintInfo($"Action {ActionHandle}, Active: {Analog.bActive}, State: {{{Analog.x}, {Analog.y}, {Analog.x}}} on: {controller.ControllerHandle}");
            }
#endif
            return Analog;
        }
    }
}