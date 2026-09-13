import React from 'react';

import { cn } from '../lib/utils';
import { Search, Loader2 } from 'lucide-react';

export interface SearchBoxProps extends React.InputHTMLAttributes<HTMLInputElement> {
  loading?: boolean;
}

export const SearchBox = React.forwardRef<HTMLInputElement, SearchBoxProps>(
  ({ className, loading = false, ...props }, ref) => {
    return (
      <div className="relative w-full" dir="rtl">
        <div className="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3">
          {loading ? (
            <Loader2 className="h-4 w-4 text-blue-500 animate-spin" />
          ) : (
            <Search className="h-4 w-4 text-gray-400" />
          )}
        </div>
        <input
          ref={ref}
          type="search"
          placeholder="بحث..."
          className={cn(
            'flex h-10 w-full rounded-md border border-gray-300 bg-white py-2 pl-10 pr-10 text-sm placeholder:text-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:cursor-not-allowed disabled:opacity-50',
            className
          )}
          {...props}
        />
      </div>
    );
  }
);
SearchBox.displayName = 'SearchBox';
