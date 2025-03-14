using System.Text;
using System.Diagnostics;

using KH2FML;
using BSharpConvention = Binarysharp.MSharp.Assembly.CallingConvention.CallingConventions;

namespace KeybladeSwitch
{
    public partial class Program
    {
        static short CURRENT_BASE = 0x00;
        static short CURRENT_FORM = 0x00;

        static ulong OLD_FORM = 0x00;
        static ulong ALLOCATE_VSB = 0x00;
        static int VSB_SIZE;

        static bool DEBOUNCE = false;
        static bool RELOADING = false;

        static bool DEBOUNCE_MENU = false;

        static int MENU_MODE = 0x00;
        static int PAST_MODE = 0xFF;

        static int PAST_INDEX = 0xFF;

        static byte[] TEXT_ACTUAL;
        static ulong TEXT_POINTER;

        public static void MenuLogic()
        {
            var _menuType = Hypervisor.Read<byte>(Variables.ADDR_MenuType);
            var _menuIndex = Hypervisor.Read<byte>(0x903D80);

            var _subMenuType = Hypervisor.Read<byte>(Variables.ADDR_SubMenuType);

            var _readValor = Hypervisor.Read<short>(Variables.ADDR_SaveData + 0x32BCU + 0x38U * 0x01);
            var _readMaster = Hypervisor.Read<short>(Variables.ADDR_SaveData + 0x32BCU + 0x38U * 0x04);
            var _readFinal = Hypervisor.Read<short>(Variables.ADDR_SaveData + 0x32BCU + 0x38U * 0x05);

            var _subMenuCheck = _subMenuType == 0x01 || _subMenuType == 0x02;

            if (_subMenuCheck)
                Hypervisor.Write(TEXT_POINTER, 0x00, true);

            else
                Hypervisor.Write(TEXT_POINTER, TEXT_ACTUAL, true);

            if (!Variables.IS_PAUSED && PAST_MODE != 0xFF)
            {
                if (MENU_MODE == 0x00)
                {
                    var _offsetList = new uint[] { 0x01, 0x04, 0x05 };

                    for (uint i = 0; i < 3; i++)
                    {
                        var _offsetKey = Variables.ADDR_SaveData + 0x32BC + (_offsetList[i] * 0x38);
                        var _readKey = Hypervisor.Read<ushort>(_offsetKey);

                        var _readTarget = Hypervisor.Read<ushort>(Variables.MemoryKH["KEYBLADE_DRIVES"] + (i * 0x02), true);

                        Hypervisor.Write(_offsetKey, _readTarget);

                        Hypervisor.Write(Variables.ADDR_SaveData + 0xE400 + (i * 0x02), _readKey);
                        Hypervisor.Write(Variables.MemoryKH["KEYBLADE_SWITCH"] + (i * 0x02), _readKey, true);
                    }
                }

                MENU_MODE = 0x00;
                PAST_MODE = 0xFF;

                Hypervisor.Write<byte>(Variables.ADDR_SubMenuType, 0x00);

                RecolorMenu();
            }

            else if (Variables.IS_PAUSED && _menuType == 0x08 && _subMenuCheck)
            {
                Hypervisor.Write(0x9ACF6A, (byte)(MENU_MODE == 0x00 && _menuIndex != 0x00 ? 0x01 : 0x00));

                if (!DEBOUNCE_MENU && Variables.IS_PRESSED(Variables.BUTTON.SELECT))
                {
                    MENU_MODE = MENU_MODE == 0x00 ? 0x01 : 0x00;
                    DEBOUNCE_MENU = true;
                }

                else if (DEBOUNCE_MENU && !Variables.IS_PRESSED(Variables.BUTTON.SELECT))
                    DEBOUNCE_MENU = false;

                if (MENU_MODE != PAST_MODE)
                {
                    if (MENU_MODE == 0x00)
                    {
                        Terminal.Log("Switching to Switch Memory...", 0);

                        var _offsetList = new uint[] { 0x01, 0x04, 0x05 };

                        for (uint i = 0; i < 3; i++)
                        {
                            var _offsetKey = Variables.ADDR_SaveData + 0x32BC + (_offsetList[i] * 0x38);
                            var _readKey = Hypervisor.Read<ushort>(_offsetKey);

                            var _readTarget = Hypervisor.Read<ushort>(Variables.MemoryKH["KEYBLADE_SWITCH"] + (i * 0x02), true);

                            Hypervisor.Write(_offsetKey, _readTarget);
                            Hypervisor.Write(Variables.ADDR_SaveData + 0xE400 + (i * 0x02), _readKey);
                            Hypervisor.Write(Variables.MemoryKH["KEYBLADE_DRIVES"] + (i * 0x02), _readKey, true);
                        }
                    }

                    else
                    {
                        Terminal.Log("Switching to Form Memory...", 0);

                        var _offsetList = new uint[] { 0x01, 0x04, 0x05 };

                        for (uint i = 0; i < 3; i++)
                        {
                            var _offsetKey = Variables.ADDR_SaveData + 0x32BC + (_offsetList[i] * 0x38);
                            var _readKey = Hypervisor.Read<ushort>(_offsetKey);

                            var _readTarget = Hypervisor.Read<ushort>(Variables.MemoryKH["KEYBLADE_DRIVES"] + (i * 0x02), true);

                            Hypervisor.Write(_offsetKey, _readTarget);
                            Hypervisor.Write(Variables.ADDR_SaveData + 0xE400 + (i * 0x02), _readKey);
                            Hypervisor.Write(Variables.MemoryKH["KEYBLADE_SWITCH"] + (i * 0x02), _readKey, true);
                        }
                    }

                    Variables.SharpHook[0x34C770].Execute(BSharpConvention.MicrosoftX64, Hypervisor.Read<ulong>(0x0BEECC8), Hypervisor.MemoryOffset + 5, 0x00);
                    Variables.SharpHook[0x34C1B0].Execute(BSharpConvention.MicrosoftX64, Hypervisor.Read<ulong>(0x0BEECC8), 0x00, 0x00);

                    RecolorMenu();

                    if (DEBOUNCE_MENU)
                        Sound.PlaySFX(2);

                    PAST_MODE = MENU_MODE;
                }
            }
        }

