
import { Search } from 'lucide-react';

export interface EmptyStateProps {
  title?: string;
  description?: string;
}

export function EmptyState({
  title = 'لا توجد نتائج',
  description = 'لم نتمكن من العثور على أي بيانات مطابقة.',
}: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center p-8 text-center" dir="rtl">
      <div className="mb-4 rounded-full bg-gray-100 p-3 text-gray-400">
        <Search className="h-8 w-8" />
      </div>
      <h3 className="mb-1 text-lg font-medium text-gray-900">{title}</h3>
      <p className="text-gray-500">{description}</p>
    </div>
  );
}
