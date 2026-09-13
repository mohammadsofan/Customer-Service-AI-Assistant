import React from 'react';
import { cn } from '../lib/utils';

export interface BrandLogoProps {
  size?: 'sm' | 'md' | 'lg' | 'xl';
  showText?: boolean;
  textLight?: boolean;
  subtitle?: string;
  className?: string;
}

export const BrandLogo: React.FC<BrandLogoProps> = ({
  size = 'md',
  showText = true,
  textLight = false,
  subtitle = 'مساعد خدمة العملاء الذكي',
  className,
}) => {
  const sizeMap = {
    sm: { img: 'w-7 h-7 rounded-lg', title: 'text-sm', sub: 'text-[10px]' },
    md: { img: 'w-10 h-10 rounded-xl', title: 'text-base font-bold', sub: 'text-xs' },
    lg: { img: 'w-14 h-14 rounded-2xl', title: 'text-xl font-extrabold', sub: 'text-sm' },
    xl: { img: 'w-20 h-20 rounded-3xl', title: 'text-2xl font-black', sub: 'text-base' },
  };

  const current = sizeMap[size];

  return (
    <div className={cn('flex items-center gap-3', className)} dir="rtl">
      <div
        className={cn(
          'relative shrink-0 overflow-hidden flex items-center justify-center shadow-md bg-white border border-slate-100',
          current.img
        )}
      >
        <img
          src="/brand/emblem.png"
          alt="Brand Emblem"
          className="w-full h-full object-cover"
        />
      </div>

      {showText && (
        <div className="flex flex-col text-right leading-tight">
          <span
            className={cn(
              current.title,
              textLight ? 'text-white' : 'text-slate-900',
              'tracking-tight'
            )}
          >
            مساعد الدعم الذكي
          </span>
          {subtitle && (
            <span
              className={cn(
                current.sub,
                textLight ? 'text-[#76bc21] font-medium' : 'text-slate-500 font-normal',
                'mt-0.5'
              )}
            >
              {subtitle}
            </span>
          )}
        </div>
      )}
    </div>
  );
};
