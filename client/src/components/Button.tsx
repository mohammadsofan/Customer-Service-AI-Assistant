import React from 'react';

import { cn } from '../lib/utils';
import { Loader2 } from 'lucide-react';

export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger' | 'outline' | 'ghost';
  size?: 'sm' | 'md' | 'lg' | 'icon';
  isLoading?: boolean;
}

export const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant = 'primary', size = 'md', isLoading, children, disabled, ...props }, ref) => {
    const variants = {
      primary: 'bg-[#76bc21] text-white hover:bg-[#67a61d] active:bg-[#5b9419] shadow-sm shadow-[#76bc21]/25 font-semibold focus:ring-[#76bc21]',
      secondary: 'bg-slate-100 text-slate-800 hover:bg-slate-200 border border-slate-200/80',
      danger: 'bg-rose-600 text-white hover:bg-rose-700 focus:ring-rose-500',
      outline: 'border border-slate-200 bg-white hover:border-[#76bc21]/60 hover:text-[#589015] hover:bg-[#76bc21]/5 text-slate-700 shadow-xs',
      ghost: 'bg-transparent hover:bg-slate-100 text-slate-700 hover:text-[#589015]',
    };

    const sizes = {
      sm: 'h-8 px-3 text-xs rounded-lg',
      md: 'h-10 px-4 py-2 rounded-xl',
      lg: 'h-12 px-6 text-base rounded-xl',
      icon: 'h-10 w-10 p-2 rounded-xl',
    };

    return (
      <button
        ref={ref}
        disabled={disabled || isLoading}
        className={cn(
          'inline-flex items-center justify-center text-sm font-medium transition-all duration-150 focus:outline-none focus:ring-2 focus:ring-[#76bc21] focus:ring-offset-2 disabled:opacity-50 disabled:pointer-events-none cursor-pointer',
          variants[variant],
          sizes[size],
          className
        )}
        {...props}
      >
        {isLoading && <Loader2 className="ml-2 h-4 w-4 animate-spin" />}
        {isLoading ? 'جاري التحميل...' : children}
      </button>
    );
  }
);
Button.displayName = 'Button';
