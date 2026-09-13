
import { cn } from '../lib/utils';

export interface BadgeProps extends React.HTMLAttributes<HTMLSpanElement> {
  variant?: 'default' | 'success' | 'warning' | 'error' | 'info';
}

export function Badge({ className, variant = 'default', children, ...props }: BadgeProps) {
  const variants = {
    default: 'bg-slate-100 text-slate-700 border border-slate-200',
    success: 'bg-[#76bc21]/15 text-[#3b680c] border border-[#76bc21]/30',
    warning: 'bg-[#f4771d]/15 text-[#b34f07] border border-[#f4771d]/30',
    error: 'bg-rose-50 text-rose-700 border border-rose-200',
    info: 'bg-[#0055b8]/10 text-[#0055b8] border border-[#0055b8]/25',
  };

  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold',
        variants[variant],
        className
      )}
      {...props}
    >
      {children}
    </span>
  );
}