        public static void RecolorMenu()
        {
            byte[] _colorMenu = MENU_MODE == 0 ? [0x30, 0x00, 0x00] : [0x00, 0x20, 0x20];
            byte[] _colorLabel = MENU_MODE == 0 ? [0x3C, 0x00, 0x00] : [0x00, 0x32, 0x32];
            byte[] _colorEmboss = MENU_MODE == 0 ? [0x88, 0x64, 0x64] : [0x64, 0x80, 0x80];

            byte _colorOpacity = (byte)(MENU_MODE == 0 ? 0x00 : 0x80);

            Hypervisor.Write(0xBBE628, MENU_MODE == 0x00 ? 0x80000050 : 0x80504000);
            Hypervisor.Write(0xBBE62C, MENU_MODE == 0x00 ? 0x80000050 : 0x80504000);

            Hypervisor.Write(0xBC9BD8, _colorMenu);
            Hypervisor.Write(0xBC9BDC, _colorMenu);

            Hypervisor.Write(0xBC9C68, _colorEmboss);
            Hypervisor.Write(0xBC9C6C, _colorEmboss);

            Hypervisor.Write(0xBC9FC8, _colorMenu);
            Hypervisor.Write(0xBC9FCC, _colorMenu);

            Hypervisor.Write(0xBCA058, _colorEmboss);
            Hypervisor.Write(0xBCA05C, _colorEmboss);

            Hypervisor.Write(0xBCA3B8, _colorMenu);
            Hypervisor.Write(0xBCA3BC, _colorMenu);

            Hypervisor.Write(0xBCA448, _colorEmboss);
            Hypervisor.Write(0xBCA44C, _colorEmboss);

            Hypervisor.Write(0xBCA8C8, _colorLabel);
            Hypervisor.Write(0xBCA8CC, _colorLabel);

            Hypervisor.Write(0xBCA9E8, _colorLabel);
            Hypervisor.Write(0xBCA9EC, _colorLabel);

            Hypervisor.Write(0xBCAB98, _colorMenu);
            Hypervisor.Write(0xBCAB9C, _colorMenu);

            Hypervisor.Write(0xBCAC28, _colorEmboss);
            Hypervisor.Write(0xBCAC2C, _colorEmboss);

            Hypervisor.Write(0xBCAEF8, _colorMenu);
            Hypervisor.Write(0xBCAEFC, _colorMenu);

            Hypervisor.Write(0xBCAF88, _colorEmboss);
            Hypervisor.Write(0xBCAF8C, _colorEmboss);

            Hypervisor.Write(0xBCB258, _colorMenu);
            Hypervisor.Write(0xBCB25C, _colorMenu);

            Hypervisor.Write(0xBCB2E8, _colorEmboss);
            Hypervisor.Write(0xBCB2EC, _colorEmboss);

            Hypervisor.Write(0xBCB528, _colorLabel);
            Hypervisor.Write(0xBCB52C, _colorLabel);

            Hypervisor.Write(0xBD0358, _colorLabel);
            Hypervisor.Write(0xBD035C, _colorLabel);

            Hypervisor.Write(0xBD0478, _colorLabel);
            Hypervisor.Write(0xBD047C, _colorLabel);

            Hypervisor.Write(0xBD0598, _colorLabel);
            Hypervisor.Write(0xBD059C, _colorLabel);

            Hypervisor.Write(0xBD1F7B, _colorOpacity);
            Hypervisor.Write(0xBD200B, _colorOpacity);

            Hypervisor.Write(0xBD209B, _colorOpacity);
            Hypervisor.Write(0xBD212B, _colorOpacity);

            Hypervisor.Write(0xBD21BB, _colorOpacity);
            Hypervisor.Write(0xBD224B, _colorOpacity);
        }

