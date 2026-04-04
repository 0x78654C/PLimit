using System.Runtime.InteropServices;

namespace PLimit.Utils
{
    internal sealed class SystemMonitorPanel : Panel
    {
        // ── NtQuerySystemInformation (per-core CPU) ───────────────────────
        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION
        {
            public long IdleTime;
            public long KernelTime; // includes IdleTime
            public long UserTime;
            public long DpcTime;
            public long InterruptTime;
            public uint InterruptCount;
        }

        [DllImport("ntdll.dll")]
        private static extern int NtQuerySystemInformation(
            int SystemInformationClass,
            IntPtr SystemInformation,
            int SystemInformationLength,
            out int ReturnLength);

        private const int SystemProcessorPerformanceInformation = 8;

        // ── GlobalMemoryStatusEx (RAM) ────────────────────────────────────
        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll")]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        // ── State ─────────────────────────────────────────────────────────
        private readonly int _coreCount;
        private readonly float[] _cpuValues;

        private long[] _prevKernel;
        private long[] _prevUser;
        private long[] _prevIdle;

        private float _memLoadPct;
        private ulong _usedMemMb;
        private ulong _totalMemMb;

        private readonly System.Windows.Forms.Timer _timer;

        // ── Theme ─────────────────────────────────────────────────────────
        private static readonly Color PanelBack    = Color.FromArgb(22, 22, 22);
        private static readonly Color BarBg        = Color.FromArgb(50, 50, 50);
        private static readonly Color CpuBarNormal = Color.FromArgb(30, 180, 100);
        private static readonly Color CpuBarHigh   = Color.FromArgb(220, 70, 50);
        private static readonly Color RamBarNormal = Color.FromArgb(60, 130, 220);
        private static readonly Color RamBarHigh   = Color.FromArgb(220, 70, 50);
        private static readonly Color TextColor    = Color.FromArgb(190, 190, 190);
        private static readonly Color SectionColor = Color.FromArgb(130, 130, 130);

        public SystemMonitorPanel()
        {
            _coreCount = Environment.ProcessorCount;
            _cpuValues = new float[_coreCount];
            _prevKernel = new long[_coreCount];
            _prevUser   = new long[_coreCount];
            _prevIdle   = new long[_coreCount];

            BackColor = PanelBack;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            // Prime first sample (delta needs a baseline)
            SampleCpu(prime: true);
            SampleMemory();

            _timer = new System.Windows.Forms.Timer { Interval = 1000 };
            _timer.Tick += (_, _) => { SampleCpu(); SampleMemory(); Invalidate(); };
            _timer.Start();
        }

        // ── Sampling ──────────────────────────────────────────────────────

        private void SampleCpu(bool prime = false)
        {
            int structSize = Marshal.SizeOf<SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION>();
            int bufSize    = structSize * _coreCount;
            IntPtr buf     = Marshal.AllocHGlobal(bufSize);

            try
            {
                if (NtQuerySystemInformation(SystemProcessorPerformanceInformation, buf, bufSize, out _) != 0)
                    return;

                for (int i = 0; i < _coreCount; i++)
                {
                    var info = Marshal.PtrToStructure<SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION>(
                        buf + i * structSize);

                    if (!prime)
                    {
                        long kernelDelta = info.KernelTime - _prevKernel[i];
                        long userDelta   = info.UserTime   - _prevUser[i];
                        long idleDelta   = info.IdleTime   - _prevIdle[i];
                        long totalDelta  = kernelDelta + userDelta;

                        _cpuValues[i] = totalDelta > 0
                            ? Math.Clamp((1f - (float)idleDelta / totalDelta) * 100f, 0, 100)
                            : 0;
                    }

                    _prevKernel[i] = info.KernelTime;
                    _prevUser[i]   = info.UserTime;
                    _prevIdle[i]   = info.IdleTime;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buf);
            }
        }

        private void SampleMemory()
        {
            var mem = new MEMORYSTATUSEX { dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>() };
            if (!GlobalMemoryStatusEx(ref mem)) return;

            _memLoadPct  = mem.dwMemoryLoad;
            _totalMemMb  = mem.ullTotalPhys  / 1024 / 1024;
            _usedMemMb   = _totalMemMb - mem.ullAvailPhys / 1024 / 1024;
        }

