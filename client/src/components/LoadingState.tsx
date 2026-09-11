
import { Spinner } from './Spinner';

export interface LoadingStateProps {
  text?: string;
}

export function LoadingState({ text = 'جاري التحميل...' }: LoadingStateProps) {
  return (
    <div className="flex flex-col items-center justify-center p-8 text-gray-500" dir="rtl">
      <Spinner className="mb-4 h-8 w-8 text-blue-500" />
      <p>{text}</p>
    </div>
  );
}