        public static void SwitchLogic()
        {
            var _soraAction = Hypervisor.GetPointer64(0x0718CB0, [0x170, -0x0C]);

            var _soraForm = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x3524);
            var _formCheck = _soraForm == 0x01 || _soraForm == 0x05;

            var _actionFetch = Encoding.ASCII.GetString(Hypervisor.Read<byte>(_soraAction, 0x04, true));

            var _actionCheck = _actionFetch != "A346"
                            && _actionFetch != "A314" && _actionFetch != "A315" && _actionFetch != "A316"
                            && _actionFetch != "A360" && _actionFetch != "A361" && _actionFetch != "A362" && _actionFetch != "A363" && _actionFetch != "A364" && _actionFetch != "A365"
                            && _actionFetch != "A370" && _actionFetch != "A371" && _actionFetch != "A372" && _actionFetch != "A373" && _actionFetch != "A374" && _actionFetch != "A375"
                            && _actionFetch != "B303" && _actionFetch != "B332"
                            && _actionFetch != "E300" && _actionFetch != "E302" && _actionFetch != "E303" && _actionFetch != "E332";

            var _formButtonCheck = Variables.IS_PRESSED(Variables.BUTTON.RIGHT | Variables.BUTTON.L2) && !DEBOUNCE;
            var _baseButtonCheck = Variables.IS_PRESSED(Variables.BUTTON.RIGHT) && !Variables.IS_PRESSED(Variables.BUTTON.L2) && !DEBOUNCE;

            var _gameStateCheck = !Variables.IS_CUTSCENE && !Variables.IS_EVENT && !Variables.IS_TITLE && Variables.IS_LOADED && !Variables.IS_PAUSED;

