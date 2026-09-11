
import { cn } from '../lib/utils';
import { AlertCircle, CheckCircle, Info, AlertTriangle } from 'lucide-react';

export interface AlertProps {
  type: 'info' | 'success' | 'warning' | 'error';
  title?: string;
  message: string;
  className?: string;
}

export function Alert({ type, title, message, className }: AlertProps) {
  const types = {
    info: {
      container: 'bg-blue-50 border-blue-200',
      icon: <Info className="h-5 w-5 text-blue-500" />,
      title: 'text-blue-800',
      message: 'text-blue-700',
    },
    success: {
      container: 'bg-green-50 border-green-200',
      icon: <CheckCircle className="h-5 w-5 text-green-500" />,
      title: 'text-green-800',
      message: 'text-green-700',
    },
    warning: {
      container: 'bg-yellow-50 border-yellow-200',
      icon: <AlertTriangle className="h-5 w-5 text-yellow-500" />,
      title: 'text-yellow-800',
      message: 'text-yellow-700',
    },
    error: {
      container: 'bg-red-50 border-red-200',
      icon: <AlertCircle className="h-5 w-5 text-red-500" />,
      title: 'text-red-800',
      message: 'text-red-700',
    },
  };

  const config = types[type];

  return (
    <div
      className={cn(
        'flex items-start gap-3 rounded-md border p-4',
        config.container,
        className
      )}
      dir="rtl"
    >
      <div className="shrink-0">{config.icon}</div>
      <div>
        {title && (
          <h3 className={cn('mb-1 font-medium', config.title)}>{title}</h3>
        )}
        <div className={cn('text-sm', config.message)}>{message}</div>
      </div>
    </div>
  );
}