        // ── Painting ──────────────────────────────────────────────────────

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;

            const int padX       = 8;
            const int padY       = 6;
            const int gap        = 3;    // gap between bars
            const int sectionGap = 14;   // gap between CPU group and RAM bar
            const int labelH     = 14;   // height reserved for bottom labels
            const int headerH    = 14;   // height reserved for top section labels

            using var labelFont  = new Font("Segoe UI", 6.8f);
            using var headerFont = new Font("Segoe UI", 7f, FontStyle.Bold);
            using var pctFont    = new Font("Segoe UI", 6.5f);

            int totalItems = _coreCount + 1; // cores + 1 RAM bar
            int availW     = Width - padX * 2 - sectionGap;
            int barW       = Math.Max(6, (availW - gap * (totalItems - 1)) / totalItems);
            int barH       = Height - padY * 2 - headerH - labelH;
            int barTop     = padY + headerH;

            using var barBgBrush = new SolidBrush(BarBg);
            using var textBrush  = new SolidBrush(TextColor);
            using var sectBrush  = new SolidBrush(SectionColor);

            // ── Section headers ──────────────────────────────────────────
            g.DrawString("CPU Cores", headerFont, sectBrush, padX, padY);

            int ramBarX = padX + _coreCount * (barW + gap) + sectionGap;
            g.DrawString("RAM", headerFont, sectBrush, ramBarX, padY);

            // ── CPU bars ─────────────────────────────────────────────────
            for (int i = 0; i < _coreCount; i++)
            {
                int x    = padX + i * (barW + gap);
                float pct = _cpuValues[i] / 100f;
                int fillH = (int)(barH * pct);

                g.FillRectangle(barBgBrush, x, barTop, barW, barH);

                if (fillH > 0)
                {
                    using var fill = new SolidBrush(pct >= 0.9f ? CpuBarHigh : CpuBarNormal);
                    g.FillRectangle(fill, x, barTop + barH - fillH, barW, fillH);
                }

                // Percentage overlay (top of bar)
                string pctStr = $"{_cpuValues[i]:F0}%";
                var pctSz = g.MeasureString(pctStr, pctFont);
                if (pctSz.Width <= barW + 2)
                {
                    using var whiteBrush = new SolidBrush(Color.White);
                    g.DrawString(pctStr, pctFont, whiteBrush,
                        x + (barW - pctSz.Width) / 2f, barTop + 2);
                }

                // Core index label (below bar)
                string coreLabel = i.ToString();
                var cSz = g.MeasureString(coreLabel, labelFont);
                g.DrawString(coreLabel, labelFont, textBrush,
                    x + (barW - cSz.Width) / 2f, barTop + barH + 2);
            }

            // ── RAM bar ──────────────────────────────────────────────────
            float memPct  = _memLoadPct / 100f;
            int   memFillH = (int)(barH * memPct);

            g.FillRectangle(barBgBrush, ramBarX, barTop, barW, barH);

            if (memFillH > 0)
            {
                using var fill = new SolidBrush(memPct >= 0.85f ? RamBarHigh : RamBarNormal);
                g.FillRectangle(fill, ramBarX, barTop + barH - memFillH, barW, memFillH);
            }

            // Percentage overlay
            string memPctStr = $"{_memLoadPct:F0}%";
            var mPctSz = g.MeasureString(memPctStr, pctFont);
            if (mPctSz.Width <= barW + 2)
            {
                using var whiteBrush = new SolidBrush(Color.White);
                g.DrawString(memPctStr, pctFont, whiteBrush,
                    ramBarX + (barW - mPctSz.Width) / 2f, barTop + 2);
            }

            // Used/Total label (below bar)
            string memLabel = _totalMemMb >= 1024
                ? $"{_usedMemMb / 1024.0:F1}/{_totalMemMb / 1024.0:F0}GB"
                : $"{_usedMemMb}/{_totalMemMb}MB";
            g.DrawString(memLabel, labelFont, textBrush, ramBarX, barTop + barH + 2);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Stop();
                _timer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