            if (_actionCheck && (_baseButtonCheck || _formButtonCheck) && _gameStateCheck)
            {
                var _changingMain = _baseButtonCheck;

            KEY_INIT:

                var _keyDictionary = Hypervisor.Read<short>(Variables.MemoryKH["KEYBLADE_SWITCH"], 0x03, true);

                if (_keyDictionary.All(x => x == 0x0310))
                    return;

                DEBOUNCE = true;

                var _fetchKey = _keyDictionary[_changingMain ? CURRENT_BASE : CURRENT_FORM];
                var _mainKey = Hypervisor.Read<short>(Variables.ADDR_SaveData + 0x24F0);
                var _formKey = Hypervisor.Read<short>(Variables.ADDR_SaveData + 0x32BCU + 0x38U * _soraForm);

                if (_fetchKey == 0x0310)
                    goto KEY_INIT;

                Sound.PlayVSB("se/zz00_keyswitch.win32.scd");

                Terminal.Log("Switching to Keyblade ID: 0x" + _fetchKey.ToString("X4"), 0);
                IO.CreateTASKMGR();

                Terminal.Log("Initializing the Weapon Changer...", 1);
                Variables.SharpHook[0x36ACC0].Execute();
                Variables.SharpHook[0x36AFA0].ExecuteJMP(BSharpConvention.MicrosoftX64, 0x01, _changingMain ? 0x00 : 0x01, _fetchKey, 0x00);
                Variables.SharpHook[0x36B210].Execute();

                Terminal.Log("Starting the Weapon Changer Task...", 1);
                var _taskFunction = Hypervisor.GetPointer64(Variables.ADDR_TaskManager, [0x10, 0x00]);
                Variables.SharpHook[0x36AF80].ExecuteJMP(BSharpConvention.MicrosoftX64, _taskFunction);

                Terminal.Log("Playing the Keyblade PAX...", 1);

                var _paxPointer = Hypervisor.GetPointer64(0x0AC1CC0, [_changingMain ? 0x5B0 : 0x5B8, 0x00]);
                Effect.PlayFromPAX(_paxPointer, 0x00);

                IO.FreeTASKMGR();

                Terminal.Log("Setting the current Keyblade as the Keyblade...", 1);
                Hypervisor.Write(_changingMain ? Variables.ADDR_SaveData + 0x24F0 : Variables.ADDR_SaveData + 0x32BCU + 0x38U * _soraForm, _fetchKey);

                Terminal.Log("Committing the Swap...", 1);
                var _offsetKey = 0x02 * (_changingMain ? CURRENT_BASE : CURRENT_FORM);
                Hypervisor.Write(Variables.MemoryKH["KEYBLADE_SWITCH"] + (ushort)_offsetKey, _changingMain ? _mainKey : _formKey, true);

                var _playerStats = Hypervisor.PureAddress + Variables.ADDR_PlayerStats;

                Terminal.Log("Submitting changes to Sora's Stats...", 1);
                Variables.SharpHook[0x4016B0].ExecuteJMP(BSharpConvention.MicrosoftX64, _playerStats + 0x1D0, _playerStats);

                if (!_changingMain)
                    Variables.SharpHook[0x3C1190].ExecuteJMP(BSharpConvention.MicrosoftX64, _playerStats, _mainKey);

                Variables.SharpHook[0x3C1190].ExecuteJMP(BSharpConvention.MicrosoftX64, _playerStats, _fetchKey);
                Variables.SharpHook[0x4014A0].Execute(_playerStats + 0x1D0);

                Variables.SharpHook[0x36AC70].Execute();

                if (_changingMain)
                {
                    CURRENT_BASE++;

                    if (CURRENT_BASE >= _keyDictionary.Length)
                        CURRENT_BASE = 0x00;
                }

                else
                {
                    CURRENT_FORM++;

                    if (CURRENT_FORM >= _keyDictionary.Length)
                        CURRENT_FORM = 0x00;
                }

                Terminal.Log("Done!\n", 0);
            }

