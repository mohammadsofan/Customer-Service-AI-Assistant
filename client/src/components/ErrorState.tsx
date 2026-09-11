
import { AlertCircle } from 'lucide-react';
import { Button } from './Button';

export interface ErrorStateProps {
  title?: string;
  message?: string;
  onRetry?: () => void;
}

export function ErrorState({
  title = 'حدث خطأ',
  message = 'حدث خطأ غير متوقع أثناء تحميل البيانات. يرجى المحاولة مرة أخرى.',
  onRetry,
}: ErrorStateProps) {
  return (
    <div className="flex flex-col items-center justify-center p-8 text-center" dir="rtl">
      <div className="mb-4 rounded-full bg-red-100 p-3 text-red-500">
        <AlertCircle className="h-8 w-8" />
      </div>
      <h3 className="mb-1 text-lg font-medium text-gray-900">{title}</h3>
      <p className="mb-4 text-gray-500 max-w-md">{message}</p>
      {onRetry && (
        <Button onClick={onRetry} variant="outline">
          إعادة المحاولة
        </Button>
      )}
    </div>
  );
}
