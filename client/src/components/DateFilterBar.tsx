import React, { useState } from 'react';
import { Calendar, RotateCcw, Filter, Check } from 'lucide-react';

export type DatePreset = 'all' | 'today' | 'yesterday' | 'last7days' | 'last30days' | 'custom';

export interface DateFilterRange {
  preset: DatePreset;
  startDate?: string;
  endDate?: string;
  label: string;
}

interface DateFilterBarProps {
  onFilterChange: (range: DateFilterRange) => void;
  className?: string;
}

export const DateFilterBar: React.FC<DateFilterBarProps> = ({ onFilterChange, className = '' }) => {
  const [selectedPreset, setSelectedPreset] = useState<DatePreset>('all');
  const [customStart, setCustomStart] = useState<string>('');
  const [customEnd, setCustomEnd] = useState<string>('');
  const [activeLabel, setActiveLabel] = useState<string>('جميع الأوقات');

  const computePresetRange = (preset: DatePreset): { startDate?: string; endDate?: string; label: string } => {
    const now = new Date();

    if (preset === 'today') {
      const start = new Date(now);
      start.setHours(0, 0, 0, 0);
      const end = new Date(now);
      end.setHours(23, 59, 59, 999);
      const dateStr = start.toLocaleDateString('ar-EG', { year: 'numeric', month: 'short', day: 'numeric' });
      return {
        startDate: start.toISOString(),
        endDate: end.toISOString(),
        label: `اليوم (${dateStr})`
      };
    }

    if (preset === 'yesterday') {
      const start = new Date(now);
      start.setDate(start.getDate() - 1);
      start.setHours(0, 0, 0, 0);
      const end = new Date(now);
      end.setDate(end.getDate() - 1);
      end.setHours(23, 59, 59, 999);
      const dateStr = start.toLocaleDateString('ar-EG', { year: 'numeric', month: 'short', day: 'numeric' });
      return {
        startDate: start.toISOString(),
        endDate: end.toISOString(),
        label: `أمس (${dateStr})`
      };
    }

    if (preset === 'last7days') {
      const start = new Date(now);
      start.setDate(start.getDate() - 6);
      start.setHours(0, 0, 0, 0);
      const end = new Date(now);
      end.setHours(23, 59, 59, 999);
      return {
        startDate: start.toISOString(),
        endDate: end.toISOString(),
        label: 'آخر 7 أيام'
      };
    }

    if (preset === 'last30days') {
      const start = new Date(now);
      start.setDate(start.getDate() - 29);
      start.setHours(0, 0, 0, 0);
      const end = new Date(now);
      end.setHours(23, 59, 59, 999);
      return {
        startDate: start.toISOString(),
        endDate: end.toISOString(),
        label: 'آخر 30 يوماً'
      };
    }

    return {
      startDate: undefined,
      endDate: undefined,
      label: 'جميع الأوقات'
    };
  };

  const handleSelectPreset = (preset: DatePreset) => {
    setSelectedPreset(preset);
    if (preset !== 'custom') {
      const { startDate, endDate, label } = computePresetRange(preset);
      setActiveLabel(label);
      onFilterChange({ preset, startDate, endDate, label });
    }
  };

  const handleApplyCustom = (e: React.FormEvent) => {
    e.preventDefault();
    if (!customStart || !customEnd) return;

    const start = new Date(`${customStart}T00:00:00`);
    const end = new Date(`${customEnd}T23:59:59.999`);

    const label = `من ${customStart} إلى ${customEnd}`;
    setActiveLabel(label);
    onFilterChange({
      preset: 'custom',
      startDate: start.toISOString(),
      endDate: end.toISOString(),
      label
    });
  };

  const handleReset = () => {
    setSelectedPreset('all');
    setCustomStart('');
    setCustomEnd('');
    setActiveLabel('جميع الأوقات');
    onFilterChange({
      preset: 'all',
      startDate: undefined,
      endDate: undefined,
      label: 'جميع الأوقات'
    });
  };

  const presetsList: { id: DatePreset; label: string }[] = [
    { id: 'all', label: 'جميع الأوقات' },
    { id: 'today', label: 'اليوم' },
    { id: 'yesterday', label: 'أمس' },
    { id: 'last7days', label: 'آخر 7 أيام' },
    { id: 'last30days', label: 'آخر 30 يوماً' },
    { id: 'custom', label: 'فترة مخصصة' }
  ];

  return (
    <div className={`bg-white rounded-2xl border border-slate-200/80 p-4 sm:p-5 shadow-xs space-y-4 ${className}`} dir="rtl">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-3">
        <div className="flex items-center gap-2.5">
          <div className="w-8 h-8 rounded-xl bg-blue-50 text-[#0055b8] flex items-center justify-center shrink-0">
            <Filter className="w-4 h-4" />
          </div>
          <div>
            <h4 className="text-sm font-bold text-slate-900">تصفية النتائج بحسب التاريخ</h4>
            <p className="text-xs text-slate-500">
              الفترة النشطة:{' '}
              <span className="font-bold text-[#0055b8]">{activeLabel}</span>
            </p>
          </div>
        </div>

        {selectedPreset !== 'all' && (
          <button
            type="button"
            onClick={handleReset}
            className="inline-flex items-center gap-1 text-xs text-slate-500 hover:text-slate-800 bg-slate-100 hover:bg-slate-200 px-3 py-1.5 rounded-lg font-medium transition-all cursor-pointer self-start md:self-center"
          >
            <RotateCcw className="w-3.5 h-3.5" />
            <span>إعادة التعيين للكل</span>
          </button>
        )}
      </div>

      {/* Preset Buttons */}
      <div className="flex flex-wrap items-center gap-2">
        {presetsList.map((p) => {
          const isActive = selectedPreset === p.id;
          return (
            <button
              key={p.id}
              type="button"
              onClick={() => handleSelectPreset(p.id)}
              className={`px-3.5 py-1.5 rounded-xl text-xs font-bold transition-all cursor-pointer flex items-center gap-1.5 ${
                isActive
                  ? 'bg-[#0055b8] text-white shadow-xs'
                  : 'bg-slate-100 text-slate-700 hover:bg-slate-200'
              }`}
            >
              {isActive && <Check className="w-3.5 h-3.5" />}
              <span>{p.label}</span>
            </button>
          );
        })}
      </div>

      {/* Custom Date Form (Shown when custom is selected) */}
      {selectedPreset === 'custom' && (
        <form onSubmit={handleApplyCustom} className="pt-3 border-t border-slate-100 flex flex-wrap items-center gap-3">
          <div className="flex items-center gap-2">
            <label className="text-xs font-semibold text-slate-600">من تاريخ:</label>
            <input
              type="date"
              required
              value={customStart}
              onChange={(e) => setCustomStart(e.target.value)}
              className="px-3 py-1.5 text-xs rounded-xl border border-slate-200 bg-white text-slate-800 focus:outline-none focus:ring-2 focus:ring-[#0055b8]/20 focus:border-[#0055b8]"
            />
          </div>

          <div className="flex items-center gap-2">
            <label className="text-xs font-semibold text-slate-600">إلى تاريخ:</label>
            <input
              type="date"
              required
              value={customEnd}
              onChange={(e) => setCustomEnd(e.target.value)}
              className="px-3 py-1.5 text-xs rounded-xl border border-slate-200 bg-white text-slate-800 focus:outline-none focus:ring-2 focus:ring-[#0055b8]/20 focus:border-[#0055b8]"
            />
          </div>

          <button
            type="submit"
            disabled={!customStart || !customEnd}
            className="px-4 py-1.5 rounded-xl bg-[#76bc21] hover:bg-[#68a61d] text-white text-xs font-bold transition-all disabled:opacity-50 cursor-pointer flex items-center gap-1.5 shadow-xs"
          >
            <Calendar className="w-3.5 h-3.5" />
            <span>تطبيق الفترة</span>
          </button>
        </form>
      )}
    </div>
  );
};