            if (!Variables.IS_PRESSED(Variables.BUTTON.RIGHT) && DEBOUNCE)
                DEBOUNCE = false;
        }

        public static void Main(string[] args)
        {
            Terminal.Log("Initalizing Keyblade Switcher v1.10 by TopazTK...", 0);

            Terminal.Log("Initializing Kingdom Hearts II - Flexible Modding Library...", 1);
            Entry.Initialize(Process.GetProcessesByName("KINGDOM HEARTS II FINAL MIX")[0]);

            Terminal.Log("Allocating memory for use...", 1);

            Variables.MemoryKH.Allocate("KEYBLADE_DRIVES", 0x10);
            Variables.MemoryKH.Allocate("KEYBLADE_SWITCH", 0x10);

            Task.Run(() =>
            {
                while (true)
                {
                    if (!Variables.IS_TITLE && Variables.IS_LOADED)
                    {
                        var _readForm = Hypervisor.Read<ushort>(Variables.MemoryKH["KEYBLADE_DRIVES"], true);
                        var _readSwitch = Hypervisor.Read<ushort>(Variables.MemoryKH["KEYBLADE_SWITCH"], true);

                        if (_readSwitch == 0x0000)
                        {
                            Terminal.Log("Reading Switch Keyblade Data...", 1);

                            for (uint i = 0; i < 3; i++)
                            {
                                var _offsetKey = Variables.ADDR_SaveData + 0xE400 + (i * 0x02);
                                var _readKey = Hypervisor.Read<ushort>(_offsetKey);

                                Hypervisor.Write(Variables.MemoryKH["KEYBLADE_SWITCH"] + (i * 0x02), _readKey == 0x00 ? 0x0310 : _readKey, true);
                            }
                        }

                        if (_readForm == 0x0000)
                        {
                            Terminal.Log("Reading Form Keyblade Data...", 1);

                            var _offsetList = new uint[] { 0x01, 0x04, 0x05 };

                            for (uint i = 0; i < 3; i++)
                            {
                                var _offsetKey = Variables.ADDR_SaveData + 0x32BC + (_offsetList[i] * 0x38);
                                var _readKey = Hypervisor.Read<ushort>(_offsetKey);

                                Hypervisor.Write(Variables.MemoryKH["KEYBLADE_DRIVES"] + (i * 0x02), _readKey == 0x00 ? 0x0310 : _readKey, true);
                            }
                        }
                    }

                    else if (Variables.IS_TITLE)
                    {
                        var _readForm = Hypervisor.Read<ushort>(Variables.MemoryKH["KEYBLADE_DRIVES"], true);
                        var _readSwitch = Hypervisor.Read<ushort>(Variables.MemoryKH["KEYBLADE_SWITCH"], true);

                        if (_readForm != 0x0000 || _readSwitch != 0x0000)
                        {
                            Terminal.Log("Resetting all Keyblade Data...", 1);

                            Hypervisor.Write(Variables.MemoryKH["KEYBLADE_DRIVES"], new ushort[0x04], true);
                            Hypervisor.Write(Variables.MemoryKH["KEYBLADE_SWITCH"], new ushort[0x04], true);
                        }
                    }
                }
            });

            if (TEXT_ACTUAL == null)
            {
                TEXT_ACTUAL = Text.GetStringLiteral(0x5756);
                TEXT_POINTER = Text.GetStringPointer(0x5756);
            }

            Terminal.Log("Initialization complete! You can now start using the trainer!", 0);
            Terminal.Log("Press RIGHT to switch the Main Keyblade, press L2 + RIGHT to switch the Form Keyblade!\n", 0);

            Task.Run(() =>
            {
                while (true)
                { MenuLogic(); }
            });

            Task.Run(() =>
            {
                while (true)
                { SwitchLogic(); }
            });

            Console.ReadLine();
        }
    }
}
