using InfinityScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AntiKnife
{
    public class AntiKnife
    {
        private int ProcessID = System.Diagnostics.Process.GetCurrentProcess().Id;

        string KnifeFolder;

        private int DefaultKnifeAddress;
        private unsafe int* KnifeRange;
        private unsafe int* ZeroAddress;

        bool _knifeEnabled = true;
        bool KnifeEnabled
        {
            get
            {
                return _knifeEnabled;
            }
            set
            {
                switch (value)
                {
                    case true:
                        EnableKnife();
                        _knifeEnabled = true;
                        break;
                    case false:
                        DisableKnife();
                        _knifeEnabled = false;
                        break;
                }
            }
        }

        public unsafe void SetupKnife()
        {
            KnifeFolder = System.IO.Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "scripts"), "AntiKnife");
            Directory.CreateDirectory(KnifeFolder);

            try
            {
                #region search1
                byte?[] search1 = new byte?[]
                {
                  139,
                  new byte?(),
                  new byte?(),
                  new byte?(),
                  131,
                  new byte?(),
                  4,
                  new byte?(),
                  131,
                  new byte?(),
                  12,
                  217,
                  new byte?(),
                  new byte?(),
                  new byte?(),
                  139,
                  new byte?(),
                  217,
                  new byte?(),
                  new byte?(),
                  new byte?(),
                  5,
                };
                #endregion
                KnifeRange = (int*)(FindMem(search1, 1, 4194304, 5242880) + search1.Length);

                if ((int)KnifeRange == search1.Length)
                {
                    #region search2
                    byte?[] search2 = new byte?[]
                    {
                        139,
                        new byte?(),
                        new byte?(),
                        new byte?(),
                        131,
                        new byte?(),
                        24,
                        new byte?(),
                        131,
                        new byte?(),
                        12,
                        217,
                        new byte?(),
                        new byte?(),
                        new byte?(),
                        141,
                        new byte?(),
                        new byte?(),
                        new byte?(),
                        217,
                        new byte?(),
                        new byte?(),
                        new byte?(),
                        217,
                        5,
                    };
                    #endregion
                    KnifeRange = (int*)(FindMem(search2, 1, 4194304, 5242880) + search2.Length);
                    if ((int)KnifeRange == search2.Length)
                        KnifeRange = null;
                }
                DefaultKnifeAddress = *KnifeRange;
                #region search3
                byte?[] search3 = new byte?[]
                {
                  217,
                  92,
                  new byte?(),
                  new byte?(),
                  216,
                  new byte?(),
                  new byte?(),
                  216,
                  new byte?(),
                  new byte?(),
                  217,
                  92,
                  new byte?(),
                  new byte?(),
                  131,
                  new byte?(),
                  1,
                  15,
                  134,
                  new byte?(),
                  0,
                  0,
                  0,
                  217,
                };
                #endregion
                ZeroAddress = (int*)(FindMem(search3, 1, 4194304, 5242880) + search3.Length + 2);

                if (!((int)KnifeRange != 0 && DefaultKnifeAddress != 0 && (int)ZeroAddress != 0))
                    Log.Debug("Error finding address: NoKnife Plugin will not work");
            }
            catch (Exception ex)
            {
                Log.Debug("Error in NoKnife Plugin. Plugin will not work.");
                Log.Debug(ex.ToString());
            }

            if (DefaultKnifeAddress == (int)ZeroAddress)
            {
                if (!File.Exists(KnifeFolder + @"\addr_" + ProcessID))
                {
                    Log.Debug("Error: NoKnife will not work.");
                    return;
                }

                DefaultKnifeAddress = int.Parse(File.ReadAllText(KnifeFolder + @"\addr_" + ProcessID));

            }
            else
            {
                File.WriteAllText(KnifeFolder + @"\addr_" + ProcessID, DefaultKnifeAddress.ToString());
            }
        }

        public unsafe void DisableKnife()
        {
            *KnifeRange = (int)ZeroAddress;
        }

        public unsafe void EnableKnife()
        {
            *KnifeRange = DefaultKnifeAddress;
        }

        private unsafe int FindMem(byte?[] search, int num = 1, int start = 16777216, int end = 63963136)
        {
            try
            {
                int num1 = 0;
                for (int index1 = start; index1 < end; ++index1)
                {
                    byte* numPtr = (byte*)index1;
                    bool flag = false;
                    for (int index2 = 0; index2 < search.Length; ++index2)
                    {
                        if (search[index2].HasValue)
                        {
                            int num2 = *numPtr;
                            byte? nullable = search[index2];
                            if ((num2 != nullable.GetValueOrDefault() ? 1 : (!nullable.HasValue ? 1 : 0)) != 0)
                                break;
                        }
                        if (index2 == search.Length - 1)
                        {
                            if (num == 1)
                            {
                                flag = true;
                            }
                            else
                            {
                                ++num1;
                                if (num1 == num)
                                    flag = true;
                            }
                        }
                        else
                            ++numPtr;
                    }
                    if (flag)
                        return index1;
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Error in Knife:" + ex.Message);
            }
            return 0;
        }
    }
}
