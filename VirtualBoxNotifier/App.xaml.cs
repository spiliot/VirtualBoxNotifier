using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using VirtualBox;


namespace VirtualBoxNotifier
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        static IVirtualBoxClient vboxClient = new VirtualBoxClient();
        static int ActiveVMs = 0;
        static System.Windows.Forms.PowerLineStatus PowerMode = System.Windows.Forms.PowerLineStatus.Unknown;

        NotifyIcon notifyIcon = new NotifyIcon();

        App()
        {
            CreateNotifyIcon();
            RegisterVBoxListener();
            RegisterPowerModeListener();

            ActiveVMs = GetActiveVirtualMachineCount(vboxClient.VirtualBox);
            notifyIcon.Text = CreateMessage(ActiveVMs);

            PowerMode = SystemInformation.PowerStatus.PowerLineStatus;
            NotifyIfActiveVMsOnBattery();
        }

        private void CreateNotifyIcon()
        {
            ContextMenu rightClickContextMenu = new ContextMenu();
            rightClickContextMenu.MenuItems.Add("Quit", QuitMenu_Click);
            notifyIcon.ContextMenu = rightClickContextMenu;
            notifyIcon.Icon = new Icon(SystemIcons.Information, 48, 48);
            notifyIcon.Visible = true;
        }

        private void RegisterVBoxListener()
        {
            VirtualBoxSimpleEventNotifier vboxEventListener = new VirtualBoxSimpleEventNotifier();
            VBoxEventType[] eventsOfInterest = { VBoxEventType.VBoxEventType_OnMachineStateChanged };

            vboxClient.VirtualBox.EventSource.RegisterListener(vboxEventListener, eventsOfInterest, 1);

            vboxEventListener.EventReceived += VboxEventListener_EventFired;
        }

        private void RegisterPowerModeListener()
        {
            Microsoft.Win32.SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        private void VboxEventListener_EventFired()
        {
            int currentActiveVMs = GetActiveVirtualMachineCount(vboxClient.VirtualBox);
            if (currentActiveVMs == ActiveVMs) return;

            ActiveVMs = currentActiveVMs;
            notifyIcon.Text = CreateMessage(ActiveVMs);
        }

        private void SystemEvents_PowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        {
            if (e.Mode != Microsoft.Win32.PowerModes.StatusChange) return;

            PowerMode = SystemInformation.PowerStatus.PowerLineStatus;

            NotifyIfActiveVMsOnBattery();
        }

        private void NotifyIfActiveVMsOnBattery()
        {
            if (PowerMode != System.Windows.Forms.PowerLineStatus.Offline || ActiveVMs == 0) return;

            string message = "You're on battery and have " + CreateMessage(ActiveVMs);
            notifyIcon.ShowBalloonTip(5000, "VirtualBoxNotifier", message, ToolTipIcon.Warning);
        }

        private string CreateMessage(int numberOfActiveVMs)
        {
            return (numberOfActiveVMs > 0 ? numberOfActiveVMs.ToString() : "No") + " active virtual machine" + (numberOfActiveVMs != 1 ? "s" : "");
        }

        private int GetActiveVirtualMachineCount(VirtualBox.VirtualBox virtualBox)
        {
            int activeVMs = 0;

            foreach (IMachine machine in virtualBox.Machines)
            {
                if (
                    machine.State == MachineState.MachineState_Running ||
                    machine.State == MachineState.MachineState_Restoring ||
                    machine.State == MachineState.MachineState_RestoringSnapshot ||
                    machine.State == MachineState.MachineState_LiveSnapshotting ||
                    machine.State == MachineState.MachineState_DeletingSnapshot
                ) {
                    activeVMs++;
                }
            }

            return activeVMs;
        }

        private void QuitMenu_Click(object sender, EventArgs e)
        {
            //If icon is not made invisible before shutdown it will stay in the try until a mouse over removes it
            notifyIcon.Visible = false;
            Current.Shutdown();
        }

    }
}
