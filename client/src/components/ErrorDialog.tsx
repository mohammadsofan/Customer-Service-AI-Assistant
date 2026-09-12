import { Button } from './Button';
import { AlertCircle, X } from 'lucide-react';

export interface ErrorDialogProps {
  isOpen: boolean;
  onClose: () => void;
  title?: string;
  message: string;
  errors?: string[];
  buttonText?: string;
}

export function ErrorDialog({
  isOpen,
  onClose,
  title = 'حدث خطأ أثناء الحفظ',
  message,
  errors,
  buttonText = 'حسناً',
}: ErrorDialogProps) {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-xs p-4 transition-all duration-200" dir="rtl">
      <div 
        className="w-full max-w-md rounded-2xl bg-white p-6 shadow-2xl border border-red-100 transform transition-all animate-in fade-in zoom-in-95 duration-150"
        role="dialog"
        aria-modal="true"
      >
        <div className="flex items-start justify-between pb-3 border-b border-gray-100">
          <div className="flex items-center gap-3">
            <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-full bg-red-100 text-red-600">
              <AlertCircle className="h-6 w-6" />
            </div>
            <div>
              <h3 className="text-lg font-bold text-gray-900">{title}</h3>
              <p className="text-xs text-gray-500">يرجى مراجعة التفاصيل أدناه</p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="rounded-full p-1.5 text-gray-400 hover:bg-gray-100 hover:text-gray-600 transition"
            aria-label="إغلاق"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        <div className="py-4">
          <p className="text-sm font-medium text-gray-700 leading-relaxed">{message}</p>

          {errors && errors.length > 0 && (
            <div className="mt-3 rounded-xl bg-red-50 p-3.5 border border-red-100">
              <h4 className="text-xs font-bold text-red-800 mb-1.5">الأخطاء المكتشفة:</h4>
              <ul className="space-y-1 text-xs text-red-700 list-disc list-inside">
                {errors.map((err, idx) => (
                  <li key={idx} className="leading-snug">{err}</li>
                ))}
              </ul>
            </div>
          )}
        </div>

        <div className="pt-2 flex justify-end">
          <Button variant="danger" onClick={onClose} className="px-6 py-2 shadow-xs">
            {buttonText}
          </Button>
        </div>
      </div>
    </div>
  );
}
